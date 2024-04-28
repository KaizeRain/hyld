using Google.Protobuf.Collections;
using SocketProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class FriendRoom
    {
        private FriendRoomPack _friendroomInfo;
        private Server _server;
        private List<Client> _clientsList = new List<Client>();//房间内所有客户端

        /// <summary>
        /// 获取房间信息
        /// </summary>
        public FriendRoomPack GetRoomInfo
        {
            get 
            {
                _friendroomInfo.Curnum = _clientsList.Count;
                return _friendroomInfo;
            }
        }
        public string RoomID
        {
            get 
            {
                return _friendroomInfo.Roomid;
            }
        }

        public FriendRoom(Client client, FriendRoomPack pack, Server server)
        {
            _friendroomInfo = pack;
            _clientsList.Add(client);
            client.FriendRoom = this;
            _server = server;
        }

        /// <summary>
        /// 获取当前房间所有用户信息
        /// </summary>
        /// <returns></returns>
        public RepeatedField<PlayerPack> GetPlayerInfo()
        {
            RepeatedField<PlayerPack> playerPacks = new RepeatedField<PlayerPack>();
            foreach (Client c in _clientsList)
            {
                PlayerPack playerPack = new PlayerPack();
                playerPack.Playername = c.PlayerName;
                playerPack.Id = c.UID;
                playerPack.Hero = c.PlayerHero;
                playerPacks.Add(playerPack);
            }
            return playerPacks;
        }


        public void BroadCastTCP(Client client, MainPack pack)
        {
            foreach (Client c in _clientsList)
            {
                if (!c.Equals(client))
                {
                    c.Send(pack);
                }
            }
        }
        //需要用UDP广播
        public void BroadCastUDP(Client client, MainPack pack)
        {
            foreach (Client c in _clientsList)
            {
                if (!c.Equals(client))
                {
                    c.Send(pack);
                }
            }
        }

        public void Join(Client client)
        {
            _clientsList.Add(client);
            if (_clientsList.Count >= _friendroomInfo.Maxnum)
            {
                //满人了
                _friendroomInfo.State = RoomState.RoomFull;
            }
            client.FriendRoom = this;
            MainPack pack = new MainPack();
            pack.Actioncode = ActionCode.JoinRoom;
            pack.Returncode = ReturnCode.Succeed;
            pack.Requestcode = RequestCode.FriendRoom;
            foreach (PlayerPack player in GetPlayerInfo())
            {
                pack.Playerspack.Add(player);
            }
            BroadCastTCP(client, pack);
        }
        public void Exit(Server server, Client client)
        {
            MainPack pack = new MainPack();
            if (_friendroomInfo.State== RoomState.RoomGame)//游戏已经开始
            {
                ExitGame(client);
            }
            else//游戏未开始
            {
                _clientsList.Remove(client);
                client.PlayerState = PlayerState.PlayerOnline;
                /*
                Logging.Debug.Log("Start Find  FriendsDic:\n");
                foreach (Client friend in client.FriendsDic.Values)
                {
                    Logging.Debug.Log(friend.PlayerName+":  "+friend.FriendActiveList.Count);
                    client.FriendActiveList.Add(friend);
                }
                Logging.Debug.Log("End Find  FriendsDic:\n");*/
                client.UpdateMyselfInfo();
                if (_clientsList.Count == 0)
                {
                    server.RemoveFriendRoom(this);
                    client.UpdateActiveFriendInfo();
                    return;
                }
                _friendroomInfo.State = RoomState.RoomNormal;
                client.FriendRoom = null;
                pack.Requestcode = RequestCode.FriendRoom;
                pack.Returncode = ReturnCode.Succeed;
                pack.Actioncode = ActionCode.ExitRoom;
                pack.Str = "Have a Friend Exit";
                foreach (PlayerPack player in GetPlayerInfo())
                {
                    pack.Playerspack.Add(player);
                }
                BroadCastTCP(client, pack);
            }
        }


        private void Time()
        {
            /*
            MainPack pack = new MainPack();
            pack.Actioncode = ActionCode.Chat;
            pack.Str = "房主已启动游戏...";
            Broadcast(null, pack);
            Thread.Sleep(1000);
            for (int i = 5; i > 0; i--)
            {
                pack.Str = i.ToString();
                Broadcast(null, pack);
                Thread.Sleep(1000);
            }
    

            pack.Actioncode = ActionCode.Starting;


            foreach (var VARIABLE in clientList)
            {
                PlayerPack player = new PlayerPack();
                VARIABLE.GetUserInFo.HP = 100;
                player.Playername = VARIABLE.GetUserInFo.UserName;
                player.Hp = VARIABLE.GetUserInFo.HP;
                pack.Playerpack.Add(player);
            }
            Broadcast(null, pack);
                    */
        }

        public void ExitGame(Client client)
        {
            /*
            MainPack pack = new MainPack();
            if (client == clientList[0])
            {
                //房主退出
                pack.Actioncode = ActionCode.ExitGame;
                pack.Str = "r";
                Broadcast(client, pack);
                server.RemoveRoom(this);
                client.GetRoom = null;
            }
            else
            {
                //其他成员退出
                clientList.Remove(client);
                client.GetRoom = null;
                pack.Actioncode = ActionCode.UpCharacterList;
                foreach (var VARIABLE in clientList)
                {
                    PlayerPack playerPack = new PlayerPack();
                    playerPack.Playername = VARIABLE.GetUserInFo.UserName;
                    playerPack.Hp = VARIABLE.GetUserInFo.HP;
                    pack.Playerpack.Add(playerPack);
                }
                pack.Str = client.GetUserInFo.UserName;
                Broadcast(client, pack);
            }
            */
        }

        public ReturnCode StartGame(Client client)
        {
            /*
            if (client != clientList[0])
            {
                return ReturnCode.Fail;
            }
            roominfo.Statc = 2;
            Thread starttime = new Thread(Time);
            starttime.Start();
            Logging.Debug.Log("开始游戏");
            */
            return ReturnCode.Succeed;
            
        }
    }
}
