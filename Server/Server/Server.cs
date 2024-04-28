using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Server.Controller;
using MySql.Data.MySqlClient;
using Server.DAO;
using SocketProto;
using System.Reflection;

namespace Server
{
    class Server
    {
        private Socket _socket;
        private List<Client> _clients = new List<Client>();
        private ControllerManger _controllerManger;
        private Dictionary<int, Client> _activeClient = new Dictionary<int, Client>();
        public Dictionary<int, List<int>> DIC_BattlePlayerUIDs = new Dictionary<int, List<int>>();
        public Dictionary<int, int> DIC_BattleIDs = new Dictionary<int,int>();
        public EndPoint EndPoint
        {
            get { return _socket.LocalEndPoint; }
        } 
        public Server(int port)
        {
            //socket()—>bind()—>listen()—>accept()—>recv()/recvfrom()—>send()/sendto(); 
            //1.创建ControlManger
            //启动服务器启动TCP监听线程
            //通过Clientsoket发送数据 
            //启动UDP监听线程
            _controllerManger = new ControllerManger(this);


            IPAddress ip = IPAddress.Parse("0.0.0.0");
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(ip, port));
            _socket.Listen(10);
            //创建udp线程
            LZJUDP.Instance.Init();

            //启动tcp监听线程
            Thread myThread = new Thread(ListenClientConnect);
            myThread.Start();
            Thread myThread1 = new Thread(OnTimer);
            myThread1.Start();

            Logging.Debug.Log("启动监听{0}成功", _socket.LocalEndPoint.ToString());
            Logging.Debug.Trace($"启动监听{_socket.LocalEndPoint.ToString()}成功", true);
        }
        public Client GetActiveClient(int id)
        {
            if (_activeClient.ContainsKey(id))
            {
                return _activeClient[id];
            }
            return null;
        }
        public void AddActiveClient(int id, Client client)
        {
            _activeClient.Add(id, client);
        }
        public void RemoveActiveClient(Client client)
        {
            _controllerManger.CloseClient(client,client.UID); //_activeClient
            _activeClient.Remove(client.UID);
        }
        public PlayerState GetPlayerState(int id)
        {
            if (GetActiveClient(id) != null)
            {
                return _activeClient[id].PlayerState;
            }
            return PlayerState.PlayerOutline;
        }
        public Client GetClientByUserName(string username)
        {
            foreach (Client client in _clients)
            {
                if (client.UserName == username)
                {
                    return client;
                }
            }
            return null;
        }
        public Client GetClientByID(int id)
        {
            foreach (Client client in _clients)
            {
                if (client.UID == id)
                {
                    return client;
                }
            }
            return null;
        }
        public Client GetClientByPlayerName(string playername)
        {
            foreach (Client client in _clients)
            {
                if (client.PlayerName == playername)
                {
                    return client;
                }
            }
            return null;
        }

        public void HandleRequest(MainPack pack, Client client)
        {
            //6.接受到消息给它处理Handle
            _controllerManger.HandleRequest(pack, client);
        }
        public void RemoveClient(Client client)
        {
            _clients.Remove(client);
        }


        /// <summary> 
        /// 监听客户端连接 
        /// </summary> 
        private void ListenClientConnect()
        {
            while (true)
            {
                try
                {
                    //3.为新连接的客户端new Client()并加入到_clients集合
                    Socket clientSocket = _socket.Accept();
                    _clients.Add(new Client(clientSocket, this));
                }
                catch (Exception ex)
                {
                    Logging.Debug.Log(ex);
                }

            }
        }
#region EventHandler回调
        public void OnTimer()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (Tool.PingPongTool.isUserPing)
                    CheckPing();
            }
        }
        public void CheckPing()
        {

            long timenow = Tool.PingPongTool.GetTimeStamp();

            foreach (Client c in _clients)
            {
               // Logging.Debug.Log(timenow +"  +  "+ c.lastPingTime + " = "+(timenow - c.lastPingTime));
                if (timenow - c.lastPingTime > Tool.PingPongTool.pingInterval * 4)
                {
                    Logging.Debug.Log("Ping Close " + c._socket.RemoteEndPoint.ToString());
                    c.Close();
                    return;
                }
            }
        }

#endregion







        /************************处理好友房间********************/
        #region 
        private List<FriendRoom> _friendRoomList = new List<FriendRoom>();

        public MainPack CreateFriendRoom(Client client, MainPack pack)
        {
            try
            {
                FriendRoom room = new FriendRoom(client, pack.Friendroompack[0], this);
                _friendRoomList.Add(room);
                foreach (PlayerPack p in room.GetPlayerInfo())
                {
                    pack.Playerspack.Add(p);
                }
                client.PlayerState = PlayerState.PlayerOnRoom;
                client.UpdateMyselfInfo();
                // Logging.Debug.Log("CreateFriendRoom!!!!!\n");
                if (InvateToRoom(client, pack.Str, pack.Friendroompack[0]))
                {
                    pack.Returncode = ReturnCode.Succeed;
                }
                else pack.Returncode = ReturnCode.Fail;
                //Logging.Debug.Log(pack);
                return pack;
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
                pack.Returncode = ReturnCode.Fail;
                return pack;
            }
        }
        public void CancalInvateFriend(MainPack pack)
        {
            Client friend = GetClientByPlayerName(pack.Str);
            Logging.Debug.Log(friend.PlayerName);
            Logging.Debug.Log("CancalInvatingFreind  :  " + pack);
            pack.Returncode = ReturnCode.Succeed;
            pack.Actioncode = ActionCode.CancalInvateFriend;
            if (friend.FriendRoom != null) pack.Returncode = ReturnCode.Fail;
            pack.Str = "?";
            Logging.Debug.Log("to  \n\n" + pack);
            friend.Send(pack);
        }
        public bool InvateToRoom(Client client, string friendname, FriendRoomPack friendRoomPack)
        {
            Client friendclient = GetClientByPlayerName(friendname);
            Logging.Debug.Log(friendclient.UserName);
            if (friendclient != null)
            {
                Logging.Debug.Log(_activeClient.Count);
                if (_activeClient.ContainsValue(friendclient))
                {
                    friendclient.PlayerState = PlayerState.PlayerOnInvated;
                    friendclient.UpdateMyselfInfo();
                    // Logging.Debug.Log("?????");
                    MainPack pack = new MainPack();
                    pack.Actioncode = ActionCode.InviteFriend;
                    pack.Requestcode = RequestCode.FriendRoom;
                    pack.Returncode = ReturnCode.Succeed;
                    pack.Str = client.PlayerName;
                    pack.Friendroompack.Add(friendRoomPack);
                    //Logging.Debug.Log("@!@#!#!#!");
                    friendclient.Send(pack);
                    // Logging.Debug.Log("!!!!");
                    return true;
                }
            }
            return false;
        }
        public MainPack AcceptInvateFriend(Client client, MainPack pack,Server server)
        {
            pack = JoinFriendRoom(client, pack,server);
            Logging.Debug.Log(pack);
            return pack;
        }
        public MainPack RejectInvateFriend(Client client, MainPack pack)
        {
            try
            {
                foreach (FriendRoom r in _friendRoomList)
                {
                    if (r.GetRoomInfo.Roomid.Equals(pack.Str))
                    {
                        pack.Returncode = ReturnCode.Succeed;
                        Logging.Debug.Log("RejectInvateFriend  AC:  \n\n" + pack);
                        
                        //Logging.Debug.Log(GetClientByPlayerName(r.GetPlayerInfo()[0].Playername));
                        foreach (PlayerPack playerPack in r.GetPlayerInfo())
                        {
                            Logging.Debug.Log(playerPack);
                            Client friendClient = GetClientByPlayerName(playerPack.Playername);
                            pack.UserInfopack = new PlayerPack();
                            pack.UserInfopack.Id = client.UID;
                            friendClient.Send(pack);
                        }

                        Logging.Debug.Log("????????????");
                        pack.Actioncode = ActionCode.ActionNone;
                        client.PlayerState = PlayerState.PlayerOnline;
                        client.UpdateMyselfInfo();
                        return pack;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
                Logging.Debug.Log("RejectInvateFriend  WA:  \n\n" + pack);
                pack.Returncode = ReturnCode.Fail;
            }

            return pack;
        }
        public MainPack JoinFriendRoom(Client client, MainPack pack,Server server)
        {
            Logging.Debug.Log("JoinFriendRoom\n");
            try
            {
                foreach (FriendRoom r in _friendRoomList)
                {
                    if (r.GetRoomInfo.Roomid.Equals(pack.Str))
                    {
                        if (r.GetRoomInfo.State == RoomState.RoomNormal)
                        {

                            //可以加入房间
                            r.Join(client);


                            pack.Friendroompack.Add(r.GetRoomInfo);
                            foreach (PlayerPack p in r.GetPlayerInfo())
                            {
                                pack.Playerspack.Add(p);
                            }
                            pack.Returncode = ReturnCode.Succeed;
                            client.PlayerState = PlayerState.PlayerOnRoom;
                            client.UpdateMyselfInfo();
                            client.UpdateActiveFriendInfo();
                            return pack;
                        }
                        else
                        {
                            client.PlayerState = PlayerState.PlayerOnline;
                            client.UpdateMyselfInfo();
                            client.UpdateActiveFriendInfo();
                            //房间不可加入
                            pack.Returncode = ReturnCode.Fail;
                            return pack;
                        }
                    }
                }
                client.PlayerState = PlayerState.PlayerOnline;
                client.UpdateMyselfInfo();
                client.UpdateActiveFriendInfo();
                ////没有此房间
                pack.Returncode = ReturnCode.NotRoom;
                return pack;
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
                pack.Returncode = ReturnCode.Fail;
                return pack;
            }
           

        }

        public MainPack ExitFriendRoom(Client client, MainPack pack)
        {
            if (client.FriendRoom == null)
            {
                pack.Returncode = ReturnCode.Fail;
                return pack;
            }

            client.FriendRoom.Exit(this, client);
            pack.Actioncode = ActionCode.ActionNone;
            pack.Returncode = ReturnCode.Succeed;
            return pack;
        }
        public void RemoveFriendRoom(FriendRoom room)
        {
            _friendRoomList.Remove(room);
            room = null;
            //Memory.ClearMemory();
        }
        public void Chat(Client client, MainPack pack)
        {
            client.FriendRoom.BroadCastTCP(client, pack);
        }
        public void ChangeHero(Client client, MainPack pack)
        {
            pack.Requestcode = RequestCode.FriendRoom;
            pack.Returncode = ReturnCode.Succeed;
            pack.Actioncode = ActionCode.UpDateActiveFriendInfo;
            pack.UserInfopack = new PlayerPack();
            pack.UserInfopack.Id = client.UID;
            pack.UserInfopack.Playername = client.PlayerName;
            pack.UserInfopack.Hero = client.PlayerHero;
            pack.Str = "ChangeHero";
            client.FriendRoom.BroadCastTCP(client, pack);
        }
        #endregion
    }
}
