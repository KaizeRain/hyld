using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Google.Protobuf.Collections;
using MySql.Data.MySqlClient;
using SocketProto;

namespace Server.DAO
{

    /*
    SELECT * FROM `friends`


    查找lzj的所有好友信息
    SELECT DISTINCT u.`name`,u.`id`
    FROM `users` u
    CROSS JOIN `friends` f
    WHERE (f.`UserID`= 12 AND f.`FriendID`=u.`id`)OR(f.`FriendID`=12 AND u.`id`=f.`UserID`)

    改名
    UPDATE `users` SET `name` = '' WHERE `UserName` = 'lzj';

    添加好友
    INSERT INTO `friends` SET `UserID`= 12,`FriendID`=17; 

    获取玩家名字
    SELECT `name` FROM `users` WHERE `UserName` = 'LZJ'

    //注册
    INSERT INTO `users` SET `UserName` = 'llf',`Password`='jjy',`name`=''; 
     */

    public static class MySqlCommdString
    {
        /// <summary>
        /// 防止sql注入
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSafeString(string str)
        {
            if(!Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\{|[}|%|@|\*|!|\`]"))
            {
                return true;
            }
            Logging.Debug.Log($"{str}  不安全 ");
            return false;
        }
        /// <summary>
        /// 判断账户是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mySql"></param>
        /// <returns></returns>
        public static bool IsAccountExist(string id,MySqlConnection mySql)
        {
            if (!IsSafeString(id))
            {
                return true;
            }

            string s = $"SELECT * FROM `users` WHERE `UserName` = '{id}'";
            try
            {
                MySqlCommand cmd = new MySqlCommand(s, mySql);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasrow = dataReader.HasRows;
                dataReader.Close();
                Logging.Debug.Log("??" + id+"   "+hasrow);
                return hasrow;
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
                return true;
            }
        }
        /// <summary>
        /// 找到username的所有好友
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GetFriendList(int userid)
        {
            string res = $"SELECT DISTINCT u.`name`,u.`id` FROM `users` u CROSS JOIN `friends` f WHERE(f.`UserID`= {userid} AND f.`FriendID`= u.`id`)OR(f.`FriendID`={userid} AND u.`id`= f.`UserID`)";
            return res;
        }
        /// <summary>
        /// 找到username的玩家信息
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GetPlayerInfo(string username)
        {
            string res = $"SELECT `name`,id FROM `users` WHERE `UserName` = '{username}'";
            return res;
        }
        /// <summary>
        /// 找到name的username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GetUserName(string name)
        {
            string res = $"SELECT `UserName` FROM `users` WHERE `name` = '{name}'";
            return res;
        }
        /// <summary>
        /// 找到id的名字
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GetUserName(int userid)
        {
            string res = $"SELECT `UserName` FROM `users` WHERE `id` = '{userid}'";
            return res;
        }
        /// <summary>
        /// 找到username的name
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GetName(string username)
        {
            string res = $"SELECT `name` FROM `users` WHERE `UserName` = '{username}'";
            return res;
        }
        /// <summary>
        /// 修改username的名字为name
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string UpdateName(string username, string name)
        {
            string res = $"UPDATE `users` SET `name` = '{name}' WHERE `UserName` = '{username}';";
            return res;
        }
        public static string InsertLogon(string username, string password)
        {
            string res = $"INSERT INTO `users` SET `UserName` = '{username}',`Password`= '{password}',`name`= '';";
            return res;
        }
        /// <summary>
        /// username和friendname成为好友
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string AddFriend(int userid, int friendid)
        {
            string res = $"INSERT INTO `friends` SET `UserID`= {userid},`FriendID`={friendid};  ";
            return res;
        }
    }


    class UserData
    {
        public int UID
        {
            get; private set;
        }
        public Hero PlayerHero
        {
            get;private set;
        }
        public string UserName
        {
            get; private set;
        }
        public string PlayerName
        {
            get; private set;
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public bool Logon(MainPack pack, MySqlConnection mySqlConnection)
        {
            //1.3将账号密码信息录入数据库
            string username = pack.Loginpack.Username;
            string password = pack.Loginpack.Password;
            if (!MySqlCommdString.IsSafeString(username))
            {
                return false;
            }
            if (!MySqlCommdString.IsSafeString(password))
            {
                return false;
            }
            if (MySqlCommdString.IsAccountExist(username,mySqlConnection))
            {
                Logging.Debug.Log("该账户已存在啊！！");
                return false;
            }
            ///插入数据
            string mysql1 = MySqlCommdString.InsertLogon(username, password);
            try
            {
                MySqlCommand comd = new MySqlCommand(mysql1, mySqlConnection);
                comd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public bool Login(MainPack pack, MySqlConnection mySqlConnection)
        {
            //0.3.查寻数据库用户信息
            string username = pack.Loginpack.Username;
            string password = pack.Loginpack.Password;
            if (!MySqlCommdString.IsSafeString(username))
            {
                return false;
            }
            if (!MySqlCommdString.IsSafeString(password))
            {
                return false;
            }

            Logging.Debug.Log(username + "      " + password);
            try
            {
                //0.4.返回查询结果，记录玩家信息

                ///插入数据
                /// WHERE `UserName` =  '" + username + "' and `Password` = '" + password + "'
                string mysql1 = $"SELECT * FROM `users` WHERE `UserName` =  '" + username + "' and `Password` = '" + password + "'";
                MySqlCommand comd = new MySqlCommand(mysql1, mySqlConnection);
                MySqlDataReader read = comd.ExecuteReader();
                bool res = read.HasRows;
                Logging.Debug.Log(res);
                read.Close();
                if (res)
                {
                    UserName = username;
                }
                return res;
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
                return false;
            }
        }
        /// <summary>
        /// 查找好友信息
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="client"></param>
        /// <param name="mySqlConnection"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public bool FindFriendsInfo(ref MainPack pack,Client client, MySqlConnection mySqlConnection,Server server)
        {
            Logging.Debug.Log(UserName + " ??? FindFriendsInfo  ??:"+UID);
            string mysql1 = MySqlCommdString.GetFriendList(UID);
            try
            {
                MySqlCommand comd = new MySqlCommand(mysql1, mySqlConnection);
                MySqlDataReader read = comd.ExecuteReader();
                object[] myobj = new object[read.FieldCount];
                string[] res = new string[read.FieldCount];
                PlayerPack myinfo = new PlayerPack
                {
                    Playername = PlayerName,
                    Id = UID,
                    State = PlayerState.PlayerOnline
                };
                while (read.Read())
                {
                    read.GetValues(myobj);
                    
                    PlayerPack playerinfo = new PlayerPack
                    {
                        Playername = myobj[0].ToString(),
                        Id = (int)myobj[1],
                        State = server.GetPlayerState((int)myobj[1])
                    };

                    //如果好友在线，就告诉他，老子上号了！
                    if (playerinfo.State != PlayerState.PlayerOutline)
                    {
                        MainPack SendToFriendpack = new MainPack();
                        SendToFriendpack.Returncode = ReturnCode.Succeed;
                        SendToFriendpack.Requestcode = RequestCode.Friend;
                        SendToFriendpack.Actioncode = ActionCode.FriendLogin;
                        SendToFriendpack.UserInfopack = myinfo;
                        Client friendclient = server.GetActiveClient(playerinfo.Id);
                        client.FriendsDic.Add(friendclient.UID, friendclient);
                        friendclient.FriendsDic.Add(client.UID,client);
                        //friendclient.FriendActiveList.Add(client);
                        //client.FriendActiveList.Add(friendclient);
                        friendclient.UpdateActiveFriendInfo();
                        //friendclient.Send(SendToFriendpack);
                    }

                    pack.Friendspack.Add(playerinfo);
          
                    foreach (object obj in myobj)
                    {
                        Logging.Debug.Log(obj.ToString() + "\t\n");
                    }
                }
                //TODO:这里可能要加入状态回调
                read.Close();
                return true;
            }
            catch(Exception ex)
            {
                Logging.Debug.Log(ex);
                return false;
            }
            
        }
        /// <summary>
        ///查找玩家信息
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool FindPlayerInfo(ref MainPack pack,Client client)
        {
            try
            {
                MySqlConnection mySqlConnection = client.GetMysqlConnecet;
                string username = pack.Loginpack.Username;
                Logging.Debug.Log(username + "  FindPlayerInfo");
                string mysql1 = MySqlCommdString.GetPlayerInfo(username);
                MySqlCommand comd = new MySqlCommand(mysql1, mySqlConnection);
                MySqlDataReader read = comd.ExecuteReader();
                object[] myobj = new object[read.FieldCount];
                string[] res = new string[read.FieldCount];
                while (read.Read())
                {
                    read.GetValues(myobj);
                    PlayerPack playerinfo = new PlayerPack();
                    playerinfo.Username = username;
                    UserName = username;
                    playerinfo.Playername = myobj[0].ToString();
                    PlayerName = playerinfo.Playername;
                    playerinfo.Id = (int)myobj[1];
                    UID = playerinfo.Id;
                    pack.UserInfopack = playerinfo;
                    foreach (object obj in myobj)
                    {
                        Logging.Debug.Log(obj.ToString() + "\t\n");
                    }
                }
                read.Close();
                if (res == null) return false;
                PlayerHero = (Hero)Enum.Parse(typeof(Hero), pack.Str);
                client.PlayerState = PlayerState.PlayerOnline;
                client.UpdateMyselfInfo();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
                return false;
            }
        }

        public void ChangeHero(MainPack pack)
        {
            PlayerHero = pack.UserInfopack.Hero;
        }
        /// <summary>
        /// 更新名字
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public bool UpdateName(MainPack pack, MySqlConnection mySqlConnection)
        {
            string username = pack.Loginpack.Username;
            Logging.Debug.Log(username + "UpdateName:  " + pack.Str);
            string mysql1 = MySqlCommdString.UpdateName(username, pack.Str);
            MySqlCommand comd = new MySqlCommand(mysql1, mySqlConnection);

            try
            {
                comd.ExecuteNonQuery();//
                UserName = username;
                PlayerName = pack.Str;
                return true;
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex.Message);
                return false;
            }
        }
        //*********************加好友*************************//
        /// <summary>
        /// 申请加好友
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public bool AplyAddFriend(MainPack pack, MySqlConnection mySqlConnection, Server server)
        {
            string Playername = pack.UserInfopack.Playername;
            Logging.Debug.Log(Playername + "  !!!AplyAddFriend!!!:  " + pack.Str);
            ///根据姓名查找好友账号
            try
            {
                string mysql1 = MySqlCommdString.GetUserName(int.Parse(pack.Str));
                MySqlCommand comd = new MySqlCommand(mysql1, mySqlConnection);
                MySqlDataReader read = comd.ExecuteReader();
                object[] myobj = new object[read.FieldCount];
                string res = "";
                while (read.Read())
                {
                    read.GetValues(myobj);
                    foreach (object obj in myobj)
                    {
                        Logging.Debug.Log(obj + "\t\n");
                        res = (string)obj;
                    }
                }
                read.Close();
                if (res != "")
                {
                    MainPack SendToFriendpack = new MainPack();
                    SendToFriendpack.Returncode = ReturnCode.AddFriend;
                    SendToFriendpack.Actioncode = ActionCode.AcceptAddFriend;
                    SendToFriendpack.Str = Playername;
                    server.GetClientByUserName(res).Send(SendToFriendpack);
                    Logging.Debug.Log("AplyAddFriend!!!!:  " + pack);
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Logging.Debug.Log("ex  :" + ex.Message);
                return false;
            }

        }
        /// <summary>
        /// 同意加好友
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public bool AcceptAddFriend(ref MainPack pack, Client client,MySqlConnection mySqlConnection, Server server)
        {
            int userid = pack.UserInfopack.Id;
            Logging.Debug.Log(" AcceptAddFriend:  " + pack.Str);
            Client friend = server.GetClientByPlayerName(pack.Str);
            string mysql1 = MySqlCommdString.AddFriend(userid, friend.UID);
            MySqlCommand comd = new MySqlCommand(mysql1, mySqlConnection);
            try
            {
                comd.ExecuteNonQuery();
                MainPack SendToFriendpack = new MainPack();
                SendToFriendpack.Returncode = ReturnCode.Succeed;
                SendToFriendpack.Actioncode = ActionCode.AcceptAddFriend;
                SendToFriendpack.Str = PlayerName+"#"+ server.GetClientByPlayerName(PlayerName).UID;
                PlayerPack myinfo = new PlayerPack
                {
                    Playername = PlayerName,
                    Id = UID,
                    State = client.PlayerState
                };
                PlayerPack friendinfo = new PlayerPack
                {
                    Playername = PlayerName,
                    Id = UID,
                    State = client.PlayerState
                };
               // friend.FriendActiveList.Add(client);
                //client.FriendActiveList.Add(friend);
                ///TODO：同意加好友可能要加入状态回滚
                friend.Send(SendToFriendpack);
                pack.Str = friend.PlayerName + "#" + server.GetClientByPlayerName(friend.PlayerName).UID;
                //Logging.Debug.Log($"!!!!!!!!!!!\nUser: {SendToFriendpack.Str} ----- friend : {pack.Str} !!!!!!\n ");
                //Logging.Debug.Log("AcceptAddFriend!!!!:  " + pack);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 拒绝加好友
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public bool RejectAddFriend(MainPack pack, MySqlConnection mySqlConnection, Server server)
        {
            int userid = pack.UserInfopack.Id;
            Logging.Debug.Log(" RejectAddFriend:  " + pack.Str);
            Client friend = server.GetClientByPlayerName(pack.Str);
            MainPack SendToFriendpack = new MainPack();
            SendToFriendpack.Returncode = ReturnCode.Fail;
            SendToFriendpack.Actioncode = ActionCode.RejectAddFriend;
            friend.Send(SendToFriendpack);
            return false;
        }
        public void BordCaseToFriendLogout(MySqlConnection mySqlConnection, Server server,Client client,Action UpdateActiveFriendInfo)
        {
            Logging.Debug.Log(UserName + "  BordCaseToFriendLogout  :" + UID);
            string mysql1 = MySqlCommdString.GetFriendList(UID);
            try
            {
                MySqlCommand comd = new MySqlCommand(mysql1, mySqlConnection);
                PlayerPack myinfo = new PlayerPack();
                if (PlayerName == null)
                {
                    return;
                }
                myinfo.Playername = PlayerName;
                myinfo.Id = UID;
                myinfo.State = PlayerState.PlayerOutline;


                MySqlDataReader read = comd.ExecuteReader();
                object[] myobj = new object[read.FieldCount];
                string[] res = new string[read.FieldCount];
                while (read.Read())
                {
                    read.GetValues(myobj);

                    PlayerPack playerinfo = new PlayerPack
                    {
                        Playername = myobj[0].ToString(),
                        Id = (int)myobj[1],
                        State = server.GetPlayerState((int)myobj[1])
                    };
                    //如果好友在线，就告诉他，老子下号了！
                    if (playerinfo.State != PlayerState.PlayerOutline)
                    {
                        Client friendclient = server.GetActiveClient(playerinfo.Id);
                        friendclient.FriendsDic.Remove(client.UID);
                        foreach (object obj in myobj)
                        {
                            Logging.Debug.Log(obj.ToString() + "\t\n");
                        }
                    }
                }
                client.PlayerState = PlayerState.PlayerOutline;
                client.UpdateMyselfInfo();
                read.Close();
            }
            catch (Exception ex)
            {
                Logging.Debug.Log(ex);
            }
        }
    }
}
