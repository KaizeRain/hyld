using System;
using System.Collections.Generic;
using System.Text;
using SocketProto;
using Server;
using System.Linq;

namespace Server.Controller
{
    /// <summary>
    /// 通过反射机制找到RequestCode对应的方法来调用
    /// </summary>
    abstract class BaseControllers
    {
        protected RequestCode requestCode = RequestCode.RequestNone;
        public RequestCode GetRequestCode
        {
            get { return requestCode; }
        }
        public virtual void CloseClient(Client client, int id)
        {
            
        }
    }
    class ClearSenceController : BaseControllers
    {
        private Dictionary<int, bool> _dic_ClearFinish; 
        public ClearSenceController()
        {
            requestCode = RequestCode.ClearSence;
            _dic_ClearFinish = new Dictionary<int, bool>();
        }
        public void  AllClearSenceReady(Server server, List<int> playeruids)
        {
            MainPack pack = new MainPack();
            pack.Actioncode = ActionCode.AllClearSenceReady;
            pack.Requestcode = RequestCode.ClearSence;
            pack.Str = "1";
            foreach (int id in playeruids)
            {
                server.GetActiveClient(id).Send(pack);
                _dic_ClearFinish.Remove(id);
            }

            //return pack;

        }
        public MainPack ClientSendClearSenceReady(Server server, Client client, MainPack pack)
        {
            int uid = int.Parse(pack.Str);
            List<int> playeruids = server.DIC_BattlePlayerUIDs[server.DIC_BattleIDs[uid]];
            _dic_ClearFinish[uid] = true;
            bool isOK = true;
            foreach (int id in playeruids)
            {
                if (!_dic_ClearFinish.ContainsKey(id))
                {
                    isOK = false;
                    break;
                }
            }
            if (isOK)
            {
                AllClearSenceReady(server,playeruids);
            }
            return pack;
        }
    }



    public struct MatchUserInfo
    {
        public int uid;
        public string userName;
        public Hero hero;
        public int teamid;
        public string socketIP;
        public override string ToString()
        {
            return $"[uid: {uid}  userName: {userName}  hero: {hero}  teamid: {teamid}  socketIP:{socketIP}]";
        }
    }
    class MatchingController : BaseControllers
    {
        public MatchingController()
        {
            MathingDic = new Dictionary<FightPattern, List<BattleRoom>>();
            foreach (FightPattern pattern in Enum.GetValues(typeof(FightPattern)))
            {
                MathingDic.Add(pattern, new List<BattleRoom>());
            }
            PlayerIDMapRoomDic = new Dictionary<int, BattleRoom>();
            requestCode = RequestCode.Matching;
        }

        /// <summary>
        /// 雪花算法获得时间戳ID
        /// </summary>
        public class TimestampID
        {
            private long _lastTimestamp;
            private long _sequence; //计数从零开始
            private readonly DateTime? _initialDateTime;
            private static TimestampID _timestampID;
            private const int MAX_END_NUMBER = 9999;

            private TimestampID(DateTime? initialDateTime)
            {
                _initialDateTime = initialDateTime;
            }

            /// <summary>
            /// 获取单个实例对象
            /// </summary>
            /// <param name="initialDateTime">最初时间，与当前时间做个相差取时间戳</param>
            /// <returns></returns>
            public static TimestampID GetInstance(DateTime? initialDateTime = null)
            {
                if (_timestampID == null) System.Threading.Interlocked.CompareExchange(ref _timestampID, new TimestampID(initialDateTime), null);
                return _timestampID;
            }

            /// <summary>
            /// 最初时间，作用时间戳的相差
            /// </summary>
            protected DateTime InitialDateTime
            {
                get
                {
                    if (_initialDateTime == null || _initialDateTime.Value == DateTime.MinValue) return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    return _initialDateTime.Value;
                }
            }
            /// <summary>
            /// 获取时间戳ID
            /// </summary>
            /// <returns></returns>
            public string GetID()
            {
                long temp;
                var timestamp = GetUniqueTimeStamp(_lastTimestamp, out temp);
                return $"{timestamp}{Fill(temp)}";
            }

            private string Fill(long temp)
            {
                var num = temp.ToString();
                IList<char> chars = new List<char>();
                for (int i = 0; i < MAX_END_NUMBER.ToString().Length - num.Length; i++)
                {
                    chars.Add('0');
                }
                return new string(chars.ToArray()) + num;
            }

            /// <summary>
            /// 获取一个时间戳字符串
            /// </summary>
            /// <returns></returns>
            private long GetUniqueTimeStamp(long lastTimeStamp, out long temp)
            {
                lock (this)
                {
                    temp = 1;
                    var timeStamp = GetTimestamp();
                    if (timeStamp == _lastTimestamp)
                    {
                        _sequence = _sequence + 1;
                        temp = _sequence;
                        if (temp >= MAX_END_NUMBER)
                        {
                            timeStamp = GetTimestamp();
                            _lastTimestamp = timeStamp;
                            temp = _sequence = 1;
                        }
                    }
                    else
                    {
                        _sequence = 1;
                        _lastTimestamp = timeStamp;
                    }
                    return timeStamp;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private long GetTimestamp()
            {
                if (InitialDateTime >= DateTime.Now) throw new Exception("最初时间比当前时间还大，不合理");
                var ts = DateTime.UtcNow - InitialDateTime;
                return (long)ts.TotalMilliseconds;
            }
        }
        /// <summary>
        /// 匹配房间
        /// </summary>
        public class BattleRoom
        {
            public class BattleTeam
            {
                public string Teamid { get; private set; }
                public int num { get { return playerIDs.Count; } }
                private int maxnum;//{ get; private set; }
                public bool IsFull { get { return num >= maxnum; } }

                public void Join(int id)
                {
                    playerIDs.Add(id);
                }
                public List<int> playerIDs { get; private set; }
                public BattleTeam(string teamid,List<int> playerIDs, int maxnum)
                {
                    Logging.Debug.Log($"BattleTeam:  {teamid}  {playerIDs}   {maxnum}");
                    Teamid = teamid;
                    this.maxnum = maxnum;
                    this.playerIDs = new List<int>();
                    this.playerIDs.AddRange(playerIDs);
                }
            }
            
            public BattleRoom(string roomid,FightPattern fightPattern)
            {
                this.roomid = roomid;
                switch (fightPattern)
                {
                    case FightPattern.BaoShiZhengBa:
                    case FightPattern.JinKuGongFang:
                    case FightPattern.LuanDouZuQiu:
                        RoomMaxNumber = ServerConfig.MaxRoom3_3Number;
                        maxTeamnum = ServerConfig.MaxTeam3_3Number;
                        break;
                    case FightPattern.HuangYeJueDou:
                        RoomMaxNumber = 10;
                        maxTeamnum = 5;
                        break;
                }
            }
            public string roomid { get; private set; }
            public int RoomMaxNumber { get; private set; }
            private int maxTeamnum;//{ get; private set; }
            private int curTeamnum{ 
                get {return battleTeams.Count;} 
            }
            public int RoomNumber
            {
                get {
                    int res = 0;
                    foreach (var room in battleTeams)
                    {
                        res += room.num;
                    }
                    return res;
                }
            }
            private List<BattleTeam> battleTeams = new List<BattleTeam>();

            public void Exit(int id)
            {
                List<int> team = null;
                foreach (var room in battleTeams)
                {
                    foreach (var xid in room.playerIDs)
                    {
                        if (xid == id)
                        {
                            team = room.playerIDs;//.Remove(id);
                            break;
                        }
                    }
                }
                team.Remove(id);
            }

            public bool Join(List<int> teamids)
            {
                if (teamids.Count == 1) return join(teamids[0]);
                return join(teamids);
            }
            private bool join(int id)
            {
                //搜索所有fightPattern模式的队伍
                //Logging.Debug.Log("搜索所有fightPattern模式的队伍");
                bool isJoin = false;
                foreach (BattleTeam Team in battleTeams)
                {
                    //如果有队伍没满的就加入
                    if (!Team.IsFull)
                    {
                        isJoin = true;
                        Team.Join(id);
                        break;
                    }
                }
                if (!isJoin)
                {
                    //队伍都满人了否则自己创建一个小队
                    //队伍已满
                    if (curTeamnum == maxTeamnum) return false;
                    //Logging.Debug.Log("队伍都满人了,否则自己创建一个小队");
                    battleTeams.Add(new BattleTeam(TimestampID.GetInstance().GetID(),new List<int>() { id },RoomMaxNumber/maxTeamnum));
                }
                return true;
            }
            private bool join(List<int> teamids)
            {
                if (curTeamnum == maxTeamnum) return false;
                //否则自己创建一个小队
                battleTeams.Add(new BattleTeam(TimestampID.GetInstance().GetID(),teamids, RoomMaxNumber / maxTeamnum));
                return true;   
            }
            public bool CheckCanFight()
            {
                if (curTeamnum == maxTeamnum)
                {
                    //bool isOk = false;
                    foreach (var team in battleTeams)
                    {
                        if (!team.IsFull) return false;
                    }
                    return true;
                }
                return false;
            }

            public List<Tuple<int,string,string>> GetRoomPlayerInfo()
            {
                //         uid   roomid  teamid
                List<Tuple<int, string, string>> res = new List<Tuple<int, string, string>>();
               
                string roomid = this.roomid;

                foreach (var team in battleTeams)
                {
                    string teamid = team.Teamid;
                    foreach (var player in team.playerIDs)
                    {
                        res.Add(new Tuple<int, string, string>(player, roomid, teamid));
                    }
                }
                return res;
            }
        }
       
        public enum FightPattern
        {
            BaoShiZhengBa = 0,
            JinKuGongFang = 1,
            ShangJinLieRen = 2,
            LuanDouZuQiu = 3,
            HuangYeJueDou = 4,
        }
        private Dictionary<FightPattern, List<BattleRoom>> MathingDic;
        private Dictionary<int, BattleRoom> PlayerIDMapRoomDic;
        /// <summary>
        /// 将把所有队伍的成员由房主传达List<BattlePlayerPack>
        /// 或者传自己的 BattlePlayerPack即可
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack AddMatchingPlayer(Server server, Client client, MainPack pack)
        {
            //获取模式信息，玩家们id
            FightPattern fightPattern = (FightPattern)pack.Playerspack[0].Fightpattern;
            //Logging.Debug.Log($"{fightPattern}    {pack.Playerspack[0].Fightpattern}");
            List<int> playerids = new List<int>();
            foreach (var player in pack.Playerspack)
            {
                playerids.Add(player.Id);
            }
            /*
             1.2AddMatchingPlayer将当前玩家和信息加入到匹配队列
             */
            try
            {
                BattleRoom room = Join(playerids, fightPattern);
                //1.4判断是否达到对战条件
                if (room.CheckCanFight())
                {
                    //1.5记录对局batlleID创建对局
                    //Logging.Debug.Log("可以开始战斗了！");
                    var players = room.GetRoomPlayerInfo();
                    List<MatchUserInfo> matchUsers = new List<MatchUserInfo>();

                    Dictionary<string, int> teamID = new Dictionary<string, int>();
                    int curMaxID = 1;
                    foreach (var player in players)
                    {
                        MatchUserInfo userInfo = new MatchUserInfo();
                        userInfo.uid = player.Item1;
                        Client c = server.GetActiveClient(userInfo.uid);
                        userInfo.hero = c.PlayerHero;
                        userInfo.userName = c.PlayerName;
                        if (!teamID.ContainsKey(player.Item3))
                        {
                            teamID.Add(player.Item3, curMaxID++);
                        }
                        userInfo.teamid = teamID[player.Item3];

                        userInfo.socketIP = c.socketIp;
                        matchUsers.Add(userInfo);

                        //将那些玩家移除房间字典
                        PlayerIDMapRoomDic.Remove(userInfo.uid);
                        //Exit(userInfo.uid, fightPattern);
                        Logging.Debug.Log(userInfo);
                    }

                    //将那些玩家移除房间字典
                    MathingDic[fightPattern].Remove(room);

                    //1.5记录对局batlleID创建对局
                    StartFighting(server,matchUsers,fightPattern);
                    
                }

                //广播房间人数
                pack.Returncode = ReturnCode.Succeed;
                pack.Str = room.RoomNumber.ToString() + "/" + room.RoomMaxNumber;
                Logging.Debug.Log("房间人数" + pack.Str);
                foreach (var player in room.GetRoomPlayerInfo())
                {
                    Client c = server.GetActiveClient(player.Item1);
                    if (!c.Equals(client))
                    {
                        c.Send(pack);
                    }
                }
            }
            catch(Exception ex)
            {
                Logging.Debug.Log(ex);
            }
            return pack;
        }

        public MainPack RemoveMatchingPlayer(Server server, Client client, MainPack pack)
        {
            try
            {
                //获取模式信息，玩家们id
                FightPattern fightPattern = (FightPattern)pack.Playerspack[0].Fightpattern;
                Logging.Debug.Log($"{fightPattern}    {pack.Playerspack[0].Fightpattern}");

                List<int> playerids = new List<int>();
                foreach (var player in pack.Playerspack)
                {
                    playerids.Add(player.Id);
                }
                Logging.Debug.Log("移出对战房间");
                //退出对战房间
                BattleRoom room = Exit(playerids, fightPattern);


                //广播房间人数
                pack.Returncode = ReturnCode.Succeed;
                pack.Str = room.RoomNumber.ToString() + "/" + room.RoomMaxNumber;
                Logging.Debug.Log("广播房间人数: "+pack.Str);
                foreach (var player in room.GetRoomPlayerInfo())
                {
                    Client c = server.GetClientByID(player.Item1);
                    if (!c.Equals(client))
                    {
                        c.Send(pack);
                    }
                }

                pack.Str = "-1";
                foreach (var player in playerids)
                {
                    Client c = server.GetClientByID(player);
                    if (!c.Equals(client))
                    {
                        c.Send(pack);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
            }
            return pack;
        }
        private BattleRoom Join(List<int> playerids,FightPattern fightPattern)
        {
            bool isOk = false;
            BattleRoom battleroom = null;
            foreach (var room in MathingDic[fightPattern])
            {
                //加入成功
                if (room.Join(playerids))
                {
                    battleroom = room;
                    isOk = true;
                    break;
                }
            }
            Logging.Debug.Log($"加入是否房间成功： {isOk}");
            //加入失败
            if (!isOk)
            {
                battleroom = new BattleRoom(TimestampID.GetInstance().GetID(), fightPattern);
                battleroom.Join(playerids);

                MathingDic[fightPattern].Add(battleroom);
            }
            foreach (int id in playerids)
                PlayerIDMapRoomDic[id] = battleroom;
            return battleroom;
        }
        private BattleRoom Exit(List<int> playerids, FightPattern fightPattern)
        {
            BattleRoom battleroom = PlayerIDMapRoomDic[playerids[0]];
            foreach (int id in playerids)
            {
                battleroom.Exit(id);
            }
            return battleroom;
        }
        private void StartFighting(Server server,List<MatchUserInfo> infos,FightPattern fightPattern)
        {
            //1.5记录对局batlleID创建对局
           // Logging.Debug.Log("开打开打");
            //uid teamid roomid
            foreach (var info in infos)
            {
                //Logging.Debug.Log(info);
            }
            BattleManage.Instance.BeginBattle(server,infos, fightPattern);
        }
        public override void CloseClient(Client client, int id)
        {
            base.CloseClient(client, id);
            if (PlayerIDMapRoomDic.ContainsKey(id))
            {
                PlayerIDMapRoomDic[id].Exit(id);
            }

        }
    }
    class PingPongController : BaseControllers
    {
        public PingPongController()
        {
            requestCode = RequestCode.PingPong;
        }
        public MainPack Ping(Server server, Client client, MainPack pack)
        {
            pack.Actioncode = ActionCode.Pong;
            client.lastPingTime = Tool.PingPongTool.GetTimeStamp();
            return pack;
        }
    }
    class FriendRoomController : BaseControllers
    {
        public FriendRoomController()
        {
            requestCode = RequestCode.FriendRoom;
        }
        public MainPack CreateRoom(Server server, Client client, MainPack pack)
        {
            return server.CreateFriendRoom(client, pack);
        }
        public MainPack InviteFriend(Server server, Client client, MainPack pack)
        {
            if (server.InvateToRoom(client, pack.Str,pack.Friendroompack[0]))
            {
                pack.Actioncode = ActionCode.ActionNone;
                pack.Returncode = ReturnCode.Succeed;
            }
            else
            {
                pack.Actioncode = ActionCode.ActionNone;
                pack.Returncode = ReturnCode.Fail;
            }
            return pack;
        }

        public MainPack AcceptInvateFriend(Server server, Client client, MainPack pack)
        {
            pack=server.AcceptInvateFriend(client,pack,server);
            return pack;
        }

        public MainPack RejectInvateFriend(Server server, Client client, MainPack pack)
        {
            return server.RejectInvateFriend(client, pack);
        }

        public MainPack CancalInvateFriend(Server server, Client client, MainPack pack)
        {
            server.CancalInvateFriend(pack);
            pack.Actioncode = ActionCode.ActionNone;
            return pack;
        }
        public MainPack ExitRoom(Server server, Client client, MainPack pack)
        {
            return  server.ExitFriendRoom(client, pack);
            
        }
        public MainPack Chat(Server server, Client client, MainPack pack)
        {
            server.Chat(client, pack);
            pack.Actioncode = ActionCode.ActionNone;
            return pack;
        }
    }
    class FriendController : BaseControllers
    {
        public FriendController()
        {
            requestCode = RequestCode.Friend;
        }
        /// <summary>
        /// 加好友
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack AplyAddFriend(Server server, Client client, MainPack pack)
        {
            if (client.GetUserData.AplyAddFriend(pack, client.GetMysqlConnecet, server))
            {
                pack.Returncode = ReturnCode.Succeed;
            }
            else pack.Returncode = ReturnCode.Fail;
            return pack;
        }
        public MainPack AcceptAddFriend(Server server, Client client, MainPack pack)
        {
            if (client.GetUserData.AcceptAddFriend(ref pack,client, client.GetMysqlConnecet, server))
            {
                pack.Returncode = ReturnCode.Succeed;
            }
            else pack.Returncode = ReturnCode.Fail;
            return pack;
        }

        public MainPack RejectAddFriend(Server server, Client client, MainPack pack)
        {
            client.GetUserData.RejectAddFriend(pack, client.GetMysqlConnecet, server);
            pack.Returncode = ReturnCode.Fail;
            return pack;
        }
    }
    class UserController : BaseControllers
    {
        public UserController()
        {
            requestCode = RequestCode.User;
        }
        /// <summary>
        /// 注册 反射调用 返回包的结果：Succeed或者Fail
        /// </summary>
        /// <returns></returns>
        public MainPack Logon(Server server, Client client, MainPack pack)
        {
            //1.3将账号密码信息录入数据库
            if (client.GetUserData.Logon(pack, client.GetMysqlConnecet))
            {
                pack.Returncode = ReturnCode.Succeed;
            }
            else pack.Returncode = ReturnCode.Fail;
            //1.4返回结果
            return pack;
        }

        /// <summary>
        /// 登陆  反射调用
        /// </summary>
        /// <returns></returns>
        public MainPack Login(Server server, Client client, MainPack pack)
        {
            Logging.Debug.Log(client.UserName);
            if (client.UserName != null)
            {
                pack.Returncode = ReturnCode.Fail;
                return pack;
            }
            Logging.Debug.Log("find has already user"+ server.GetClientByUserName(pack.Loginpack.Username));
            if (server.GetClientByUserName(pack.Loginpack.Username) !=null&& server.GetActiveClient(server.GetClientByUserName(pack.Loginpack.Username).UID)!=null)
            {
                pack.Returncode = ReturnCode.Fail;
                return pack;
            }
            //0.3.查寻数据库用户信息
            if (client.GetUserData.Login(pack, client.GetMysqlConnecet))
            {
                Logging.Debug.Log("登录成功");
                pack.Returncode = ReturnCode.Succeed;
            }
            else pack.Returncode = ReturnCode.Fail;

            //发送结果
            return pack;
        }
        /// <summary>
        /// 找名字
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack FindPlayerInfo(Server server, Client client, MainPack pack)
        {
            //2.2FindPlayerInfo查找玩家名字
            if (client.GetUserData.FindPlayerInfo(ref pack, client))
            {
                server.AddActiveClient(client.UID, client);
                pack.Returncode = ReturnCode.Succeed;
            }
            else pack.Returncode = ReturnCode.Fail;
            //2.4返回查询结果
            return pack;
        }

        /// <summary>
        /// 找好友
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack FindFriendsInfo(Server server, Client client, MainPack pack)
        {
            if (client.GetUserData.FindFriendsInfo(ref pack,client, client.GetMysqlConnecet,server))
            {
                pack = client.GetActiveFriendInfoPack();
                pack.Actioncode = ActionCode.FindFriendsInfo;
                pack.Requestcode = RequestCode.User;
                pack.Returncode = ReturnCode.Succeed;
            }
            else pack.Returncode = ReturnCode.Fail;
            return pack;
        }
        /// <summary>
        /// 修改名字
        /// </summary>
        /// <returns></returns>
        public MainPack UpdateName(Server server, Client client, MainPack pack)
        {
            if (client.GetUserData.UpdateName(pack, client.GetMysqlConnecet))
            {
                pack.Returncode = ReturnCode.Succeed;
            }
            else pack.Returncode = ReturnCode.Fail;
            return pack;
        }
        public MainPack ChangeHero(Server server, Client client, MainPack pack)
        {
            client.GetUserData.ChangeHero(pack);
            if (pack.Str == "Room")
                server.ChangeHero(client, pack);
            pack.Actioncode = ActionCode.ActionNone;
            return pack;
        }
    }
}
