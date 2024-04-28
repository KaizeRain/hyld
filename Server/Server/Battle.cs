using Server.Controller;
using SocketProto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
	/// <summary>
	/// 用于开启战场的单例管理
	/// </summary>
    class BattleManage
    {
		private int battleID;
		private Dictionary<int, BattleController> dic_battles;
		private Dictionary<int, Controller.MatchingController.FightPattern> dic_pattern;
		private Dictionary<int, List<MatchUserInfo>> dic_matchUserInfo;
		private static BattleManage instance = null;
		private Server server;
		public static BattleManage Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new BattleManage();
				}
				return instance;
			}
		}

		private BattleManage()
		{
			battleID = 0;
			dic_pattern=new Dictionary<int, MatchingController.FightPattern>();
			dic_battles = new Dictionary<int, BattleController>();
			dic_matchUserInfo = new Dictionary<int, List<MatchUserInfo>>();
		}

		public void BeginBattle(Server server,List<MatchUserInfo> _battleUser, Controller.MatchingController.FightPattern fightPattern)
		{
			
			///1.5记录对局batlleID  创建对局
			if (this.server == null) this.server = server;
			battleID++;
			List<int> uids = new List<int>();
			foreach (var info in _battleUser)
			{
				uids.Add(info.uid);
				server.DIC_BattleIDs.Add(info.uid, battleID);
			}
			server.DIC_BattlePlayerUIDs.Add(battleID, uids);

			//生成战斗控制器
			BattleController _battle = new BattleController(server,battleID, _battleUser);
			dic_pattern[battleID] = fightPattern;
			dic_battles[battleID] = _battle;
			dic_matchUserInfo[battleID] = _battleUser;
			Logging.Debug.Log("开始战斗。。。。。BattleID：" + battleID);
		}


		public void FinishBattle(int _battleID, Dictionary<int, AllPlayerOperation> dic_match_frames)
		{
			dic_battles.Remove(_battleID);
			MainPack mainPack = new MainPack();
			mainPack.Actioncode = ActionCode.BattleReview;
			BattleInfo battleInfo = new BattleInfo();
			int userBattleID = 0;
			int playerCount = dic_matchUserInfo[_battleID].Count;
			for (int i = 0; i < playerCount; i++)
			{
				int _userUid = dic_matchUserInfo[_battleID][i].uid;
				userBattleID++;  // 为每个user设置一个battleID， 这里就从1开始。
								 //dic_clearbattleReady[_userUid] = false;
				//var _udp = new ClientUdp(_ip,_userUid,Handle);
				//dic_udp[userBattleID] = _udp;
				//包消息
				BattlePlayerPack _bUser = new BattlePlayerPack();
				_bUser.Id = _userUid;
				_bUser.Battleid = userBattleID;
				_bUser.Playername = dic_matchUserInfo[_battleID][i].userName;
				_bUser.Hero = dic_matchUserInfo[_battleID][i].hero;
				_bUser.Teamid = dic_matchUserInfo[_battleID][i].teamid;


				battleInfo.BattleUserInfo.Add(_bUser);
			}

			foreach (AllPlayerOperation allPlayerOperation in dic_match_frames.Values)
			{
				battleInfo.AllPlayerOperation.Add(allPlayerOperation);
			}
	
			mainPack.Str=((int)dic_pattern[battleID]).ToString();
			mainPack.BattleInfo = battleInfo;
			Console.WriteLine(mainPack);
			foreach (int uid in server.DIC_BattlePlayerUIDs[_battleID])
			{
				server.DIC_BattleIDs.Remove(uid);
				Client c = server.GetClientByID(uid);
				//发送所有帧操作。

				c.Send(mainPack);
			}
		
			server.DIC_BattlePlayerUIDs[_battleID].Clear();
			server.DIC_BattlePlayerUIDs.Remove(_battleID);
			Logging.Debug.Log("战斗结束。。。。。BattleID：" + _battleID);
		}
	}


	/// <summary>
	/// TODO:战场控制，是一个额外的线程(以后优化？)
	/// </summary>
	class BattleController
	{
		public int battleID { get; private set; }
		private Dictionary<int, int> dic_battleUserUid;//存储玩家的id
		private Dictionary<int, string> dic_battleid_ip_port = new Dictionary<int, string>();//存储玩家的ip端口，方便发送消息
		private int playerCount;//存储玩家数量

		/**************准备阶段************/
		private Dictionary<int, bool> dic_battleReady;//存储玩家是否准备好
		//private Dictionary<int, bool> dic_clearbattleReady;//存储玩家是否准备好
		//private bool isclearSenceOver = false;
		private bool isBeginBattle = false;
		/***************战斗阶段*******************/
		private int frameid;//1.服务器上每个比赛对象，都有一个成员的freamid，保存了当前比赛下一帧要进入的id
		//private int lastFrame;//存一下上一帧（为了游戏结束）
		private Dictionary<int, AllPlayerOperation> dic_match_frames;//2.用来保存我们所有玩家的每帧的操作。(录像回放，断线重连，不同步的情况，有无作弊，udp丢包时序问题，我们要补发给客户端的帧保存起来)
		private Dictionary<int, PlayerOperation> dic_next_frame_opts;//3.dic_next_frame_opts 每帧服务器将采集来的客户端的操作都存放在这里；(<battleid,opt>)
		private Dictionary<int, int> dic_next_opts_frameid;//玩家的帧id
		private static System.Object lockThis = new System.Object();//锁
		private bool _isRun = false;  //isRun判断是否需要一直传帧
		/***************结束阶段*****************/
		private Timer waitBattleFinish;
		private float finishTime;//结束倒计时
		private Dictionary<int, bool> dic_playerGameOver;//记录玩家游戏结束
		private bool oneGameOver;//有人结束了
		private bool allGameOver;//全部人都结束了
		public BattleController(Server server,int _battleID, List<MatchUserInfo> _battleUser)
		{
			//1.6生成随机种子
			//增加监听消息处理回调
			int randSeed = (new Random()).Next(0, 100);
			LZJUDP.Instance.AddListenRecv(Handle);



			//1.7发送给客户端对战的所有玩家信息
			ThreadPool.QueueUserWorkItem((obj) => {
				battleID = _battleID;
				dic_battleUserUid = new Dictionary<int, int>();
				dic_battleid_ip_port = new Dictionary<int, string>();
				dic_battleReady = new Dictionary<int, bool>();
				//dic_clearbattleReady = new Dictionary<int, bool>();
				int userBattleID = 0;
				MainPack pack = new MainPack();
				pack.Requestcode = RequestCode.Matching;
				pack.Returncode = ReturnCode.Succeed;
				pack.Actioncode = ActionCode.StartEnterBattle;
				BattleInfo _mes = new BattleInfo();
				_mes.RandSeed = randSeed;
				playerCount = _battleUser.Count;
				for (int i = 0; i < playerCount; i++)
				{
					int _userUid = _battleUser[i].uid;
					userBattleID++;  // 为每个user设置一个battleID， 这里就从1开始。
					//dic_clearbattleReady[_userUid] = false;
					dic_battleUserUid[_userUid] = userBattleID;
					//建立udp连接
					string _ip = _battleUser[i].socketIP;
					//var _udp = new ClientUdp(_ip,_userUid,Handle);

					//dic_udp[userBattleID] = _udp;
					dic_battleReady[userBattleID] = false;
					//包消息
					 BattlePlayerPack _bUser = new BattlePlayerPack();
					_bUser.Id = _userUid;
					_bUser.Battleid = userBattleID;
					_bUser.Playername = _battleUser[i].userName;
					_bUser.Hero = _battleUser[i].hero;
					_bUser.Teamid = _battleUser[i].teamid;
					

					_mes.BattleUserInfo.Add(_bUser);
				}

				pack.BattleInfo = _mes;
				Logging.Debug.Log("向客户端发送战场数据！" + pack);
				///tcp发送消息我准备好了！
				for (int i = 0; i < _battleUser.Count; i++)
				{
					int _userUid = _battleUser[i].uid;
					Client c = server.GetActiveClient(_userUid);
					c.Send(pack);
					//string _ip = UserManage.Instance.GetUserInfo(_userUid).socketIp;
					//ServerTcp.Instance.SendMessage(_ip, CSData.GetSendMessage<TcpEnterBattle>(_mes, SCID.TCP_ENTER_BATTLE));
				}
				isBeginBattle = false;
				//isclearSenceOver = false;
			}, null);
		}

		/*************战斗部分**************************/
		///开始战斗
		private void BeginBattle()
		{
			frameid = 1;
			//lastFrame = 0;
			_isRun = true;
			oneGameOver = false;
			allGameOver = false;

			int playerNum = dic_battleUserUid.Keys.Count;

			dic_match_frames = new Dictionary<int, AllPlayerOperation>();
			dic_next_frame_opts = new Dictionary<int, PlayerOperation>();
			dic_next_opts_frameid = new Dictionary<int, int>();
			dic_playerGameOver = new Dictionary<int, bool>();

			foreach (int id in dic_battleUserUid.Values)
			{
				//Logging.Debug.Log(" dic初始化  "+id);
				dic_next_frame_opts[id] = null;
				dic_next_opts_frameid[id] = 0;
				dic_playerGameOver[id] = false;
			}


			//4.服务器开启一个线程Thread_SendFrameData去等待所有玩家准备ok(收到第一帧操作)，收到第一帧之后游戏正式开始，之后每隔66ms(15FPS)触发一次sendFrameDate
			Thread _threadSenfd = new Thread(Thread_SendFrameData);
			//Logging.Debug.Log("开始战斗准备5s");
			//Thread.Sleep(10000);
			_threadSenfd.Start();
		}
		/// <summary>
		/// 战斗中。。。发送帧操作
		/// </summary>
		private void Thread_SendFrameData()
		{
		
			Logging.Debug.Log("开始战斗");

			bool isFinishBS = false;
			//1.11循环向客户端发送战斗开始，接受客户端帧消息
			//向所有客户端发送Fight_Start
			while (!isFinishBS)
			{
				MainPack pack  = new MainPack();
				pack.Requestcode = RequestCode.Battle;
				pack.Actioncode = ActionCode.BattleStart;
				pack.Str = "1";//防止空包
				foreach (var item in dic_battleid_ip_port)
				{
					//Logging.Debug.Log(item.Value+"   "+pack);
					//item.Value.Send(pack);
					LZJUDP.Instance.Send(pack, item.Value);
				}

				bool _allData = true;// (dic_next_frame_opts.Count== playerCount);
									 // 有一个玩家没有发送上来操作 则判断为false
				foreach (var op in dic_next_frame_opts.Values)
				{
					if (op == null)
					{
						_allData = false;// 有一个玩家没有发送上来操作 则判断为false
						break;
					}
				}
				//Logging.Debug.Log("kkkk " + _allData+"  "+ dic_next_frame_opts.Count);
				if (_allData)
				{
					Logging.Debug.Log("战斗服务器:收到全部玩家的第一次操作数据 ");
					frameid = 1;

					isFinishBS = true;
				}

				Thread.Sleep(500);
			}

			Logging.Debug.Log("开始发送帧数据 ");
			/*
			 1.15更新第一帧数据
			 */
			while (_isRun)
			{
				//dic_next_frame_opts每帧服务器将采集来的客户端的操作，都存放在这里；
				//dic_next_opts_frameid 存储每个玩家在服务器上的帧
				//启动一个定时器，每隔66ms(15FPS)触发一次sendFrameDate

				if (oneGameOver)
				{
					break;
					//游戏结束停止发送帧操作。
				}
				else
				{
					//5.触发一个帧操作，保存当前的帧操作到dic_match_frames
					AllPlayerOperation next_frame_opt = new AllPlayerOperation();



					try
					{
						for (int i = 1; i <= ServerConfig.MaxPlayerCount; i++)
						{
							if (dic_next_frame_opts.ContainsKey(i))
							{
								next_frame_opt.Operations.Add(dic_next_frame_opts[i]);
							}
						}/*
						foreach (var opt in dic_next_frame_opts.Values)
						{
							lock (lockThis)
								next_frame_opt.Operations.Add(opt);
						}*/
					}
					catch (Exception ex)
					{
						Logging.Debug.Log(ex);
						//Unhandled exception. System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
						//这个bug不知道怎么解决，认为是多线程访问Dic的问题，但是加了锁还是会寄。我是废物，所以干脆直接给个空帧
						next_frame_opt = new AllPlayerOperation();
					}

					//Logging.Debug.Log("触发一个帧操作，保存当前的帧操作到dic_match_frames :   frameid: "+frameid+"  opt:  " + next_frame_opt);
					next_frame_opt.Frameid = frameid;
					dic_match_frames.Add(frameid, next_frame_opt);
					//1.16 遍历每个玩家给每个玩家发送帧操作。
					//服务器进入下一帧self.frameid++
					//服务器清空上一帧操作next_frame_opt.allplayeroperation.clear()


					//6.遍历每个玩家给每个玩家发送帧操作。
					foreach (var item in dic_battleid_ip_port)
					{

						//7.发送服务器认为  玩家还没同步的帧，每个玩家对象都记录一个dic_next_opts_frameid,记录当前客户端已经同步到了多少帧
						//如果没游戏结束才发送
						if (!dic_playerGameOver[item.Key])
							send_unsync_frames(item.Value, item.Key);
					}

					// 9.服务器进入下一帧self.frameid++
					frameid++;
					// 10.服务器清空上一帧操作next_frame_opt.allplayeroperation.clear()

					lock (lockThis)
						dic_next_frame_opts.Clear();
				}

				Thread.Sleep(ServerConfig.frameTime);
			}

			Logging.Debug.Log("帧数据发送线程结束.....................");
		}


		/// <summary>
		/// 发送服务器认为  玩家还没同步的帧，每个玩家对象都记录一个dic_next_opts_frameid,记录当前客户端已经同步到了多少帧
		/// </summary>
		/// <param name="udp"></param>
		/// <param name="battleid"></param>
		private void send_unsync_frames(string ipport,int battleid)
		{
			MainPack pack = new MainPack();
			pack.Requestcode = RequestCode.Battle;
			pack.Actioncode = ActionCode.BattlePushDowmAllFrameOpeartions;
			BattleInfo battleInfo = new BattleInfo();
			//发送服务器认为  玩家还没同步的帧，每个玩家对象都记录一个dic_next_opts_frameid,记录当前客户端已经同步到了多少帧
			for (int i = dic_next_opts_frameid[battleid] + 1; i <= frameid; i++)
			{
				//Logging.Debug.Log(dic_match_frames[i]);
				battleInfo.AllPlayerOperation.Add(dic_match_frames[i]);
			}



			battleInfo.OperationID = frameid;
			pack.BattleInfo = battleInfo;
			//Logging.Debug.Log("发送服务器认为  玩家还没同步的帧:" + pack+"\n");
			//8.采用udp将数据包发送出去   可能会发送补发的帧
			LZJUDP.Instance.Send(pack,ipport);
		}
		/// <summary>
		/// 更新玩家操作
		/// </summary>
		/// <param name="_operation"></param>
		/// <param name="_sync_frameid"></param>
		public void UpdatePlayerOperation(PlayerOperation _operation, int _sync_frameid)
		{
			/*
			 1.14发送BattlePushDowmPlayerOpeartions请求
			发送操作和操作序列号
			更新服务端的dic_next_opts_frameid，dic_next_frame_opts
			 */
			// Debug.Log("收到玩家操作:" + _index + "," + _mesNum + "," + playerMesNum[_index]); // mesnum 客户端消息序列，   playuermesnum 服务器存储的玩家消息。
			//		Debug.Log ("收到玩家操作:" + _index + "," + _mesNum + "," + playerMesNum [_index]);


			//_sync_frameid-1  当前客户端更新到哪个帧id了
			//dic_next_opts_frameid[_operation.Battleid]是服务器记录的玩家的帧id
			//Logging.Debug.Log("dic_next_opts_frameid[_operation.Battleid] =  " + dic_next_opts_frameid[_operation.Battleid] + "   <  _sync_frameid-1  =" + (_sync_frameid - 1)+"        frameid =  "+frameid);
			//18.服务器收到数据。更新服务器的dic_next_opts_frameid
			if (dic_next_opts_frameid[_operation.Battleid] < _sync_frameid-1)
			{
				//更新到最新id
				dic_next_opts_frameid[_operation.Battleid] = _sync_frameid - 1;
			}
			//19.如果收到玩家操作帧id不等于马上要触发的帧id，说明玩家过时的操作。
			//frameid是服务器现在最新帧，保存了当前比赛下一帧要进入的id
			if (_sync_frameid != frameid)
			{
				//丢弃过时的包
				return;
			}

			//没啥问题的话，就把玩家操作插入到
			//Logging.Debug.Log($"没啥问题的话，就把玩家操作插入到");
			//20.把这个操作插入到next_frame_opt。等待下一帧处理。GOTO: 逻辑5
			lock (lockThis)
				dic_next_frame_opts[_operation.Battleid] = _operation;
			//Logging.Debug.Log($"没啥问题的话，就把玩家操作插入到  {_operation.Battleid}  {dic_next_frame_opts[_operation.Battleid]}");
		}

		/************ 处理玩家消息**********************************/
		public void Handle(MainPack pack)
		{
			//Logging.Debug.Log("Handle  " + pack);
			switch (pack.Actioncode)
			{
				case  ActionCode.BattleReady:

					//1.10检查是否所有玩家都发送了准备就绪请求
					//如果ok，则开始战斗
					//向玩家发起战斗开始请求
					//向玩家发送战斗开始
					//玩家发送”我准备好了“
					//Logging.Debug.Log("Handle UDP 玩家发送”我准备好了“  " + pack + "   " + isBeginBattle);
					if (isBeginBattle) return;
					dic_battleReady[pack.Battleplayerpack[0].Battleid]=true;
					isBeginBattle = true;
					foreach (var item in dic_battleReady.Values)
					{
						isBeginBattle = (isBeginBattle && item);
					}
					dic_battleid_ip_port[pack.Battleplayerpack[0].Battleid] = pack.Str;
					//dic_udp[pack.Battleplayerpack[0].Battleid].RecvClientReady(pack.Battleplayerpack[0].Id);
					//Logging.Debug.Log("isBeginBattle = " + isBeginBattle);
					if (isBeginBattle)
					{
						//开始战斗
						BeginBattle();
					}

					break;

				case ActionCode.BattlePushDowmPlayerOpeartions:
					if (!isBeginBattle) return;
					//Logging.Debug.Log("Handle UDP 玩家发送“玩家认为的”帧消息“  " + pack + "   " + isBeginBattle);
					//玩家发送“玩家认为的”帧消息
					/*
					 1.14发送BattlePushDowmPlayerOpeartions请求
					发送操作和操作序列号
					更新服务端的dic_next_opts_frameid，dic_next_frame_opts
					 */
					BattleInfo battleInfo = pack.BattleInfo;
					int _sync_frameid = battleInfo.OperationID;//玩家操作帧
					PlayerOperation _operation = battleInfo.SelfOperation;//玩家操作
					UpdatePlayerOperation(_operation,_sync_frameid);
					break;
				case ActionCode.ClientSendGameOver:
					//Console.WriteLine("ClientSendGameOver   "+pack.Str);
					UpdatePlayerGameOver(int.Parse(pack.Str));
					break;
			}
		}
		/*****************结束战斗部分*************************/
		private void SendFinishBattle(string ipport, int battleid)
		{

			MainPack pack = new MainPack();
			pack.Requestcode = RequestCode.Battle;
			pack.Actioncode = ActionCode.BattlePushDowmGameOver;
			pack.Str = "1";
			//Logging.Debug.Log("发送服务器认为  玩家还没同步的帧:" + pack+"\n");
			//10.采用udp将数据包发送出去   可能会发送补发的帧
			LZJUDP.Instance.Send(pack, ipport);
		}
		private void WaitClientFinish()
		{
			Console.WriteLine("等待客户端结束～");
			finishTime -= 1000f;
			if (finishTime <= 0)
			{
				foreach (var item in dic_battleid_ip_port)
					SendFinishBattle(item.Value, item.Key);


				waitBattleFinish.Dispose();
				BattleManage.Instance.FinishBattle(battleID,dic_match_frames);
				Console.WriteLine("战斗结束咯......");
			}
		}
		/// <summary>
		/// qf qf q q
		/// </summary>
		/// <param name="_battleId"></param>
		public void UpdatePlayerGameOver(int _battleId)
		{
			oneGameOver = true;
			dic_playerGameOver[_battleId] = true;

			allGameOver = true;
			foreach(bool playergameover in dic_playerGameOver.Values)
			{
				if (playergameover == false)
				{
					allGameOver = false;
					break;
				}
			}

			if (allGameOver)
			{
				if (waitBattleFinish != null) return;
				Console.WriteLine("战斗即将结束咯......");
				_isRun = false;

				finishTime = 1000f;
				if (waitBattleFinish == null)
				{
					waitBattleFinish = new Timer(new TimerCallback(test), null, 1000, 1000);
					Thread.Sleep(1000);
					WaitClientFinish();
					
				}
			}
		}
		private void test(object x)
		{
			Console.WriteLine("1111" + x);
		}

	}



}
