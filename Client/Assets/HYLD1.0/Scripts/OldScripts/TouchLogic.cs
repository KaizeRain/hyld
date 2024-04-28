/*
 * * * * * * * * * * * * * * * * 
 * Author:        赵元恺
 * CreatTime:  2020/6/18 20:16:06 
 * Description: UI遥感交互逻辑
 * * * * * * * * * * * * * * * * 
*/

using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using LZJ;
public class TouchLogic : MonoBehaviour 
{
	public Slider 能量条;
	public GameObject 大招遥感;


	private void FixedUpdate()
{
		//HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].可以按大招 = true;
		//Logging.HYLDDebug.LogError(HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].可以按大招);
		//Logging.HYLDDebug.LogError(HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].当前能量);
		if (HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].当前能量 >= HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].最大能量)
		{
			HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].可以按大招 = true;
		}
		能量条.gameObject.SetActive(!HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].可以按大招);
		大招遥感.SetActive(HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].可以按大招);
		if (能量条.gameObject.activeSelf)
		{
			能量条.value = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].当前能量 / HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].最大能量;
		}

	}
	void fireNormalButtonUp()
	{
		return;//TODO:打咩！
		if (Toolbox.是否游戏结束) return;
		HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireState = FireState.PstolNormal;
		float min = 9999999;
		int ans = -1;
		int i = 0;
		foreach (var pos in HYLDStaticValue.Players)
		{
			
			if (pos.teamID != HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].teamID)//搜最近敌人
			{
				float temp=(pos.playerPositon - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon).magnitude;
				
				if (min > temp)
				{
					min = temp;
					ans = i;
				}
			}
			i++;
		}
		LZJ.Fixed tempX = new LZJ.Fixed(HYLDStaticValue.Players[ans].playerPositon.x - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.x);
		LZJ.Fixed tempZ = new LZJ.Fixed(HYLDStaticValue.Players[ans].playerPositon.z - HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon.z);
		
		HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireTowards=LZJ.MathFixed.xAndY2UnitVector3(tempX.ToFloat(),tempZ.ToFloat());
	}
	void OnEnable()
	{
		EasyJoystick.On_JoystickMove += OnJoystickMove;
		EasyJoystick.On_JoystickMoveEnd +=JoystickMoveEnd;
	}


	
	void JoystickMoveEnd(MovingJoystick move)
	{
		if (Toolbox.是否游戏结束) return;
		if (move.joystickName == "PlayerMove")
		{
			HYLDStaticValue.PlayerMoveX  = Fixed.Zero;
			HYLDStaticValue.PlayerMoveY  = Fixed.Zero;
			//CommandManger.Instance.AddCommad_Move(HYLDStaticValue.PlayerMoveX, HYLDStaticValue.PlayerMoveY);
		}
		if (move.joystickName == "FireNormal"||move.joystickName=="FireSuper")
		{
			
			HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].body.GetComponentInChildren<LineRenderer>().enabled = false;

			if (HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireState == FireState.none)
			{
				//遥感移动距离太打咩
				if (MathFixed.Abs(FirePositionX) <= 0.02f || MathFixed.Abs(FirePositionY) <= 0.02f)
				{
					return;
				}
				CommandManger.Instance.AddCommad_Attack(FirePositionX.ToFloat(), FirePositionY.ToFloat());
				return;
				//TODO: 子弹发射逻辑
				if (move.joystickName == "FireNormal")
				{
					HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireState = FireState.PstolNormal;
				}
				
				else
				{
					HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireState = FireState.ShotgunSuper;
				}
				Vector3 temp = LZJ.MathFixed.xAndY2UnitVector3(FirePositionY.ToFloat(), FirePositionX.ToFloat());
				temp.x *= -1;
				
				//temp.z = FirePositionX > 0 ? temp.z : -temp.z;
				
				HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireTowards = temp;
			}
		}
	}

	private Fixed FirePositionY=Fixed.Zero;
	private Fixed FirePositionX=Fixed.Zero;
	private LineRenderer selfFireLineRenderer;

	private float shootDistance;
	private float launchAngle;
	private void Start()
	{
		//Logging.HYLDDebug.LogError(HYLDStaticValue.ModenName);
		//Logging.HYLDDebug.LogError(HYLDStaticValue.playerSelfIDInServer);
		if(HYLDStaticValue.ModenName== "HYLDTryGame"||!HYLDStaticValue.isNet)
		HYLDStaticValue.playerSelfIDInServer=0;
		//Logging.HYLDDebug.LogError(HYLDStaticValue.Players.Count);
		selfFireLineRenderer = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].body.GetComponentInChildren<LineRenderer>();
		
		selfFireLineRenderer.enabled = false;

		for(int i=0;i<HYLDStaticValue.Players.Count;i++)
		{
			//Logging.HYLDDebug.LogError(HYLDStaticValue.Players.Count);
			HYLDStaticValue.Players[i].isNotDie = true;
		}

		
	}

	private void OnDestroy()
	{
		for(int i=0;i<HYLDStaticValue.Players.Count;i++)
		{
			HYLDStaticValue.Players[i].isNotDie = false;
		}
	}


	void OnJoystickMove(MovingJoystick move)
	{
		//Logging.HYLDDebug.LogError(Toolbox.是否游戏结束);
		if (Toolbox.是否游戏结束) return;
		if (move.joystickName == "FireNormal"|| move.joystickName == "FireSuper")
		{
			FirePositionY = new Fixed( move.joystickAxis.y);
			
			FirePositionX = new Fixed(move.joystickAxis.x);
			Fixed R = FirePositionX * FirePositionX + FirePositionY * FirePositionY;
			if(selfFireLineRenderer==null)
			{
				HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].body.transform.Find("Capsule").Find("Gun").gameObject.AddComponent<LineRenderer>();
				selfFireLineRenderer= HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].body.GetComponentInChildren<LineRenderer>();
			}
			selfFireLineRenderer.enabled = true;
			Vector3 temp =
				LZJ.MathFixed.Vector32UnitVector3((HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon),
					(HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon+new Vector3(FirePositionX.ToFloat(),1,FirePositionY.ToFloat())));
			temp.y = temp.z;
			temp.z = temp.x;
			temp.x = -temp.y;
			temp.y = 0;
			shootDistance = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].hero.shootDistance;
			
			//Logging.HYLDDebug.Log(shootDistance);
			
			launchAngle=HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].hero.LaunchAngle;
			float lineWidth=HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].hero.shootWidth;

			if (launchAngle == 0)
			{
				selfFireLineRenderer.startWidth = lineWidth;
				selfFireLineRenderer.endWidth = lineWidth;
				selfFireLineRenderer.startColor = new Color(1,1,1,0.5f);
				
				selfFireLineRenderer.endColor = new Color(1,1,1,0.5f);
				selfFireLineRenderer.SetPosition(0,HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon);
				selfFireLineRenderer.SetPosition(1,HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon+shootDistance*temp);

			}
			else
			{
				Vector3 center = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon;
				int pointAmmount = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].hero.bulletCount;
				float eachAngle = launchAngle / pointAmmount;
				Vector3 forward = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].body.transform.forward;
				if (lineWidth == 0)
				{
					lineWidth = 0.1f;
				}
				
				selfFireLineRenderer.positionCount = (pointAmmount*2 + 2);
				selfFireLineRenderer.SetPosition(0, center);
				int i=1,cnt=1;
				for (; i <= pointAmmount; i++)
				{
					Vector3 pos = Quaternion.Euler(0, -launchAngle / 2 + eachAngle * (i - 1), 0) * temp * shootDistance+center;
					selfFireLineRenderer.SetPosition(cnt++,pos);
					selfFireLineRenderer.SetPosition(cnt++,center);

				}

				selfFireLineRenderer.SetPosition(cnt, center);
			}
			
			
			
		}
		
		if (move.joystickName == "PlayerMove")
		{
			Fixed PositionY = new Fixed(move.joystickAxis.x);
			
			Fixed PositionX = new Fixed(move.joystickAxis.y);

			Fixed speed = new Fixed(HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].移动速度)*0.0625f;

			if (PositionX > 0.5f)
			{
				HYLDStaticValue.PlayerMoveX = speed;
			}
			else if (PositionX < -0.5f)
			{
				HYLDStaticValue.PlayerMoveX = -speed;
			}
			else
			{
				HYLDStaticValue.PlayerMoveX = PositionX;
			}
		
			if (PositionY > 0.5f)
			{
				HYLDStaticValue.PlayerMoveY =speed;
			}
			else if (PositionY < -0.5f)
			{
				HYLDStaticValue.PlayerMoveY  = -speed;
			}
			else
			{
				HYLDStaticValue.PlayerMoveY  =PositionY;
			}

			CommandManger.Instance.AddCommad_Move(HYLDStaticValue.PlayerMoveX, HYLDStaticValue.PlayerMoveY);
		}
		
	}

}

