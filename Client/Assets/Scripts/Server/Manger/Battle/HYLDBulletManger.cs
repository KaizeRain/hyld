/****************************************************
    Author:            龙之介
    CreatTime:    2022/5/7 15:34:53
    Description:     Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;



namespace Manger
{
	public class HYLDBulletManger :MonoBehaviour
	{
        public bool initFinish;
        private Transform bulletParent;
        private GameObject bullet;
		private float[] Timer;
		private List<shell> AllShells = new List<shell>();
		public void InitData()
        {
            initFinish = false;
            GameObject parent = new GameObject("BulletPool");
            parent.transform.position = Vector3.zero;
            bulletParent = parent.transform;
            bullet = HYLDResourceManger.Load(HYLDResourceManger.Type.Bullet);
            initFinish = true;
			Timer = new float[HYLDStaticValue.Players.Count];
			for (int i = 0; i < Timer.Length; i++)
			{
				Timer[i] = 0;
			}
		}
		
        public void OnLogicUpdate()
        {
			//先驱动发射子弹
			for (int playerID = 0; playerID < HYLDStaticValue.Players.Count; playerID++)
			{
				Timer[playerID] += Server.NetConfigValue.frameTime;
				if (Timer[playerID] > Server.NetConfigValue.canPlayerRestoreHealthTime) HYLDStaticValue.Players[playerID].isCanCure = true;//2s No attach can cure；
				if (Timer[playerID] > HYLDStaticValue.Players[playerID].hero.每次发射可以发射间隔手感问题 && HYLDStaticValue.Players[playerID].fireState != FireState.none && HYLDStaticValue.Players[playerID].isNotDie)
				{
					if ((HYLDStaticValue.Players[playerID].fireState == FireState.PstolNormal && HYLDStaticValue.Players[playerID].playerManaValue > 30) || HYLDStaticValue.Players[playerID].fireState == FireState.ShotgunSuper)
					{
						int id = playerID;
						NetGlobal.Instance.AddAction(() => { Attack(id, HYLDStaticValue.Players[id].fireState); });
					}

				}
				else if (HYLDStaticValue.Players[playerID].fireState != FireState.none &&
				 HYLDStaticValue.Players[playerID].playerManaValue < 30)//缺少发射能量
				{
					Logging.HYLDDebug.Log("缺少能量");
					HYLDStaticValue.Players[playerID].fireState = FireState.none;
					HYLDStaticValue.Players[playerID].fireTowards = Vector3.zero;
				}

			}

			//遍历所有子弹
			for (int i = 0; i < AllShells.Count; i++)
			{
				//int id = i;

				//NetGlobal.Instance.AddAction(() => { 
					AllShells[i].OnUpdateLogic();
				//});

			}
		}
		private void changeGun(GameObject bullet, Hero hero,int playerID)
		{
			bullet.GetComponent<BulletLogic>().bulletOnwerID = playerID;
			bullet.GetComponent<BulletLogic>().setBulletInformation(hero);

		}
		private void RemoveShell(shell shell)
		{
			AllShells.Remove(shell);
		}
		private void AddShell(shell shell, float speed, float health)
		{
			shell.InitData(speed, health, RemoveShell);

			AllShells.Add(shell);
		}
		private void Attack(int playerID, FireState fireState)
		{
			HYLDStaticValue.Players[playerID].isCanCure = false;
			Timer[playerID] = 0;
			HYLDStaticValue.Players[playerID].bodyAnimator.SetTrigger("Fire");
			Vector3 fireTowardsTemp = HYLDStaticValue.Players[playerID].fireTowards;
			fireTowardsTemp.y = 0;
			GameObject temp = Instantiate(bullet, bulletParent);
			BulletLogic bulletLogic = temp.GetComponent<BulletLogic>();
			if (fireState == FireState.PstolNormal)
			{
				if (HYLDStaticValue.Players[playerID].hero.Name == "贝亚")
				{
					if (HYLDStaticValue.Players[playerID].playerManaValue < 90)
						return;
					else
						HYLDStaticValue.Players[playerID].playerManaValue -= 60;
				}//贝亚最大装弹量为1

				HYLDStaticValue.Players[playerID].playerManaValue -= 30;

				if (HYLDStaticValue.Players[playerID].hero.Name == "麦克斯")                
				{
					HYLDStaticValue.Players[playerID].playerManaValue += 3;

				}   //麦克斯最大装弹量为4


				//Transform transform = HYLDStaticValue.Players[playerID].BulletTrans;

			
				
				changeGun(temp, HYLDStaticValue.Players[playerID].hero,playerID);





				bulletLogic.Towards = fireTowardsTemp;

				//Bullet bulletClone = new Bullet(temp, 20, HYLDStaticValue.Players[playerID].hero, 20f);
				//bullets.Add(bulletClone);
			}
			if (fireState == FireState.ShotgunSuper)
			{
				HYLDStaticValue.Players[playerID].可以按大招 = false;
				HYLDStaticValue.Players[playerID].当前能量 = 0;
				if (HYLDStaticValue.Players[playerID].hero.Name == "麦克斯")
				{
					GameObject go = Instantiate(HYLDStaticValue.Players[playerID].hero.大招实体, transform.parent);
					go.transform.position = HYLDStaticValue.Players[playerID].playerPositon;
					go.GetComponent<移动型大招>().playerid = playerID;
					go.GetComponent<移动型大招>().当前英雄 = HeroName.MaiKeSi;
					Destroy(go, 2);

				}
				else
				{
				


					changeGun(temp, HYLDStaticValue.Players[playerID].hero,playerID);
					//bulletLogic = temp.GetComponent<BulletLogic>();
					bulletLogic.Towards = fireTowardsTemp;
					bulletLogic.bulletPrefab = HYLDStaticValue.Players[playerID].hero.大招实体;
					if (HYLDStaticValue.Players[playerID].hero.Name == "瑞科" || HYLDStaticValue.Players[playerID].hero.Name == "柯尔特")
					{
						bulletLogic.shootWidth = 0.2f;
						bulletLogic.shootDistance += 4;
						bulletLogic.speed += 6;
						bulletLogic.bulletCount = 12;
					}
					if (HYLDStaticValue.Players[playerID].hero.Name == "雪莉")
					{
						bulletLogic.LaunchAngle += 10;
						bulletLogic.bulletCount *= 2;
						bulletLogic.speed += 3;
					}
					if (HYLDStaticValue.Players[playerID].hero.Name == "格尔")
					{
						bulletLogic.shootWidth += 1;
						bulletLogic.bulletCountByEachTime = 4;
						bulletLogic.bulletCount = 4;
						bulletLogic.speed += 3;
						bulletLogic.EachTimebulletsShootSpace = 0;
					}
					if (HYLDStaticValue.Players[playerID].hero.Name == "贝亚")
					{
						bulletLogic.shootWidth += 0.3f;
						bulletLogic.bulletCountByEachTime = 6;
						bulletLogic.bulletCount = 6;
						bulletLogic.bulletDamage = 60;
					}
					if (HYLDStaticValue.Players[playerID].hero.Name == "帕姆")
					{
						bulletLogic.bulletDamage = 300;
						bulletLogic.IsParadola = true;
						bulletLogic.bulletCount = 1;
						bulletLogic.bulletCountByEachTime = 1;
						bulletLogic.LaunchAngle = 0;
						bulletLogic.shootDistance = 2;
						bulletLogic.speed = 5f;
						bulletLogic.shootWidth = 1;
					}
					//Bullet bulletClone = new Bullet(temp, 20, HYLDStaticValue.Players[playerID].hero, 20f);
				}
				
			}
			
			bulletLogic.InitData(AddShell);
			HYLDStaticValue.Players[playerID].fireState = FireState.none;
			HYLDStaticValue.Players[playerID].fireTowards = Vector3.zero;
		}

    }
}