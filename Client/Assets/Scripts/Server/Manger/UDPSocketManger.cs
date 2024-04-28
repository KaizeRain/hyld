using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Net.Sockets;
using SocketProto;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Google.Protobuf;
using Logging;
namespace Server
{
    class UDPSocketManger
    {
        private Socket client;
        //private Socket _socket => _client.Client;
        private static UDPSocketManger instance;
        public Action<MainPack> Handle;
        private IPEndPoint _localEnd;
        //private int localPort;
        private byte[] bytes;
        public static UDPSocketManger Instance
        {
            get
            {
                // 如果类的实例不存在则创建，否则直接返回
                if (instance == null)
                {
                    instance = new UDPSocketManger();
                }
                return instance;
            }
        }
        /// <summary>
        /// 初始化Socket
        /// </summary>
        public string InitSocket()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                client.Connect(NetConfigValue.ServiceIP, NetConfigValue.ServiceUDPPort);
                _localEnd = (IPEndPoint)client.LocalEndPoint;
              //  localPort = _localEnd.Port;
               // Debug.LogError(_localEnd.Address.ToString()+"  "+_localEnd.ToString());
                //_socket.Connect(new EndPoint(ip,6666))
               // Logging.HYLDDebug.Log(_localEnd+" 连接成功 " + NetConfigValue.ServiceIP + " "+NetConfigValue.ServiceUDPPort);
                Logging.HYLDDebug.Trace($"UDP连接成功  本地:{client.LocalEndPoint}   服务器:{client.RemoteEndPoint}");
                //HYLDManger.Instance.ShowMessage("连接成功");
                //HYLDStaticValue.是否为连接状态 = true;

                Thread.Sleep(100);
                (new Thread(ReceiveMsg)).Start();
                //连接成功
            }
            catch (Exception e)
            {
                //连接失败
                //Logging.HYLDDebug.LogError(e);
                Logging.HYLDDebug.Log("连接失败" + e);
                HYLDManger.Instance.ShowMessage("连接失败");
                HYLDStaticValue.是否为连接状态 = false;
            }
            return _localEnd.ToString();
        }
        private void ReceiveMsg()
        {
            Logging.HYLDDebug.LogError("开始接收");
           // IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(NetConfigValue.ServiceIP), localPort);

            //Socket client = o as Socket;
            while (true)
            {
                try
                {
                  //  Logging.HYLDDebug.LogError("udp接受数据中 "+localPort);//
                    //bytes = _client.Receive(ref endpoint);
                  


                    ///用来保存发送方的ip和端口号
                    EndPoint point = new IPEndPoint(IPAddress.Any, 0);
                    ///定义客户端接收到的信息大小
                    byte[] bytes = new byte[1024];
                    ///接收到的信息大小(所占字节数)
                    int length = client.Receive(bytes);
                    if (length == 0) return;
                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(bytes, 0, length);
                    //Logging.HYLDDebug.LogError("udp接受数据ing " + pack);//
                    Handle?.Invoke(pack);
                }
                catch(Exception ex)
                {

                    //_client = new UdpClient();
                    Logging.HYLDDebug.Trace($"udpClient  {_localEnd }接收数据异常:"+ex.Message, true);
                    Debug.LogError("udpClient接收数据异常:" + ex.Message+"   "+ _localEnd);
                   // Logging.HYLDDebug.Trace("udp发送失败:" + ex.Message);
                    // //_client.Connect(NetConfigValue.ServiceIP, NetConfigValue.ServiceUDPPort);
                    //IPEndPoint _localEnd = (IPEndPoint)_client.Client.LocalEndPoint;
                    //localPort = _localEnd.Port;
                    //_socket.Connect(new EndPoint(ip,6666))
                    //Logging.HYLDDebug.Log("连接成功 " + localPort);
                    //HYLDManger.Instance.ShowMessage("连接成功");
                }
            }
        }
        /// <summary>
        /// 发送玩家操作
        /// </summary>
        public void SendOperation()
        {



            MainPack pack=new MainPack();
            pack.Requestcode = RequestCode.Battle;
            pack.Actioncode = ActionCode.BattlePushDowmPlayerOpeartions;
            pack.BattleInfo = new BattleInfo();
            pack.BattleInfo.SelfOperation = Manger.BattleData.Instance.selfOperation;
            pack.BattleInfo.OperationID = Manger.BattleData.Instance.sync_frameID+1;//我认为”的下一帧
            
            //if (pack.BattleInfo.SelfOperation.OpType ==  OperationType.Move)
            {
               // Logging.HYLDDebug.LogError("SendOperation:" + pack);
            }
            //Logging.HYLDDebug.LogError(" Send_operation()" +pack);
            Send(pack);
        }

        public void Send(MainPack pack)
        {
            byte[] sendbuff = pack.ToByteArray();
            try
            {
                ///获取IP与端口号
                EndPoint point = new IPEndPoint(IPAddress.Parse(NetConfigValue.ServiceIP), NetConfigValue.ServiceUDPPort);
                ///发送内容
               // string msg = textBox3.Text;
                ///将数据发送到指定的ip的主机的端口
                client.SendTo(sendbuff, point);


                //client.Send(sendbuff, sendbuff.Length);
                //				sendClient.Send (_mes,_mes.Length,sendEndPort);	
                //				Logging.HYLDDebug.Log("发送量:" + _mes.Length.ToString());
                //				Logging.HYLDDebug.Log("udp发送量：" + _mes.Length);
            }
            catch (Exception ex)
            {
                Logging.HYLDDebug.Log("udp发送失败:" + ex.Message);
                Logging.HYLDDebug.Trace("udp发送失败:" + ex.Message);
            }
        }
    }
}
