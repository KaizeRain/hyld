using Google.Protobuf;
using Logging;
using SocketProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main()
        {
			//这个是追踪打印路径  目前服务器其实不需要追踪
			string _traceLogPath = $"D:/XuanShuiLiuLi/HYLDNew/hyld/Server/log/1.txt";
			Debug.TraceSavePath = _traceLogPath;

			ServerConfig.MaxRoom3_3Number = 2;//一场战斗多少人才开始游戏
			ServerConfig.MaxTeam3_3Number = 2;//一个战斗多少个队伍
			ServerConfig.MaxPlayerCount = 10;//最大玩家数量
			//数据库设置
			//ServerConfig.DOMConectStr = "database=hyld;data source=localhost;user=root;password=123456;pooling=false;charset=utf8mb4;port=3306;SSL Mode=none";

			//Debug.Log(IPManager.GetIP(ADDRESSFAM.IPv4));
			//Debug.Trace("?????");
			//Debug.FlushTrace();
			new Server(ServerConfig.TCPservePort);
			Console.Read();
			Logging.Debug.FlushTrace();
		}
    }
}

public class IPManager
{
	public static string GetIP(ADDRESSFAM Addfam)
	{
		//Return null if ADDRESSFAM is Ipv6 but Os does not support it
		if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
		{
			return null;
		}

		string output = "";

		foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
		{
		
//#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
			NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

			if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
//#endif
			{
				foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
				{
					//IPv4
					if (Addfam == ADDRESSFAM.IPv4)
					{
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							output = ip.Address.ToString();
							//Debug.Log("啊" + output);
						}
					}

					//IPv6
					else if (Addfam == ADDRESSFAM.IPv6)
					{
						if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
						{
							output = ip.Address.ToString();
						}
					}
				}
			}
		}
		return output;
	}
}

public enum ADDRESSFAM
{
	IPv4, IPv6
}
