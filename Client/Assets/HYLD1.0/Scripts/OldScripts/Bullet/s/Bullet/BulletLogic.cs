/*
 * * * * * * * * * * * * * * * * 
 * Author:        魏佳楠
 * CreatTime:  2020/6/19 17:49:38 
 * Description: 子弹自己的逻辑
 * * * * * * * * * * * * * * * * 
*/

using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
public class BulletLogic : MonoBehaviour
{
    public void setBulletInformation(Hero hero)
    {
        shootDistance = hero.shootDistance;
        shootWidth = hero.shootWidth;
        bulletCount = hero.bulletCount;
        bulletDamage = hero.bulletDamage;
        LaunchAngle = hero.LaunchAngle;
        speed = hero.speed;
        bulletCountByEachTime = hero.bulletCountByEachTime;
        EachTimebulletsShootSpace = hero.EachTimebulletsShootSpace;
        bulletPrefab = hero.shell;
        BoomPrefab = hero.Boom;
        high = hero.high;
        IsParadola = hero.IsParadola;
    }


    // Start is called before the first frame update
    public float shootDistance;//射程
    public float shootWidth;//射击宽度（直线）
    public int bulletCount;//子弹总数量
    public int bulletDamage;//伤害
    public float LaunchAngle = 0;//扇形角度
    public float speed = 10f;//飞行速度
    public int bulletCountByEachTime;//每行子弹的数量
    public GameObject bulletPrefab;//子弹预制体
    private Transform firePos;//开火点
    public float EachTimebulletsShootSpace;//每次开火的时间戳
                                           //zyk的代码
    public int bulletOnwerID = -1;
    public GameObject bulletBody;
    public Vector3 Towards;
    public int bulletHurt;
    //dlh
    public bool IsParadola = false;//是否抛物线射击
    public GameObject BoomPrefab;//爆炸预制体
    public float high;//抛物线高度
    private Action<shell,float,float> CallBack;
    //jn的代码
    public void InitData(Action<shell, float, float> CallBack)
    {
        //Debug.LogError("???");
        firePos = bulletBody.transform;
        //bulletTransform= bulletBody.transform;
       // Logging.HYLDDebug.LogError("?");
        //gameObject.GetComponent<Material>().color=new Color(1,0,0);
        if (bulletOnwerID == 0)//0表示客户端的自己  
        {
            gameObject.GetComponent<AudioSource>().volume = 1;
            //这个代码会报错不知道什么原因
        }
        Invoke("Shoot", 0);
        this.CallBack=CallBack;
        Destroy(this.gameObject, 3);
    }
  
    
    void Shoot()
    {
        if(HYLDStaticValue.Players[bulletOnwerID].hero.Name=="贝亚"&& bulletCount!=1)
        {
            蜜蜂大招();
        }
        //Logging.HYLDDebug.LogError(bulletPrefab);
        //Logging.HYLDDebug.LogError("bulletLogic" + bulletOnwerID+gameObject.name);
        else if (LaunchAngle != 0)//扇形的
        {
            float angle = LaunchAngle / bulletCountByEachTime;
            StartCoroutine(ShanxingShoot(angle));
        }
        else if (LaunchAngle == 0)//直线型的
        {
            StartCoroutine(StraightShoot());
        }
    }
    IEnumerator ShanxingShoot(float angle)//扇形发射的
    {
        int tamp = 0;
        int sum = bulletCount;
        int eachSum = bulletCountByEachTime;
        int sumtamp;
        for (int k = 0; k < bulletCount / bulletCountByEachTime; k++)
        {
            float j = -bulletCountByEachTime / 2;
            if (tamp == 1) { tamp = 0; j += 0.5f; }
            else { tamp = 1; }
            sumtamp = UnityEngine.Random.Range(eachSum - 1, eachSum + 1);
            if (bulletCount / bulletCountByEachTime < 2) sumtamp = sum;//卡牌大师这种单发一排的
            else if (bulletCount <= 15) sumtamp = eachSum;//子弹稀疏的且子弹不多的
            else if (sum <= sumtamp) sumtamp = sum;//子弹过多的散弹枪
            else sum -= sumtamp;
            for (int i = 0; i < sumtamp; i++, j += UnityEngine.Random.Range(0.8f, 1.2f))
            {
                //Logging.HYLDDebug.LogError(Towards);
                GameObject go = GameObject.Instantiate(bulletPrefab, HYLDStaticValue.Players[bulletOnwerID].playerPositon, Quaternion.Euler(Towards)) as GameObject;
                go.transform.LookAt(go.transform.position + Towards);

                go.transform.Rotate(new Vector3(0, j * angle));
                ParadolaShoot(go);
                //Logging.HYLDDebug.LogError(go);
            }
            yield return new WaitForSeconds(EachTimebulletsShootSpace);
        }

    }
    IEnumerator StraightShoot()//直线型
    {
        //Quaternion pos = firePos.rotation;
        if (bulletCount /bulletCountByEachTime ==1)//单发子弹的
        {
            if(bulletCount==1)
            {
                //Logging.HYLDDebug.Log(Quaternion.Euler(Towards));
                GameObject go = GameObject.Instantiate(bulletPrefab, HYLDStaticValue.Players[bulletOnwerID].playerPositon, Quaternion.Euler(Towards)) as GameObject;
                go.transform.LookAt(go.transform.position + Towards);
                go.GetComponent<shell>().bulletOnwerID = bulletOnwerID;
                ParadolaShoot(go);
             
            }
            else
            {
                int i;
                float bulletDis = shootWidth / bulletCountByEachTime;
                float temp = bulletDis;
                for (i=0;i<bulletCount/2;i++)
                {
                    GameObject go = GameObject.Instantiate(bulletPrefab, HYLDStaticValue.Players[bulletOnwerID].playerPositon, Quaternion.Euler(Towards)) as GameObject;

                    temp -=bulletDis;
                    go.transform.LookAt(go.transform.position + Towards);
                    go.transform.Translate(Vector3.right * temp);
                    go.GetComponent<shell>().bulletOnwerID = bulletOnwerID;
                    ParadolaShoot(go);
          
                   // Logging.HYLDDebug.LogError(temp);
                    yield return new WaitForSeconds(EachTimebulletsShootSpace);
                }
                temp = 0;
                for (; i < bulletCount; i++)
                {
                    GameObject go = GameObject.Instantiate(bulletPrefab, HYLDStaticValue.Players[bulletOnwerID].playerPositon, Quaternion.Euler(Towards)) as GameObject;
                    
                    temp += bulletDis;
                   // Logging.HYLDDebug.LogError(temp);
                    go.transform.LookAt(go.transform.position + Towards);
                    go.transform.Translate(Vector3.right * temp);
                    go.GetComponent<shell>().bulletOnwerID = bulletOnwerID;
                    ParadolaShoot(go);
         
                    
                    yield return new WaitForSeconds(EachTimebulletsShootSpace);
                }
            }
        }
        else//连续发射的
        {
            float bulletDis = shootWidth / bulletCountByEachTime;
            //Logging.HYLDDebug.Log("?");
            //这个需要获取玩家的位置，不然会非常的僵硬特指firePos
            for (int k = 0; k < bulletCount; k++)
            {
                //print( HYLDStaticValue.Players[bulletOnwerID].body.transform.position);
                GameObject go = Instantiate(bulletPrefab, HYLDStaticValue.Players[bulletOnwerID].playerPositon, Quaternion.Euler(Towards)) as GameObject;
                
                bulletDis *= -1;
                go.transform.LookAt(go.transform.position + Towards);
                go.transform.Translate(Vector3.right * bulletDis);
                go.GetComponent<shell>().bulletOnwerID = bulletOnwerID;
                ParadolaShoot(go);
               

                yield return new WaitForSeconds(EachTimebulletsShootSpace);
            }
        }
    }

    public void 蜜蜂大招()
    {
        int i;
        float bulletDis = shootWidth / bulletCountByEachTime;
        float temp = bulletDis;
        float shootdistancetime = 0.35f;
        float rotatespeed = -0.3f;
        for (i = 0; i < bulletCount / 2; i++)
        {
            GameObject go = GameObject.Instantiate(bulletPrefab, HYLDStaticValue.Players[bulletOnwerID].playerPositon, Quaternion.Euler(Towards)) as GameObject;

            temp -= bulletDis;
            go.transform.LookAt(go.transform.position + Towards);
            go.transform.Translate(Vector3.right * temp);
            go.GetComponent<shell>().bulletOnwerID = bulletOnwerID;
            go.GetComponent<rolateSelf>().speed = rotatespeed;
            go.GetComponent<rolateSelf>().蜜蜂大招转弯时间 = shootdistancetime;
            shootdistancetime -= 0.1f;
            rotatespeed -= 1.7f;
            ParadolaShoot(go);
           
            
        }
        shootdistancetime = 0.35f;
        rotatespeed = 0.3f;
        temp = 0;
        for (; i < bulletCount; i++)
        {
            GameObject go = GameObject.Instantiate(bulletPrefab, HYLDStaticValue.Players[bulletOnwerID].playerPositon, Quaternion.Euler(Towards)) as GameObject;

            temp += bulletDis;
            go.transform.LookAt(go.transform.position + Towards);
            go.transform.Translate(Vector3.right * temp);
            go.GetComponent<shell>().bulletOnwerID = bulletOnwerID;
            go.GetComponent<rolateSelf>().speed = rotatespeed;
            go.GetComponent<rolateSelf>().蜜蜂大招转弯时间 = shootdistancetime;
            shootdistancetime -= 0.1f;
            rotatespeed += 1.7f;
            ParadolaShoot(go);
        }
    }

    public void ParadolaShoot(GameObject go)
    {
        if (IsParadola)//是否抛物线射击
        {
            //TODO:重置抛物线攻击
            
            GameObject Boom = GameObject.Instantiate(BoomPrefab, HYLDStaticValue.Players[bulletOnwerID].playerPositon + go.transform.forward * shootDistance, Quaternion.Euler(Towards)) as GameObject;
            // GameObject Boom = GameObject.Instantiate(BoomPrefab, go.transform);
            Boom.GetComponent<BoomCreater>().BoomDamage = bulletDamage;
            Boom.GetComponent<BoomCreater>().BoomOnwerID = bulletOnwerID;
            Boom.GetComponent<BoomCreater>().BKTime = shootDistance / speed;
            Boom.GetComponent<BoomCreater>().BoomRange = shootWidth;
            Boom.GetComponent<BoomCreater>().跟随物 = go;
            go.GetComponent<Rigidbody>().useGravity = true;
            go.GetComponent<Rigidbody>().velocity = go.transform.forward * speed + go.transform.up * shootDistance / speed * high / 2;
        }
        else
        {
            //Debug.LogError((go.transform.forward * speed));
           // go.GetComponent<Rigidbody>().velocity = go.transform.forward * speed;
        }
        //Debug.LogError("go:" + go.transform.forward * speed);
        shell shell = go.GetComponent<shell>();
        //Logging.HYLDDebug.LogError(go + "  "+ go.GetComponent<Rigidbody>().velocity);
        shell.bulletDamage = bulletDamage;
        shell.bulletOnwerID = bulletOnwerID;
        
        CallBack?.Invoke(shell, speed, shootDistance / speed);
        //Destroy(go, shootDistance / speed);
    }
}

