/*
 * * * * * * * * * * * * * * * * 
 * Author:        魏佳楠
 * CreatTime:  2020/6/19 17:17:15 
 * Description: 子弹创建
 * * * * * * * * * * * * * * * * 
*/
/*


using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Bullet
{
	public GameObject body;
	public int hrut;
	public Hero hero;
	public float speed;
	public Vector3 Towards;
	public Bullet(GameObject body, int hrut, Hero hero, float speed)
	{
		this.body = body;
		this.hrut = hrut;
		this.hero = hero;
		this.speed = speed;
	}
}
public class BulletCreater : MonoBehaviour
{
	public GameObject bullet;
	public List<Bullet> bullets=new List<Bullet>();
	private Animator bodyAnimator;
	private int playerID=-1;
	private float Timer = 0;
	void Start ()
	{
		bodyAnimator = transform.parent.parent.GetComponent<PlayerLogic>().bodyAnimator;
		
		playerID = gameObject.transform.parent.parent.GetComponent<PlayerLogic>().playerID;
	}

	void changeGun(GameObject bullet,Hero hero)
	{
		bullet.GetComponent<BulletLogic>().bulletOnwerID = playerID;
		bullet.GetComponent<BulletLogic>().setBulletInformation(hero);
		
	}


	private void FixedUpdate()
{
		if (HYLDStaticValue.isloading) return;
		Timer += Time.fixedDeltaTime;
		if(Timer>2f) HYLDStaticValue.Players[playerID].isCanCure = true;//2s No attach can cure；
		//Logging.HYLDDebug.LogError(HYLDStaticValue.Players[playerID].fireState);
		if (Timer> HYLDStaticValue.Players[playerID].hero.每次发射可以发射间隔手感问题 && HYLDStaticValue.Players[playerID].fireState!=FireState.none &&HYLDStaticValue.Players[playerID].isNotDie)
		{
			if ((HYLDStaticValue.Players[playerID].fireState == FireState.PstolNormal && HYLDStaticValue.Players[playerID].playerManaValue > 30)|| HYLDStaticValue.Players[playerID].fireState == FireState.ShotgunSuper)
			{
				HYLDStaticValue.Players[playerID].isCanCure = false;
				Timer = 0;
				bodyAnimator.SetTrigger("Fire");
				Vector3 fireTowardsTemp = HYLDStaticValue.Players[playerID].fireTowards;
				fireTowardsTemp.y = 0;

				if (HYLDStaticValue.Players[playerID].fireState == FireState.PstolNormal)
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
						//麦克斯最大装弹量为4
					}




					GameObject temp = Instantiate(bullet, transform.position, Quaternion.identity);


					changeGun(temp, HYLDStaticValue.Players[playerID].hero);





					temp.GetComponent<BulletLogic>().Towards = fireTowardsTemp;
					foreach (var VARIABLE in transform.parent.GetComponentsInChildren<Collider>())
					{
						UnityEngine.Physics.IgnoreCollision(VARIABLE, temp.GetComponent<BulletLogic>().bulletBody.GetComponent<Collider>());

					}
					Bullet bulletClone = new Bullet(temp, 20, HYLDStaticValue.Players[playerID].hero, 20f);
					bullets.Add(bulletClone);
				}
				if (HYLDStaticValue.Players[playerID].fireState == FireState.ShotgunSuper)
				{
					HYLDStaticValue.Players[playerID].可以按大招 = false;
					HYLDStaticValue.Players[playerID].当前能量 = 0;
					if(HYLDStaticValue.Players[playerID].hero.Name == "麦克斯")
					{
						GameObject go = Instantiate(HYLDStaticValue.Players[playerID].hero.大招实体, transform.parent);
						go.transform.position = HYLDStaticValue.Players[playerID].playerPositon;
						go.GetComponent<移动型大招>().playerid = playerID;
						go.GetComponent<移动型大招>().当前英雄 = HeroName.MaiKeSi;
						Destroy(go,2);

					}
					else
					{
						GameObject temp = Instantiate(bullet, transform.position, Quaternion.identity);


						changeGun(temp, HYLDStaticValue.Players[playerID].hero);

						temp.GetComponent<BulletLogic>().Towards = fireTowardsTemp;
						temp.GetComponent<BulletLogic>().bulletPrefab = HYLDStaticValue.Players[playerID].hero.大招实体;
						if (HYLDStaticValue.Players[playerID].hero.Name == "瑞科" || HYLDStaticValue.Players[playerID].hero.Name == "柯尔特")
						{
							temp.GetComponent<BulletLogic>().shootWidth = 0.2f;
							temp.GetComponent<BulletLogic>().shootDistance += 4;
							temp.GetComponent<BulletLogic>().speed += 6;
							temp.GetComponent<BulletLogic>().bulletCount = 12;
						}
						if (HYLDStaticValue.Players[playerID].hero.Name == "雪莉")
						{
							temp.GetComponent<BulletLogic>().LaunchAngle += 10;
							temp.GetComponent<BulletLogic>().bulletCount *= 2;
							temp.GetComponent<BulletLogic>().speed += 3;
						}
						if (HYLDStaticValue.Players[playerID].hero.Name == "格尔")
						{
							temp.GetComponent<BulletLogic>().shootWidth += 1;
							temp.GetComponent<BulletLogic>().bulletCountByEachTime = 4;
							temp.GetComponent<BulletLogic>().bulletCount = 4;
							temp.GetComponent<BulletLogic>().speed += 3;
							temp.GetComponent<BulletLogic>().EachTimebulletsShootSpace = 0;
						}
						if (HYLDStaticValue.Players[playerID].hero.Name == "贝亚")
						{
							temp.GetComponent<BulletLogic>().shootWidth += 0.3f;
							temp.GetComponent<BulletLogic>().bulletCountByEachTime = 6;
							temp.GetComponent<BulletLogic>().bulletCount = 6;
							temp.GetComponent<BulletLogic>().bulletDamage = 60;
						}
						if(HYLDStaticValue.Players[playerID].hero.Name== "帕姆")
						{
							temp.GetComponent<BulletLogic>().bulletDamage = 300;
							temp.GetComponent<BulletLogic>().IsParadola = true;
							temp.GetComponent<BulletLogic>().bulletCount = 1;
							temp.GetComponent<BulletLogic>().bulletCountByEachTime = 1;
							temp.GetComponent<BulletLogic>().LaunchAngle = 0;
							temp.GetComponent<BulletLogic>().shootDistance = 2;
							temp.GetComponent<BulletLogic>().speed = 5f;
							temp.GetComponent<BulletLogic>().shootWidth = 1;
						}
						foreach (var VARIABLE in transform.parent.GetComponentsInChildren<Collider>())
						{
							UnityEngine.Physics.IgnoreCollision(VARIABLE, temp.GetComponent<BulletLogic>().bulletBody.GetComponent<Collider>());

						}
						Bullet bulletClone = new Bullet(temp, 20, HYLDStaticValue.Players[playerID].hero, 20f);
						bullets.Add(bulletClone);
					}

				}

				HYLDStaticValue.Players[playerID].fireState = FireState.none;
			}
			
		}
		else if (HYLDStaticValue.Players[playerID].fireState != FireState.none &&
		         HYLDStaticValue.Players[playerID].playerManaValue < 30)//缺少发射能量
		{
			
		}
		
	}


}

*/