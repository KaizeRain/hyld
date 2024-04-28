/****************************************************
    Author:            龙之介
    CreatTime:    2022/4/19 20:36:17
    Description:     Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

using SocketProto;




namespace Manger
{

    public class BattleManger : MonoBehaviour
    {
        public static BattleManger Instance { get; private set; }
        public bool ISNet = true;
        public HYLDPlayerManger playerManger;
        public HYLDCameraManger cameraManger;
        public HYLDBulletManger bulletManger;
        public ScenseBuildLogic ScenseBuildLogic;
        public GameObject _StartGameAni;
        public GameObject BG;
        private bool isBattleStart;
        private string Loacal_IP_port;
        public bool IsGameOver { get; private set; }
        public Toolbox toolbox;
        public virtual void Init()
        {
            //1.9更新战场数据，跳转战斗场景。进入战场，
            //创建角色预制体，生成场景，生成战斗管理，
            //开启协程WaitInitData，
            //等待几个初始化脚本初始化完成。
            //开始循环发送Send_FightReady
            Instance = this;
            IsGameOver = false;
            HYLDStaticValue.isNet = ISNet;
            
            playerManger = gameObject.AddComponent<HYLDPlayerManger>();
            cameraManger = gameObject.AddComponent<HYLDCameraManger>();
            bulletManger = gameObject.AddComponent<HYLDBulletManger>();
            ScenseBuildLogic.InitData();
            //HYLDStaticValue.InitData();
            playerManger.InitData();
            cameraManger.InitData();
            bulletManger.InitData();
            if (!ISNet)
            {
                gameObject.AddComponent<HYLDManger>();
                Logging.HYLDDebug.TraceSavePath = $"D:/XuanShuiLiuLi/HYLDNew/hyld/Client/build/log/单机测试.txt";

                Google.Protobuf.Collections.RepeatedField<BattlePlayerPack> list_battleUser = new Google.Protobuf.Collections.RepeatedField<BattlePlayerPack>();
                BattlePlayerPack user = new BattlePlayerPack();
                HYLDStaticValue.PlayerUID = 1;
                user.Battleid = 1;
                user.Hero = SocketProto.Hero.XueLi;
                user.Id = 1;
                user.Teamid = 1;
                user.Playername = "单机";
                list_battleUser.Add(user);
                Manger.BattleData.Instance.InitBattleInfo(1000, list_battleUser);
            }


            NetGlobal.Instance.Init();
        }
        private void Start()
        {
            isBattleStart = false;
            //开启协程WaitInitData，
            if (ISNet)
            {

                Loacal_IP_port = Server.UDPSocketManger.Instance.InitSocket();
                Server.UDPSocketManger.Instance.Handle = HandleMessage;
                StartCoroutine(WaitInitData());
            }
            else
            {
                //_StartGameAni.SetActive(true);
                StartCoroutine(WaitInitData());
  
            }
            BattleData.Instance.OnLogicUpdate = OnLogicUpdate;



        }

        private void FixedUpdate()
        {
            cameraManger.OnLogicUpdate();
        }

        IEnumerator WaitInitData()
        {
            //等待几个初始化脚本初始化完成。
            yield return new WaitUntil(() => {
                return playerManger.initFinish && cameraManger.initFinish&&bulletManger.initFinish&& ScenseBuildLogic.InitFinish;//roleManage.initFinish && obstacleManage.initFinish && bulletManage.initFinish;
            });

            if (ISNet)
            {
                //开始循环发送Send_FightReady
                //id.text = "id :" + HYLDStaticValue.playerSelfIDInServer.ToString();
                /// Logging.HYLDDebug.LogError("WaitInitData()~~~Over");
                this.InvokeRepeating("Send_BattleReady", 0.2f, 0.2f);
            }
            else
            {

                NetGlobal.Instance.AddAction(() => { BG.SetActive(false); _StartGameAni.SetActive(true); });
                this.InvokeRepeating("Send_operation", Server.NetConfigValue.frameTime, Server.NetConfigValue.frameTime);
            }
           
        }
        void Send_BattleReady()
        {
            MainPack pack = new MainPack();
            pack.Requestcode = RequestCode.Battle;
            pack.Actioncode = ActionCode.BattleReady;
            BattlePlayerPack playerPack = new BattlePlayerPack();
            playerPack.Battleid = BattleData.Instance.battleID;
            playerPack.Id = HYLDStaticValue.PlayerUID;
            pack.Battleplayerpack.Add(playerPack);
            pack.Str = Loacal_IP_port.ToString();
           // Logging.HYLDDebug.LogError("Send_BattleReady:" + pack);
            Server.UDPSocketManger.Instance.Send(pack);
        }

        void Send_operation()
        {
            //17.SendOperation:采集自己的操作，上报给服务器，“你认为”的下一帧，next_frame_id=this.sync_framed+1,发送给服务器
            //(这个是  this.InvokeRepeating("Send_BattleReady", _time, _time);)
            //Manger.BattleData.Instance.selfOperation=new PlayerOperation();

            //Manger.BattleData.Instance.selfOperation.OpType = SocketProto.OperationType.Move;
            //Manger.BattleData.Instance.selfOperation.PlayerMoveX = 1;
            //Manger.BattleData.Instance.selfOperation.PlayerMoveY = 1;

            if (ISNet)
            {
                //先清空上一次操作
                Manger.BattleData.Instance.ResetOperation();
                //使用命令模式更新所有操作
                CommandManger.Instance.Execute();
                //向服务器发送当前操作帧
                Server.UDPSocketManger.Instance.SendOperation();
            }
            else
            {
                Manger.BattleData.Instance.ResetOperation();
                CommandManger.Instance.Execute();
                MainPack pack = new MainPack();
                pack.Requestcode = RequestCode.Battle;
                pack.Actioncode = ActionCode.BattlePushDowmPlayerOpeartions;
                pack.BattleInfo = new BattleInfo();
                pack.BattleInfo.SelfOperation = Manger.BattleData.Instance.selfOperation;
                pack.BattleInfo.OperationID = Manger.BattleData.Instance.sync_frameID + 1;//我认为”的下一帧
                Google.Protobuf.Collections.RepeatedField<PlayerOperation> allPlayerOperation = new Google.Protobuf.Collections.RepeatedField<PlayerOperation>();
                allPlayerOperation.Add(pack.BattleInfo.SelfOperation);
                OnLogicUpdate(pack.BattleInfo.OperationID, allPlayerOperation);
            }
           // Logging.HYLDDebug.LogError("?????????????????????");
            //Manger.BattleData.Instance.selfOperation.OpType = OperationType.Attack;

        }

        IEnumerator WaitForFirstMessage()
        {
            yield return new WaitUntil(() => {
                return BattleData.Instance.GetFrameDataNum > 0; // 在这里等待第一帧，第一帧没更新之前不会做更新。
            });
            //1.18收到第一帧，正式开始游戏 播放开场动画
            NetGlobal.Instance.AddAction(() => { BG.SetActive(false); _StartGameAni.SetActive(true); });
            /*
            if (delegate_readyOver != null)
            {
                delegate_readyOver();    // 关闭对局等待界面
            }
            */
        }
        protected virtual void OnLogicUpdate(int frameid,Google.Protobuf.Collections.RepeatedField<PlayerOperation> allPlayerOperation,bool isBuzhen=false)
        {

           // string res = "frameid:" + frameid + "\n";
            foreach (PlayerOperation playerOperation in allPlayerOperation)
            {
               // Debug.LogError("!!!! ");
                playerManger.OnLogicUpdate(playerOperation);
               //res += "optx:" + playerOperation.PlayerMoveX + "\n";
                //res += "opty:" + playerOperation.PlayerMoveY + "\n";
            }

            playerManger.OnLogicUpdate();
           // Debug.LogError("！！！"+frameid);
            //NetGlobal.Instance.AddAction(() => { opt.text = res; });
            //Logging.HYLDDebug.LogError(res);
            //Debug.LogError("????");
            //Logging.HYLDDebug.LogError("????不追踪了？");
            Logging.HYLDDebug.Trace($"/************************/\n 第{frameid}帧行为   {isBuzhen}");
            for (int i = 0; i < HYLDStaticValue.Players.Count; i++)
            {
                if (HYLDStaticValue.Players[0].teamID != HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].teamID)
                {
                    Logging.HYLDDebug.Trace($"第{i}个player({HYLDStaticValue.Players[i].playerName})行为 : 坐标({HYLDStaticValue.Players[i].playerPositon.x*-1},{HYLDStaticValue.Players[i].playerPositon.z*-1}) || 攻击 （state: { HYLDStaticValue.Players[i].fireState}   fireTowards:({HYLDStaticValue.Players[i].fireTowards.x*-1},{HYLDStaticValue.Players[i].fireTowards.y}, { HYLDStaticValue.Players[i].fireTowards.z*-1}) ）");
                }
                else
                {
                    Logging.HYLDDebug.Trace($"第{i}个player({HYLDStaticValue.Players[i].playerName})行为 : 坐标({HYLDStaticValue.Players[i].playerPositon.x},{HYLDStaticValue.Players[i].playerPositon.z}) || 攻击 （state: { HYLDStaticValue.Players[i].fireState}   fireTowards:({HYLDStaticValue.Players[i].fireTowards.x},{HYLDStaticValue.Players[i].fireTowards.y}, { HYLDStaticValue.Players[i].fireTowards.z}) ）");
                }
               
            }
            bulletManger.OnLogicUpdate();
            Logging.HYLDDebug.Trace($"/*******/\n 子弹行为");
            /*
             myid:1
{
frameid:1
optx:
opty:
}
             */
        }


        private void HandleMessage(MainPack pack)
        {
            //Debug.LogError(pack);
            switch (pack.Requestcode)
            {
                case RequestCode.Battle:
                    if (pack.Actioncode == ActionCode.BattleStart)
                    {
                        //1.12取消发送Send_BattleReady
                        //循环调用Send_operation发送玩家操作
                        Logging.HYLDDebug.LogError("BattleStatr:  " + pack);
                        if (isBattleStart)
                        {
                            return;
                        }
                        isBattleStart = true;
                       //  Logging.HYLDDebug.LogError("???????????????");
                        NetGlobal.Instance.AddAction(() =>
                        {
                            // Logging.HYLDDebug.LogError("!!!!!!!3!!1");
                            this.CancelInvoke("Send_BattleReady");
                              //Logging.HYLDDebug.LogError("!!!!!!!!!!1!!!!!");


                            float _time = Server.NetConfigValue.frameTime;  // 66ms
                            //_time = 0.5f;
                            this.InvokeRepeating("Send_operation", _time, _time);  // 循环调用 Send_operation 方法
                          //1.13开启一个等待协程，等待服务器发送服务器帧大于0，
                            StartCoroutine(WaitForFirstMessage());
                        });

                    }
                    else if (pack.Actioncode == ActionCode.BattlePushDowmAllFrameOpeartions)
                    {
                        //1.17OnLogicUpdate_sync_FrameIdCheck
                        //提供帧，给更新方法去解析帧操作
                       
                        //11.客户端通过网络收到帧同步的数据包以后，OnLogicUpdate()
                        //服务器发送所有帧操作。
                        // Logging.HYLDDebug.LogError("服务器发送所有帧操作。" + pack);
                        //检查是不是ok的
                        if (!BattleData.Instance.OnLogicUpdate_sync_FrameIdCheck(pack.BattleInfo.OperationID, pack.BattleInfo.AllPlayerOperation))
                        {
                            Logging.HYLDDebug.Trace("好像数据包寄了？");
                        }
                    }
                    else if (pack.Actioncode == ActionCode.BattlePushDowmGameOver)
                    {
                        NetGlobal.Instance.AddAction(() =>
                        {
                            toolbox.游戏结束方法();
                        });
                    }
                    break;
            }

            
        }


        public void BeginGameOver()
        {
            this.CancelInvoke("Send_operation");
            this.InvokeRepeating("SendGameOver", 0f, 0.5f);
        }
        private void SendGameOver()
        {
            MainPack pack = new MainPack();
            pack.Requestcode = RequestCode.Battle;
            pack.Actioncode = ActionCode.ClientSendGameOver;
            pack.Str = BattleData.Instance.battleID.ToString();
            //Logging.HYLDDebug.LogError("SendGameOver:" + pack);
            Server.UDPSocketManger.Instance.Send(pack);
        }
    }
    public class BattleData
    {
        private static BattleData instance;
        public int battleID { get; private set; }
        public int teamID { get; private set; }
        public PlayerOperation selfOperation;
        /// <summary>
        /// 存储所有玩家操作帧
        /// </summary>
       // private Dictionary<int, AllPlayerOperation> dic_frameDate;
        /*
        /// <summary>
        /// 存储所有玩家当前到那一帧了(可用帧)
        /// </summary>
        private Dictionary<int, int> dic_ValidOperationID;*/
        /// <summary>
        /// 玩家信息uid，name，hero
        /// </summary>
        public List<BattlePlayerPack> list_battleUsers { get; private set; }
        /// <summary>
        /// 记录当前客户端真正已经同步到哪个帧了//务必要初始化啊按啊按啊啊啊 
        /// </summary>
        public int sync_frameID { get; private set; }
        public Action<int,Google.Protobuf.Collections.RepeatedField<PlayerOperation>, bool> OnLogicUpdate;
        private BattleData()
        {
           // dic_frameDate = new Dictionary<int, AllPlayerOperation>();
            //dic_ValidOperationID = new Dictionary<int, int>();
        }
        /// <summary>
        /// 随机种子
        /// </summary>
        public int randSeed { get; private set; }
        public static BattleData Instance
        {
            get
            {
                // 如果类的实例不存在则创建，否则直接返回
                if (instance == null)
                {
                    instance = new BattleData();
                }
                return instance;
            }
        }
        /// <summary>
        /// 获取当前游戏到第几帧
        /// </summary>
        public int GetFrameDataNum { 
            get 
            {
                return sync_frameID;
                /*
                if (dic_frameDate == null) 
                    return 0;
                return dic_frameDate.Count;
                */
            }
        }

        public void InitBattleInfo(int _randSeed, Google.Protobuf.Collections.RepeatedField<BattlePlayerPack> battleUsersInfo)
        {
            Logging.HYLDDebug.Log("InitBattleInfo  初始化战场信息 " + Time.realtimeSinceStartup);
            sync_frameID = 0;
            list_battleUsers = new List<BattlePlayerPack>();
            randSeed = _randSeed;
            foreach (var user in battleUsersInfo)
            {
                list_battleUsers.Add(user);
                if (user.Id == HYLDStaticValue.PlayerUID)
                {
                    battleID = user.Battleid;
                    selfOperation = new PlayerOperation();
                    selfOperation.Battleid = battleID;
                    //user.Playername;
                    teamID=user.Teamid;
                }
                //dic_ValidOperationID[user.Battleid] = 0;//所有玩家都在第0帧
            }
        }
        /// <summary>
        /// 清空玩家操作
        /// </summary>
        public void ResetOperation()
        {
            selfOperation.PlayerMoveX = 0;
            selfOperation.PlayerMoveY = 0;
            selfOperation.BulletInfo = null;
        }
        /// <summary>
        /// 收到服务器发来的帧，逻辑帧更新
        /// </summary>
        public bool OnLogicUpdate_sync_FrameIdCheck(int server_sync_frameid, Google.Protobuf.Collections.RepeatedField<AllPlayerOperation> allPlayerOperation)
        {
            //12.每个客户端，也都会有一个sync_frameID记录当前客户端真正已经同步到哪个帧了
            //Logging.HYLDDebug.LogError(sync_frameID + "  =比较帧=   " + server_sync_frameid);
            if (sync_frameID > server_sync_frameid)
            {
                Logging.HYLDDebug.Trace($"如果收到的帧{server_sync_frameid}<客户端的帧{sync_frameID}  ,直接丢弃这个帧。");
                /*
                13 如果收到的帧id<sync_frameID  ,直接丢弃这个帧。
	            a:为什么会出现这个情况。 因为udp会有先发后到，后发先到：  客户端处理了100帧，但是服务端这次给他发的99帧
	            b:为什么我们没有收到99帧，就开始处理100帧，能同步吗？[如果99帧没有处理，服务器在发100帧的时候会补发99帧]
                */
                return false;
            }

            int len = allPlayerOperation.Count;
            if (len < server_sync_frameid - sync_frameID)
            {
                Logging.HYLDDebug.Trace($"发的数据不够！ {len}  <  {server_sync_frameid} - {sync_frameID}");
                return false;
            }
            /*
            for (int i = sync_frameID,j=0; i <= server_sync_frameid; i++)
            {
              //  dic_frameDate[sync_frameID] = allPlayerOperation[j++];
            }
            */

            /*
             
            14.如果上一帧的操作不为空，我们处理下一帧之前，一定要先同步一下上一帧的结果。
	        客户端A: |----|---66.3--|----|
	        客户端B: |----|---66.1--|----|
	        每个客户端收到数据包的时间不一样。所以在帧与帧之间会发生时间的差异导致位置不同步。
	        所有都用66ms迭代出新的位置和结果。统一都以66ms来迭代。
	        处理下一帧之前每帧都同步；==》同样的输入--》同样的输出
            TODO:这里/*
            预测+和解是解决自己的问题，发生在逻辑层；插值是解决其他人的问题，发生在表现层
             帧同步就是，每隔固定帧率  玩家会发送自己的操作帧给服务器，服务器会收集这些操作，也是按固定帧率发给玩家。玩家收到服务器的帧就开始帧同步
            但是，这样的话，我们每次按一个操作，都要等服务器的帧操作再发回来，我们才能真的操作
            这样体验很打咩

            加入预测：
            如果只预测的话
            比如我发送了两次指令，但是服务器突然发来之前的第一次指令，那么你就会被强制拉回去。体验极差
             
             加入和解：
            如果加入和解，记录四个变量，权威状态，权威输入，预测输入，预测状态。
            权威是服务器的信息
            我们每次更新状态的时候 权威状态 = 上一次权威状态+权威输入；   预测状态=权威状态+预测输入
             这样在没有网络延迟的状态， 我们是完全可以保持每次都预测对的


            还需要插值？
            但是，还存在对手。如果你往右移动，然后预测一波往右移动，
            但如果对手击晕你了，而且是在你往右移动的操作发起之前，但是本地预测已经在右了，
            当服务器发给你权威输入权威状态的时候，你的往右操作会被对手击晕操作给撤销
             
             所以我们需要插值去平滑过渡，表现层让他感觉到没问题！
             */
            //调用逻辑更新方法LogicUpdate
            if (allPlayerOperation.Count>2)
            Logging.HYLDDebug.Trace($"跳帧：快速同步过时的帧完直到最新的一帧. 总帧数：  {allPlayerOperation.Count}");
            //15.跳帧：快速同步过时的帧完直到最新的一帧.
            for (int i = 0; i < allPlayerOperation.Count; i++)
            {
                if (sync_frameID >= allPlayerOperation[i].Frameid)
                {
                    //如果已经更新过了就不更新了
                    continue;
                }
                else if (server_sync_frameid <= allPlayerOperation[i].Frameid)
                {
                    Logging.HYLDDebug.Trace($"跳帧结束 最新帧：  {server_sync_frameid}");

                    //到最新帧了
                    break;
                }
                OnLogicUpdate(server_sync_frameid, allPlayerOperation[i].Operations,true);
                //快速更新帧
            }
            //跳到收到的最新帧之前
            
            //16.控制客户端根据操作来更新逻辑推进。



            sync_frameID = server_sync_frameid;
            if (allPlayerOperation.Count > 0)
            {
                //Logging.HYLDDebug.LogError(sync_frameID);
                OnLogicUpdate(server_sync_frameid, allPlayerOperation[allPlayerOperation.Count-1].Operations,false);
            }

            return true;
        }
    }
    public class HYLDRandom
    {
        static ulong next = 1;
        public static void srand(ulong seed)
        {
            next = seed;
        }
        public static int Range()
        {
            next = next * 1103515245 + 12345;
            return (int)((next / 65536) % 1000);
        }
    }
}