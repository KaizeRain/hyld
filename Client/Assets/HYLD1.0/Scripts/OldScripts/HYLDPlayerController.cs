/*
 * * * * * * * * * * * * * * * * 
 * Author:        魏佳楠
 * CreatTime:  2020/6/18 20:47:22 
 * Description: 
 * * * * * * * * * * * * * * * * 
*/
/*
 * * * * * * * * * * * * * * * * 
 * Author:        赵元恺
 * CreatTime:  2020/7/3 20:47:22 
 * Description: 备注掉sendPosition防止服务器的bug，
 * * * * * * * * * * * * * * * * 
*/
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class HYLDPlayerController : MonoBehaviour
{
	public int AILazyDegree = 100;
	public bool isAI = false;
	public bool isSelf = false;
	public int playerID = 0;
	public Transform selfTransform;
	private Animator Ani;
	public float rotatespped = 100;
	public float movespeed = 5;
	void Start()
	{
		//speed = HYLDStaticValue.MovingSpeed;
		//selfTransform = gameObject.transform;
		//if (playerID != 1) isAI = true;
		//HYLDStaticValue.Players[playerID].isNotDie=false;
		if (HYLDStaticValue.Players[playerID].playerName == "A")
			isAI = true;
		Ani = GetComponent<PlayerLogic>().bodyAnimator;
		//if (playerID == HYLDStaticValue.playerSelfIDInServer && !TCPSocket.是否单机)
		//	InvokeRepeating("sendPosition", 0, 0.1f);
	}

	private string tempOld = "", temp = "";
	void sendPosition()
	{

		
		if (HYLDStaticValue.玩家输了吗) return;

		if (HYLDStaticValue.isloading) return;
		if (HYLDStaticValue.Players.Count == 0) return;
		string temp = HYLDStaticValue.playerSelfIDInServer.ToString() + "#" +
					  string.Format("{0:f2}",
						  HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.x) + "#"
					  + string.Format("{0:f2}",
						  HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.z);


		if (tempOld != temp)
		{
			tempOld = temp;
			//TCPSocket.Instance.Send(OldRequestCode.HYLDGame, OldActionCode.PlayerLogicMove, temp);
		}


	}
	private void Update()
	{

	}

	private int cnt = 0;
	private int needUpdatevalue = 0;
	float movex = 1;
	float movez = 1;
	void FixedUpdate()
	{
		//Logging.HYLDDebug.LogError(HYLDStaticValue.Players[playerID].isNotDie);
		//Logging.HYLDDebug.LogError(isAI);
		if (isAI || !HYLDStaticValue.Players[playerID].isNotDie)
		{
			return;//用行为树控制AI
		}
		if (Toolbox.是否游戏结束) return;
		//print(HYLDStaticValue.Players[playerID].playerPositon);
		//print(selfTransform.position);
		/*
		float x = Input.GetAxis("Horizontal");
		float y = Input.GetAxis("Vertical");

		Vector3 pos = new Vector3(-y, 0, x);
		if (isSelf && HYLDStaticValue.Players[playerID].isNotDie && pos.magnitude != 0)
		{
			{
				Ani.SetFloat("Speed", Mathf.Abs(x) + Mathf.Abs(y));
				selfTransform.LookAt(selfTransform.position + pos);
				selfTransform.position += pos * Time.fixedDeltaTime * (HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].移动速度 - 4) * 0.5f;
				HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon = selfTransform.position;
			}
		}
		*/


		//else
		{
			//Vector3 temp = new Vector3(-HYLDStaticValue.Players[playerID].playerMoveX, 0f, HYLDStaticValue.Players[playerID].playerMoveY);
			//Ani.SetFloat("Speed", temp.magnitude);
			//selfTransform.Translate((HYLDStaticValue.Players[playerID].移动速度 - 4) * Time.deltaTime * (temp), Space.World);

			//HYLDStaticValue.Players[playerID].playerPositon = selfTransform.position;

			//LZJ.Fixed3 temp = new LZJ.Fixed3(-HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerMoveX, 0f, HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerMoveY);
			Ani.SetFloat("Speed", HYLDStaticValue.Players[playerID].playerMoveMagnitude);
			//selfTransform.LookAt(Vector3.Lerp(selfTransform.position + selfTransform.forward, selfTransform.position + HYLDStaticValue.Players[playerID].playerMoveDir, Time.fixedDeltaTime * rotatespped)); ;


			//Vector3 t = new Vector3(-HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerMoveX, 0f, HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerMoveY);
			//Logging.HYLDDebug.Log($"Fixed:{temp.magnitude.ToFloat()}   float:{ t.magnitude}");
			//selfTransform.Translate((HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].移动速度 - 4) * Time.fixedDeltaTime * (temp), Space.World);

			//= ;
			//渲染帧的位置插值到逻辑帧位置
			selfTransform.LookAt(Vector3.Lerp(selfTransform.position+selfTransform.forward,HYLDStaticValue.Players[playerID].playerPositon,Time.fixedDeltaTime*rotatespped));
			selfTransform.position = Vector3.Lerp(selfTransform.position,HYLDStaticValue.Players[playerID].playerPositon,Time.fixedDeltaTime*movespeed);

		}

		/*
		else if (playerID == HYLDStaticValue.playerSelfIDInServer && HYLDStaticValue.Players[playerID].isNotDie)
		{

			LZJ.Fixed3 temp = new LZJ.Fixed3(-HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerMoveX, 0f, HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerMoveY);
			Ani.SetFloat("Speed", temp.magnitude.ToFloat());
			//Vector3 t = new Vector3(-HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerMoveX, 0f, HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerMoveY);
            //Logging.HYLDDebug.Log($"Fixed:{temp.magnitude.ToFloat()}   float:{ t.magnitude}");
            //selfTransform.Translate((HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].移动速度 - 4) * Time.fixedDeltaTime * (temp), Space.World);
            //selfTransform.LookAt(selfTransform.position + temp);
             //= ;
			selfTransform.position = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon;
		}

		else if (playerID != HYLDStaticValue.playerSelfIDInServer && isAI == false)//其他玩家 根据全局值  调整自己位置
		{
			Vector3 temp = new Vector3(-HYLDStaticValue.Players[playerID].playerMoveX, 0f, HYLDStaticValue.Players[playerID].playerMoveY);
			Ani.SetFloat("Speed", temp.magnitude);
			selfTransform.Translate((HYLDStaticValue.Players[playerID].移动速度 - 4) * Time.deltaTime * (temp), Space.World);
			selfTransform.LookAt(selfTransform.position + temp);
			HYLDStaticValue.Players[playerID].playerPositon = selfTransform.position;

			return;
			selfTransform.LookAt(HYLDStaticValue.Players[playerID].playerPositon);
			//Logging.HYLDDebug.LogError(Vector3.Distance(selfTransform.position, HYLDStaticValue.Players[playerID].playerPositon));
			Ani.SetFloat("Speed", Vector3.Distance(selfTransform.position, HYLDStaticValue.Players[playerID].playerPositon));
			selfTransform.position = Vector3.Lerp(selfTransform.position, HYLDStaticValue.Players[playerID].playerPositon, 0.05f);
		}
		*/
	}

	Vector3 xAndY2UnitVector3(float x, float z)
	{
		float sin1 = x / (float.Parse(Math.Sqrt(x * x + z * z).ToString()));
		float cos1 = (float.Parse(Math.Sqrt(1 - sin1 * sin1).ToString()));
		return new Vector3(sin1, 0, z > 0 ? cos1 : -cos1);
	}


	void fireNormalButtonUp()
	{
		HYLDStaticValue.Players[playerID].fireState = FireState.PstolNormal;

		float min = 9999999;
		int ans = -1;
		int i = 0;
		foreach (var pos in HYLDStaticValue.Players)
		{

			if (pos.teamID != HYLDStaticValue.Players[playerID].teamID)//搜最近敌人
			{
				float temp = (pos.playerPositon - HYLDStaticValue.Players[playerID].playerPositon).magnitude;

				if (min > temp)
				{
					min = temp;
					ans = i;
				}
			}
			i++;
		}
		float tempX = HYLDStaticValue.Players[ans].playerPositon.x - HYLDStaticValue.Players[playerID].playerPositon.x;
		float tempZ = HYLDStaticValue.Players[ans].playerPositon.z - HYLDStaticValue.Players[playerID].playerPositon.z;

		HYLDStaticValue.Players[playerID].fireTowards = xAndY2UnitVector3(tempX, tempZ);
	}
}

