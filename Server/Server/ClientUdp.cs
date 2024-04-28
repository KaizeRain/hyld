using Google.Protobuf;
using SocketProto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
	/*
	public class UdpManager
	{
		private static UdpManager singleInstance;
		private static readonly object padlock = new object();

		public UdpClient _udpClient = null;
		public int recvPort;
		public static UdpManager Instance
		{
			get
			{
				lock (padlock)
				{
					if (singleInstance == null)
					{
						singleInstance = new UdpManager();
					}
					return singleInstance;
				}
			}
		}

		private UdpManager()
		{
			CreatUdp();
		}

		public void Creat()
		{

		}
		public const int SIO_UDP_CONNRESET = -1744830452;
		void CreatUdp()
		{
			_udpClient = new UdpClient(ServerConfig.UDPservePort);
			
			IPEndPoint _localip = (IPEndPoint)_udpClient.Client.LocalEndPoint;
			_udpClient.Client.IOControl(
			(IOControlCode)SIO_UDP_CONNRESET,
			new byte[] { 0, 0, 0, 0 },
			null
		);
			Logging.Debug.Log("udp端口:" + _localip.Port);
			recvPort = _localip.Port;
		}

		public void Destory()
		{

			CloseUdpClient();
			singleInstance = null;
		}

		public void CloseUdpClient()
		{
			if (_udpClient != null)
			{
				_udpClient.Close();
				_udpClient = null;
			}
		}

		public UdpClient GetClient()
		{
			if (_udpClient == null)
			{
				CreatUdp();
			}
			return _udpClient;
		}


	}


	public class ClientUdp
	{
		public int userUid;
		private int sendPortNum;
		private UdpClient sendClient = null;
		private IPEndPoint sendEndPort;
		private bool isRun;
		private string serverIp;
		private Action<MainPack> Handle;

		public ClientUdp(string _ip, int _uid,Action<MainPack> Handle)
		{

			this.Handle = Handle;
			if (sendEndPort != null)
			{
				Logging.Debug.Log("客户端udp已经启动~");
				return;
			}

			userUid = _uid;
			serverIp = _ip;
			isRun = true;

			sendClient = UdpManager.Instance.GetClient();
			Logging.Debug.Log("建立UDP连接  ip: " + _ip +"  uid: " + _uid);
			//		sendClient = new UdpClient(NormalData.recvPort);
			//		sendEndPort = new IPEndPoint(IPAddress.Parse(_ip), ServerConfig.udpRecvPort);	
			/*
			int IOC_IN = 0x80000000;
			uint IOC_VENDOR = 0x18000000;
			uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
			ClientSocket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
	
			Thread t = new Thread(new ThreadStart(RecvThread));
			t.Start();


		}


		public void EndClientUdp()
		{
			try
			{
				isRun = false;
				if (sendEndPort != null)
				{
					UdpManager.Instance.CloseUdpClient();
					sendClient = null;
					sendEndPort = null;
				}
				Handle = null;
			}
			catch (Exception ex)
			{
				Logging.Debug.Log("udp连接关闭异常:" + ex.Message);
			}

		}

		private void CreatSendEndPort(int _port)
		{
			Logging.Debug.Log("CreateEndPort " + _port);
			sendEndPort = new IPEndPoint(IPAddress.Parse(serverIp), _port);
		}

		public void Send(MainPack pack)
		{
			if (isRun)
			{
				try
				{
					Logging.Debug.Log("UDP Send   "+sendEndPort.Port +"         " + pack);
					byte[] sendbuff = pack.ToByteArray();
					sendClient.Send(sendbuff, sendbuff.Length, sendEndPort);
					//				GameData.Instance().sendNum+=_mes.Length;
					//				Logging.Debug.Log("发送量:" + _mes.Length.ToString() + "," + GameData.Instance().sendNum.ToString());
				}
				catch (Exception ex)
				{
					Logging.Debug.Log("udp发送失败:" + ex.Message);
				}

			}
		}


		public void RecvClientReady(int _userUid)
		{
			if (_userUid == userUid) //&& sendEndPort == null)
			{
				CreatSendEndPort(sendPortNum);
			}
		}
	
		// 接收线程不断 监听客户端的消息
		private void RecvThread()
		{

			IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(serverIp), UdpManager.Instance.recvPort);
			while (isRun)
			{
				try
				{

					//sendClient.Client.ReceiveFrom(bytes,ref endpoint);
					byte[] bytes = sendClient.Receive(ref endpoint);

					if (sendEndPort == null)
					{
						//Logging.Debug.Log("接收客户端udp信息:" + endpoint.Port);
						sendPortNum = endpoint.Port;
					}
					/*
					byte packMessageId = buf[PackageConstant.PackMessageIdOffset];     //消息id (1个字节)
					Int16 packlength = BitConverter.ToInt16(buf, PackageConstant.PacklengthOffset);  //消息包长度 (2个字节)
					int bodyDataLenth = packlength - PackageConstant.PacketHeadLength;
					byte[] bodyData = new byte[bodyDataLenth];
					Array.Copy(buf, PackageConstant.PacketHeadLength, bodyData, 0, bodyDataLenth);
	
					delegate_analyze_message((PBCommon.CSID)packMessageId, bodyData);
									
					//bytes = _client.Receive(ref endpoint);
					MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(bytes, 0, bytes.Length);
					Logging.Debug.Log(endpoint.Address + "  " + endpoint.Port+" udp接受数据 " + pack+"   ");
					Handle?.Invoke(pack);

					//是客户端,统计接收量
					//				GameData.Instance().recvNum+=buf.Length;
					//				Logging.Debug.Log("发送量:" + buf.Length.ToString() + "," + GameData.Instance().recvNum.ToString());
				}
				catch (Exception ex)
				{
					
					Logging.Debug.Log(endpoint.Address+"  "+endpoint.Port + ":::udpClient接收数据异常:  " + ex.Message);
				}
			}
			Logging.Debug.Log("udp接收线程退出~~~~~");
		}


		void OnDestroy()
		{
			EndClientUdp();
		}
	}
	*/

	public class LZJUDP
	{
		private Socket server;
		private Action<MainPack> Handle;
		private static LZJUDP singleInstance;
		private static readonly object padlock = new object();
		public const int SIO_UDP_CONNRESET = -1744830452;
		public void AddListenRecv(Action<MainPack> action)
		{
			Handle = action;
		}
		//public UdpClient _udpClient = null;
		//public int recvPort;
		public static LZJUDP Instance
		{
			get
			{
				lock (padlock)
				{
					if (singleInstance == null)
					{
						singleInstance = new LZJUDP();
					}
					return singleInstance;
				}
			}
		}
		private LZJUDP()
		{
			//this.Handle = Handle;
			server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			//string ip = IPManager.GetIP(ADDRESSFAM.IPv4);
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(IPManager.GetIP(ADDRESSFAM.IPv4)), ServerConfig.UDPservePort);
			server.Bind(endPoint);
			//Console.WriteLine("1");
			//Logging.Debug.Log("???");
			Logging.Debug.Log("启动udp:"+endPoint);

			/*
			 在UDP通讯过程当中，若是客户端中途断开，服务器会收到一个SocketException，错误ID为10054，
			描述是“远程主机强迫关闭了一个现有的链接”，紧接着的事就可怕了，UDP服务终止监听，全部客户端都受到了影响。
			也就是说一个客户端引发的异常致使了整个系统的崩溃。
			 使用 IOControlCode 枚举指定控制代码，为 Socket 设置低级操做模式。
			
			ioControlCode

				一个 IOControlCode 值，它指定要执行的操做的控制代码。.net

			optionInValue

				Byte 类型的数组，包含操做要求的输入数据。code

			optionOutValue

				Byte 类型的数组，包含由操做返回的输出数据。对象

			 */
			server.IOControl(
			(IOControlCode)SIO_UDP_CONNRESET,
			new byte[] { 0, 0, 0, 0 },
			null
			);
			(new Thread(RecvThread)).Start();
		}
		public void Init()
		{
			
		}
		public void Send(MainPack pack,string ip_port)
		{
			string[] ipport = ip_port.Split(":");
			//Console.WriteLine($"send  {ip_port}   {ipport[0]}      {ipport[1]}");
			//new IPEndPoint(ip_port);
			EndPoint point = new IPEndPoint(IPAddress.Parse(ipport[0]), int.Parse(ipport[1]));
			//Logging.Debug.Log("UDP Send   " + port + "         " + pack);
			byte[] sendbuff = pack.ToByteArray();
			server.SendTo(sendbuff, point);
		}
		private void RecvThread()
		{

			//IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(serverIp), UdpManager.Instance.recvPort);
			EndPoint point = new IPEndPoint(IPAddress.Parse("192.168.2.164"), 0);//用来保存发送方的ip和端口号
			while (true)
			{
				try
				{
					//point = new IPEndPoint(IPAddress.Any, 0);
					byte[] bytes = new byte[1024];
					int length = server.ReceiveFrom(bytes, ref point);//接收数据报
					if (length == 0)
					{
						continue;
					}
	
					MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(bytes, 0, length);
					//Logging.Debug.Log(point.ToString() + "  " + " udp接受数据 " + pack + "   ");
					Handle?.Invoke(pack);
					//string message = Encoding.UTF8.GetString(buffer, 0, length);
					//listBox1.Items.Add(point.ToString() + "：" + message);
					//sendClient.Client.ReceiveFrom(bytes,ref endpoint);
					//byte[] bytes = sendClient.Receive(ref endpoint);

					//if (sendEndPort == null)
					//{
					//Logging.Debug.Log("接收客户端udp信息:" + endpoint.Port);
					//sendPortNum = endpoint.Port;
					//}
					/*
					byte packMessageId = buf[PackageConstant.PackMessageIdOffset];     //消息id (1个字节)
					Int16 packlength = BitConverter.ToInt16(buf, PackageConstant.PacklengthOffset);  //消息包长度 (2个字节)
					int bodyDataLenth = packlength - PackageConstant.PacketHeadLength;
					byte[] bodyData = new byte[bodyDataLenth];
					Array.Copy(buf, PackageConstant.PacketHeadLength, bodyData, 0, bodyDataLenth);
	
					delegate_analyze_message((PBCommon.CSID)packMessageId, bodyData);
									*/
					//bytes = _client.Receive(ref endpoint);


					//是客户端,统计接收量
					//				GameData.Instance().recvNum+=buf.Length;
					//				Logging.Debug.Log("发送量:" + buf.Length.ToString() + "," + GameData.Instance().recvNum.ToString());
				}
				catch (Exception ex)
				{

					Logging.Debug.Log(point + ":::udpClient接收数据异常:  " + ex.Message);
				}
				Logging.Debug.Log("udp接收线程退出~~~~~");
			}
		}

	}

}
