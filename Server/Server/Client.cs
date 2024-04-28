using System;
using System.Net.Sockets;
using Server.Tool;
using Server.DAO;
using SocketProto;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Server
{
    /// <summary>
    /// 拥有Send，Receive操作异步接受消息，同步发送消息
    /// </summary>
    class Client
    {
        public Socket _socket { get; private set; }
        public long lastPingTime = 0;
        private Message _message;
        private UserData _userdata;
        private Server _server;

        private MySqlConnection _mysqlConnection;
        
        public FriendRoom FriendRoom
        {
            get; set;
        }
        public Dictionary<int, Client> FriendsDic = new Dictionary<int, Client>();
        public UserData GetUserData
        {
            get { return _userdata; }
        }
        public string UserName
        {
            get { return _userdata.UserName; }
        }
        public int UID
        {
            get { return _userdata.UID; }
        }
        public string PlayerName
        {
            get { return _userdata.PlayerName; }
        }
        public Hero PlayerHero
        {
            get { return _userdata.PlayerHero; }

        }
        public PlayerState PlayerState
        {
            get; set;
        }
        public MySqlConnection GetMysqlConnecet
        {
            get { return _mysqlConnection; }
        }
        public string socketIp { get; private set; }
        public Client(Socket socket, Server server)
        {
            lastPingTime = Tool.PingPongTool.GetTimeStamp();
            _userdata = new UserData();
            _message = new Message();
            _server = server;
            _socket = socket;


            socketIp = _socket.RemoteEndPoint.ToString().Split(':')[0];

            try
            {
                //打开数据库连接
                _mysqlConnection = new MySqlConnection(ServerConfig.DOMConectStr);
                _mysqlConnection.Open();
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex.ToString());

                Close();
                return;
            }
            //4.开始异步接受消息
            ReceiveMessage();
        }
        /// <summary> 
        /// 接收消息 
        /// </summary> 
        /// <param name="clientSocket"></param> 
        private void ReceiveMessage()
        {
            try
            {
                Logging.Debug.Log("开始接收  client:" + _socket.LocalEndPoint + "  ---  sever:" + _socket.RemoteEndPoint);
                _socket.BeginReceive(_message.Buffer, _message.StartIndex, _message.Remsize, SocketFlags.None, ReceiveCallBack, null);
            }
            catch (Exception EX)
            {
                Logging.Debug.Log(EX+"离谱");
                Close();
            }
        }
        private void ReceiveCallBack(IAsyncResult iar)
        {
            try
            {
                if (_socket == null || _socket.Connected == false) return;
                int len = _socket.EndReceive(iar);
                Logging.Debug.Log("接收成功");

                if (len == 0)
                {
                    Logging.Debug.Log("接收数据为0");
                    Close();
                    return;
                }
                //6.接受到消息给它处理Handle
                _message.ReadBuffer(len, HandleReaquest);
                ReceiveMessage();
            }
            catch
            {
                Close();
            }
        }
        /// <summary>
        /// 使用读写队列优化
        /// </summary>
        private Queue<ByteArray> writeQueue = new Queue<ByteArray>();
        public void Send(MainPack pack)
        {
            try
            {

                byte[] sendbyte = Message.PackData(pack);
                ByteArray ba = new ByteArray(sendbyte);
                int count = 0;
                lock (writeQueue)
                {
                    writeQueue.Enqueue(ba);
                    count = writeQueue.Count;
                }
                if (count == 1)
                {
                    _socket.BeginSend(sendbyte, 0, sendbyte.Length, 0, SendBackCall, _socket);
                }
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
            }
        }
        private void SendBackCall(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);
            ByteArray ba;
            lock (writeQueue)
            {
                ba = writeQueue.Peek();
            }
            ba.ReadIdx += count;
            ///完整发送了消息
            if (ba.Length == 0)
            {
                lock (writeQueue)
                {
                    ba = null;
                    writeQueue.Dequeue();
                    if (writeQueue.Count != 0)
                        ba = writeQueue.Peek();
                }
            }

            if (ba != null)
            {
                socket.BeginSend(ba.bytes, ba.ReadIdx, ba.Length, 0, SendBackCall, socket);
            }

        }
        void HandleReaquest(MainPack pack)
        {
            //6.接受到消息给它处理Handle
            _server.HandleRequest(pack, this);
        }
        private PlayerState laststate = PlayerState.PlayerOutline;
        /// <summary>
        /// 你改变好友更新
        /// </summary>
        public void UpdateMyselfInfo()
        {
            try
            {
                ///观察者模式
               // if (laststate != PlayerState)
                {
                    Logging.Debug.Log(PlayerName + "  :  " + laststate + "    ----UpdateMyselfInfo--->  " + PlayerState);
                    //Logging.Debug.Log(FriendActiveList.Count+"   "+FriendsDic.Count);
                    foreach (Client player in FriendsDic.Values)
                    {
                        if (player.PlayerState == PlayerState.PlayerGame || player.PlayerState == PlayerState.PlayerOutline) continue;
                        Logging.Debug.Log(player.PlayerName + "  " + player.PlayerState);
                        player.UpdateActiveFriendInfo();
                        /*
                        ///退出房间
                        if (laststate == PlayerState.PlayerOnRoom && PlayerState == PlayerState.PlayerOnline)
                        {
                            //player.FriendActiveList.Add(this);
                            player.UpdateActiveFriendInfo();
                        }
                        ///进入房间
                        else if (laststate == PlayerState.PlayerOnline && PlayerState == PlayerState.PlayerOnRoom)
                        {
                            //player.FriendActiveList.Remove(this);
                            player.UpdateActiveFriendInfo();
                        }
                        ///被邀请中
                        else if (laststate == PlayerState.PlayerOnInvated && PlayerState == PlayerState.PlayerOnline)
                        {
                            //player.FriendActiveList.Add(this);
                            player.UpdateActiveFriendInfo();
                        }
                        ///拒绝邀请
                        else if (laststate == PlayerState.PlayerOnline && PlayerState == PlayerState.PlayerOnInvated)
                        {
                            //player.FriendActiveList.Remove(this);
                            player.UpdateActiveFriendInfo();
                        }
                        else if (PlayerState == PlayerState.PlayerOutline)
                        {
                            //Logging.Debug.Log(player.FriendActiveList.Count);
                            //player.FriendActiveList.Remove(this);
                            player.UpdateActiveFriendInfo();
                            //Logging.Debug.Log(player.FriendActiveList.Count);
                        }*/
                    }
                    laststate = PlayerState;
                //
                }
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
            }
        }
        /// <summary>
        /// 得到活跃好友的信息包
        /// </summary>
        /// <returns></returns>
        public MainPack GetActiveFriendInfoPack()
        {
            MainPack pack = new MainPack();
            pack.Actioncode = ActionCode.UpDateActiveFriendInfo;
            pack.Requestcode = RequestCode.FriendRoom;
            foreach (Client player in FriendsDic.Values)
            {
                Logging.Debug.Log(player.PlayerName + "  :  " + player.PlayerState);
                if (player.PlayerState != PlayerState.PlayerOnline) continue;

                PlayerPack playerPack = new PlayerPack();
                playerPack.Playername = player.PlayerName;
                playerPack.Id = player.UID;
                playerPack.State = player.PlayerState;
           
                Logging.Debug.Log(playerPack.State + "   " + player.PlayerState);
                Logging.Debug.Log(playerPack);
                pack.Playerspack.Add(playerPack);
            }
            pack.Str = "x";//防止空包
            Logging.Debug.Log("UpdatePack    " + pack);
            return pack;
        }
        /// <summary>
        /// 更新你好友状态改变
        /// </summary>
        public void UpdateActiveFriendInfo()
        {
            try
            {
                ///观察者模式:
                Logging.Debug.Log(PlayerName + "   UpdateActiveFriendInfo :  " + PlayerState);

                Send(GetActiveFriendInfoPack());

            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
            }
        }
        public void Close()
        {
            Logging.Debug.Log("client  Close||||!!!!!!!!!");
            _userdata.BordCaseToFriendLogout(_mysqlConnection, _server, this, UpdateActiveFriendInfo);
            _server.RemoveClient(this);
            _server.RemoveActiveClient(this);
            if (FriendRoom != null)
            {
                FriendRoom.Exit(_server, this);
            }
            _socket.Close();
            _mysqlConnection.Close();
            _server.RemoveClient(this);
        }
    }
}
