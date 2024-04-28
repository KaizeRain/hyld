/*
 * * * * * * * * * * * * * * * * 
 * Author:        魏佳楠
 * CreatTime:  2020/6/18 21:25:33 
 * Description: 
 * * * * * * * * * * * * * * * * 
*/
/*
****************
 * Author:        赵元恺
 * CreatTime:  2020/7/4 22:41 
 * Description: 越过服务器运行单人模式,引入Hero类，hero自带子弹类型，血量，名字等参数
 **************** 
*/
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class HYLDCameraManger : MonoBehaviour
{
	private float tempx = 0;
	private float tempy = 0;
	[HideInInspector] 
	public string moden = "HYLDBaoShiZhengBa";
	//public bool isTest = false;
	public bool initFinish { get; private set; }
	public void InitData()
	{
		initFinish = false;
		moden =HYLDStaticValue.ModenName;
		//Logging.HYLDDebug.LogError(moden);
		//Logging.HYLDDebug.LogError(HYLDStaticValue.playerSelfIDInServer);
		//if(isTest)
		//HYLDStaticValue.playerSelfIDInServer = 0;


		/*
		if (moden=="HYLDTryGame")
		{
			//Logging.HYLDDebug.LogError(1);
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, 0), "玩家1", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team1));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(13, 1, -5), "A", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team2));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(11, 1, -5), "A", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team2));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 1, 5), "A", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team2));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(13, 1, 5), "A", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team2));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(11, 1, 5), "A", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team2));
			for(int i=1;i<HYLDStaticValue.Players.Count;i++)
			{
				HYLDStaticValue.Players[i].isNotDie = false;
				HYLDStaticValue.Players[i].playerType = PlayerType.Enemy;
			}
			HYLDStaticValue.playerSelfIDInServer = 0;
			HYLDStaticValue.SelfTeam = PlayerTeam.team1;
			HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerType = PlayerType.Self;
			/*	
			//zyk
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, 0), "玩家3", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team1));
			Instantiate(textEnemy, new Vector3(15, 0, -5),Quaternion.identity);
			Instantiate(textEnemy, new Vector3(13, 0, -5), Quaternion.identity);
			Instantiate(textEnemy, new Vector3(11, 0, -5), Quaternion.identity);
			Instantiate(textEnemy, new Vector3(15, 0, 5), Quaternion.identity);
			Instantiate(textEnemy, new Vector3(13, 0, 5), Quaternion.identity);
			Instantiate(textEnemy, new Vector3(11, 0, 5), Quaternion.identity);
			HYLDStaticValue.playerSelfIDInServer = 0;
			HYLDStaticValue.SelfTeam = PlayerTeam.team1;
			HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerType = PlayerType.Self;
			//zyk

			tempx = transform.position.x - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.x;
			tempy = transform.position.y - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.y;
			*/


		//}
		// 
		/*
		else if (moden == "HYLDHuangYeJueDou")
		{
			//HYLDStaticValue._myheroName = HeroName.BoKe ;
			//zyk
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, -5), "玩家1", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team1));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, -6), "玩家2", HYLDStaticValue.Heros[HeroName.BeiYa], PlayerTeam.team2));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, 0), "玩家3", HYLDStaticValue.Heros[HeroName.SiPaiKe], PlayerTeam.team3));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, 0), "玩家4", HYLDStaticValue.Heros[HeroName.LiAng], PlayerTeam.team4));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, 5), "玩家5", HYLDStaticValue.Heros[HeroName.PanNi], PlayerTeam.team5));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, 6), "玩家6", HYLDStaticValue.Heros[HeroName.BuLuoKe], PlayerTeam.team6));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(8, 0, -6), "玩家7", HYLDStaticValue.Heros[HeroName.PaMu], PlayerTeam.team7));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(8, 0, 6), "玩家8", HYLDStaticValue.Heros[HeroName.ABo], PlayerTeam.team8));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-8, 0, -6), "玩家9", HYLDStaticValue.Heros[HeroName.DaLiEr], PlayerTeam.team9));
			HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-8, 0, 6), "玩家10", HYLDStaticValue.Heros[HeroName.PeiPei], PlayerTeam.team10));
			
			
			
			HYLDStaticValue.SelfTeam = PlayerTeam.team1;
			
			
			
			for (int i = 0; i < 10; i++)
			{
				//Logging.HYLDDebug.LogError("team:" + HYLDStaticValue.Players[i].playerTeam);
				if (i == HYLDStaticValue.playerSelfIDInServer)
				{
					HYLDStaticValue.Players[i].playerType = PlayerType.Self;
				}
			
				else if (i != HYLDStaticValue.playerSelfIDInServer &&
						 HYLDStaticValue.SelfTeam != HYLDStaticValue.Players[i].playerTeam)
				{
					HYLDStaticValue.Players[i].playerType = PlayerType.Enemy;
				}
			}



			for (int i = 1; i < 10; i++)
			{
				HYLDStaticValue.Players[i].playerName = "A";
			}
			//zyk

			tempx = transform.position.x - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.x;
			tempy = transform.position.y - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.y;
		}
		else
		{
			//HYLDStaticValue._myheroName = HeroName.BoKe ;
			//zyk
			if (isTest)
			{
				TCPSocket.玩家名.Add(HYLDStaticValue.PlayerName);
				for(int i=0;i<5;i++) TCPSocket.玩家名.Add("A");
				for (int i = 0; i < 6; i++) TCPSocket.被选择的英雄.Add(HeroName.XueLi.ToString());
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, -5), TCPSocket.玩家名[0], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[0])], PlayerTeam.team1));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, -6), TCPSocket.玩家名[1], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[1])], PlayerTeam.team2));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, 0), TCPSocket.玩家名[2], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[2])], PlayerTeam.team1));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, 0), TCPSocket.玩家名[3], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[3])], PlayerTeam.team2));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, 5), TCPSocket.玩家名[4], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[4])], PlayerTeam.team1));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, 6), TCPSocket.玩家名[5], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[5])], PlayerTeam.team2));
			}
			else
			{
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, -5), TCPSocket.玩家名[0], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[0])], PlayerTeam.team1));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, -6), TCPSocket.玩家名[1], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[1])], PlayerTeam.team2));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, 0), TCPSocket.玩家名[2], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[2])], PlayerTeam.team1));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, 0), TCPSocket.玩家名[3], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[3])], PlayerTeam.team2));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(15, 0, 5), TCPSocket.玩家名[4], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[4])], PlayerTeam.team1));
				HYLDStaticValue.Players.Add(new PlayerInformation(new Vector3(-15, 0, 6), TCPSocket.玩家名[5], HYLDStaticValue.Heros[string2HeroName(TCPSocket.被选择的英雄[5])], PlayerTeam.team2));
			}

			
			//HYLDStaticValue.Players.Insert(HYLDStaticValue.playerSelfIDInServer, new PlayerInformation(new Vector3(15, 0, -5), "玩家1", HYLDStaticValue.Heros[HYLDStaticValue._myheroName], PlayerTeam.team1));
			
			HYLDStaticValue.SelfTeam = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerTeam;
			//if (HYLDStaticValue.playerSelfIDInServer == 0 || HYLDStaticValue.playerSelfIDInServer == 2 || HYLDStaticValue.playerSelfIDInServer == 4) HYLDStaticValue.SelfTeam = PlayerTeam.team1;
			//else HYLDStaticValue.SelfTeam = PlayerTeam.team2;

			for (int i = 0; i < 6; i++)
			{
				//Logging.HYLDDebug.LogError("team:" + HYLDStaticValue.Players[i].playerTeam);
				if (i == HYLDStaticValue.playerSelfIDInServer)
				{
					HYLDStaticValue.Players[i].playerType = PlayerType.Self;

				}
				else if (i != HYLDStaticValue.playerSelfIDInServer &&
					HYLDStaticValue.SelfTeam == HYLDStaticValue.Players[i].playerTeam)
				{
					HYLDStaticValue.Players[i].playerType = PlayerType.Teammate;
				}
				else if (i != HYLDStaticValue.playerSelfIDInServer &&
						 HYLDStaticValue.SelfTeam != HYLDStaticValue.Players[i].playerTeam)
				{
					HYLDStaticValue.Players[i].playerType = PlayerType.Enemy;
				}
			}


			/*
			for (int i = 1; i < 6; i++)
			{
				HYLDStaticValue.Players[i].playerName = "A";
			}
			*/
		//zyk


		//}

		//Logging.HYLDDebug.LogError(HYLDStaticValue.playerSelfIDInServer);
		//Logging.HYLDDebug.LogError(HYLDStaticValue.Players.Count);
		StartCoroutine(InitCamera());
	}
	IEnumerator InitCamera()
	{
		yield return new WaitUntil(() => {
			// Logging.HYLDDebug.LogError("WaitInitData()~~~等待中");
			return HYLDStaticValue.playerSelfIDInServer!=-1;//roleManage.initFinish && obstacleManage.initFinish && bulletManage.initFinish;
		});
		tempx = Mathf.Min(6, transform.position.x - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.x);
		tempy = Mathf.Min(12, transform.position.y - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.y);
		initFinish = true;
	}
	HeroName string2HeroName(string 英雄名字)
	{
		foreach (HeroName hero in Enum.GetValues(typeof(HeroName)))
		{
			if (英雄名字 == hero.ToString())
			{
				return hero;
			}
		}
		return HeroName.XueLi;
	}

	Vector3 startPos;
	Vector3 endPos;

	public void OnLogicUpdate()
	{
		//transform.rotation = new Quaternion(0, 0, 0, 0);
		if (!initFinish) return;
		if (HYLDStaticValue.isloading) return;
		startPos = transform.position;
		endPos = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon;
		endPos.x += tempx;
		//endPos.y += tempy;
		endPos.y += tempy;
		endPos.z = transform.position.z;
		transform.position = Vector3.Lerp(startPos,endPos,0.05f);
		
	}

}

