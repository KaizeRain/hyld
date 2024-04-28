/*
 * * * * * * * * * * * * * * * * 
 * Author:        赵元恺
 * CreatTime:  2020/7/29 21：22 
 * Description:  完成一些英雄拖尾特效，和特殊子弹的效果
 * * * * * * * * * * * * * * * * 
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shell : MonoBehaviour
{
    // Start is called before the first frame update

    public int bulletDamage;
    //这个是子弹内部的代码
    public string trailerName = "NULL";
    private float trailerTimer = 0;
    public GameObject trailer=null;
    public int bulletOnwerID = -1;
    private float temp = 0;
    public GameObject PanNiZhaLiXiaoGuo;
    public bool isZhaLie = false;
    private Vector3 m_preVelocity = Vector3.zero;

    private int playerid=-1;
    private float speed;
    private float health;
    private float health_timer_start;
    private float health_timer_cur;
    private Action<shell> diecallBack;
    private Vector3 Net_pos;
    private Vector3 moveDir;
    public void InitData(float _speed,float _health,Action<shell> diecallBack)
    {
        speed = _speed;
        health = _health;
        health_timer_start = Time.time;
        this.diecallBack = diecallBack;
        if (trailerName == "PeiPei") temp = 0.3f;
        Net_pos = transform.position;

        moveDir = transform.forward;
    }

    public bool IsParadola = false;
    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, Net_pos, Time.fixedDeltaTime * 5);
        if (trailerName == "LiAng")
        {
            //Logging.HYLDDebug.LogError(bulletDamage);
            bulletDamage -= 9;
        }
        //更新拖尾
        if (trailer != null)
        {
            trailerTimer += Time.deltaTime;
            if (trailerTimer > 0.04f)
            {
                ShowTrailer();
                trailerTimer = 0;

            }
        }
        health_timer_cur = Time.time;
    }
    //
    public void OnUpdateLogic()
    {
       //Debug.LogError(health_timer_cur+" - "+ health_timer_start+ " = "+(health_timer_cur - health_timer_start)+" >= "+health);
        if (health_timer_cur - health_timer_start >= health)
        {
            //到达死亡时间
            NetGlobal.Instance.AddAction(Die);
            return;
        }
        //Debug.LogError("shell:" +transform.forward * speed);
        //更新移动  因为这个计算没有太多花里胡哨的(只有乘)，所以我就用浮点了，所以有误差

        Net_pos += moveDir * speed* Server.NetConfigValue.frameTime;

        if (HYLDStaticValue.Players[0].teamID != HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].teamID)
        {
            Logging.HYLDDebug.Trace($"shell {bulletOnwerID} pos: ({ Net_pos.x*-1},{ Net_pos.z*-1}) ");
        }
        else
        {
            Logging.HYLDDebug.Trace($"shell {bulletOnwerID} pos: ({ Net_pos.x},{ Net_pos.z}) ");
        }


        // Logging.HYLDDebug.LogError(transform.GetComponent<Rigidbody>().velocity);   
   

    }
    private void OnTriggerEnter(Collider other)
    {
    
        //  print(other.transform.parent.GetComponent<PlayerLogic>().playerID);
        //print("?");
        if (other.tag == "RedSafeBox")
        {
            HYLDStaticValue.RedBP -= bulletDamage;
            HYLDStaticValue.ConfirmWinOrNot = true;
            if (isZhaLie == false)
                if (gameObject.name != "子弹6卡牌(Clone)") 
                    if(gameObject.name != "大招3瑞科(Clone)") 
                        if( gameObject.name != "大招13柯尔特")
                            if (gameObject.name != "大招14雪莉(Clone)")
                                if (gameObject.name != "大招11格尔1(Clone)")
                                    Die();
            if (gameObject.name == "子弹12钱袋(Clone)")
            {


                Instantiate(PanNiZhaLiXiaoGuo, transform.position, transform.rotation);

            }
            if (gameObject.name == "子弹17仙人掌(Clone)")
            {

                Instantiate(PanNiZhaLiXiaoGuo, transform.position, transform.rotation);
            }
        }
        if (other.tag == "BlueSafeBox")
        {
            HYLDStaticValue.BlueBP -= bulletDamage;
            HYLDStaticValue.ConfirmWinOrNot = true;
            if (isZhaLie == false)
                if (gameObject.name != "子弹6卡牌(Clone)")
                    if( gameObject.name != "大招3瑞科(Clone)")
                        if(gameObject.name != "大招13柯尔特(Clone)")
                            if(gameObject.name!= "大招14雪莉(Clone)")
                                if (gameObject.name != "大招11格尔1(Clone)")
                                    Die();
            if (gameObject.name == "子弹12钱袋(Clone)")
            {


                Instantiate(PanNiZhaLiXiaoGuo, transform.position, transform.rotation);

            }
            if (gameObject.name == "子弹17仙人掌(Clone)")
            {

                Instantiate(PanNiZhaLiXiaoGuo, transform.position, transform.rotation);
            }
        }
        else if (other.tag == "Obstacles")
        {
            if (gameObject.name == "大招3瑞科(Clone)")
            {
                Vector3 m_forceHit = gameObject.GetComponent<Rigidbody>().velocity;
                m_forceHit.x *= -1;
                gameObject.GetComponent<Rigidbody>().velocity = m_forceHit;
            }
            else if (gameObject.name == "子弹3瑞科(Clone)")
            {
                Vector3 m_forceHit = gameObject.GetComponent<Rigidbody>().velocity;
                m_forceHit.x *= -1;
                gameObject.GetComponent<Rigidbody>().velocity = m_forceHit;
            }
            else if(gameObject.name == "大招13柯尔特(Clone)")
            {
                Destroy(other.gameObject);
            }
            else if(gameObject.name== "大招14雪莉(Clone)")
            {
                Destroy(other.gameObject);
            }
            else if(gameObject.name== "大招11格尔1(Clone)")
            {

            }
            else 
            {
                Die();
            }
        }
        else if(other.tag=="Wall")
        {
            if (gameObject.name == "大招3瑞科(Clone)")
            {
                Vector3 m_forceHit = gameObject.GetComponent<Rigidbody>().velocity;
                m_forceHit.x *= -1;
                gameObject.GetComponent<Rigidbody>().velocity = m_forceHit;
            }
            else if(gameObject.name == "子弹3瑞科(Clone)")
            {
                Vector3 m_forceHit = gameObject.GetComponent<Rigidbody>().velocity;
                m_forceHit.x *= -1;
                gameObject.GetComponent<Rigidbody>().velocity = m_forceHit;
            }
            else
            {
                Die();
            }
        }
        else if (other.tag == "Player")
        {
            {
                int targetPlayerId = other.transform.parent.GetComponent<PlayerLogic>().playerID;
              
                if (HYLDStaticValue.Players[targetPlayerId].teamID != HYLDStaticValue.Players[bulletOnwerID].teamID)
                {
                    //Logging.HYLDDebug.LogError(targetPlayerId);
                    //Logging.HYLDDebug.LogError(bulletOnwerID);
                    if (HYLDStaticValue.Players[targetPlayerId].是否有防护罩)
                    {
                        Die();
                        return;
                    }

                    
                    if (gameObject.name == "子弹8乌鸦(Clone)")
                    {
                        HYLDStaticValue.Players[targetPlayerId].body.transform.Find("Canvas").Find("HeiYa").gameObject.SetActive(true);
                        HYLDStaticValue.Players[targetPlayerId].isPoisoning = true;
                    }
                    if (gameObject.name == "子弹16小蜜蜂(Clone)")
                    {
                        
                        if(HYLDStaticValue.Players[bulletOnwerID].beeReady)
                        {
                            HYLDStaticValue.Players[bulletOnwerID].body.transform.Find("Canvas").Find("Bee").gameObject.SetActive(false);
                            HYLDStaticValue.Players[bulletOnwerID].beeReady = false;
                            bulletDamage += 1200;
                        }
                        else
                        {
                            HYLDStaticValue.Players[bulletOnwerID].beeReady = true;
                            HYLDStaticValue.Players[bulletOnwerID].body.transform.Find("Canvas").Find("Bee").gameObject.SetActive(true);
                        }
                    }
                    HYLDStaticValue.Players[bulletOnwerID].当前能量 += bulletDamage/2f;
                    HYLDStaticValue.Players[targetPlayerId].playerBloodValue -= bulletDamage;//扣血
                        if(isZhaLie==false)
                            if(gameObject.name!= "子弹6卡牌(Clone)")
                                if(gameObject.name != "大招3瑞科(Clone)")
                                    if(gameObject.name != "大招13柯尔特(Clone)")
                                        if (gameObject.name != "大招14雪莉(Clone)")
                                            if (gameObject.name != "大招11格尔1(Clone)")
                                            Die();
                    
                    
                    if (gameObject.name == "子弹12钱袋(Clone)")
                    {
                        
                        
                        Instantiate(PanNiZhaLiXiaoGuo, transform.position, transform.rotation);
                       
                    }
                    if (gameObject.name == "子弹17仙人掌(Clone)")
                    {
                     
                        Instantiate(PanNiZhaLiXiaoGuo, transform.position, transform.rotation);
                    }
                    if (gameObject.name == "大招14雪莉(Clone)")
                    {
                        other.GetComponent<移动型大招>().playerid = targetPlayerId;
                        if (HYLDStaticValue.Players[targetPlayerId].isNotDie)
                        {
                            HYLDStaticValue.Players[targetPlayerId].被控制 = true;
                            HYLDStaticValue.Players[targetPlayerId].isNotDie = false;
                            other.GetComponent<移动型大招>().当前英雄 = HeroName.XueLi;
                            other.GetComponent<移动型大招>().控制时间 = 0.6f;

                            other.GetComponent<移动型大招>().子弹位置 = gameObject.transform.position;
                        }
                    }
                    if (gameObject.name == "大招11格尔1(Clone)")
                    {
                        other.GetComponent<移动型大招>().playerid = targetPlayerId;

                        if (HYLDStaticValue.Players[targetPlayerId].isNotDie)
                        {
                            HYLDStaticValue.Players[targetPlayerId].被控制 = true;
                            HYLDStaticValue.Players[targetPlayerId].isNotDie = false;
                            other.GetComponent<移动型大招>().当前英雄 = HeroName.GeEr;
                            other.GetComponent<移动型大招>().控制时间 = 0.5f;

                            other.GetComponent<移动型大招>().格尔子弹 = gameObject;
                        }
                    }
                    else if (gameObject.name == "大招16小蜜蜂(Clone)")
                    {
                        other.transform.parent.GetComponent<PlayerLogic>().减速(3);
                    }
                }
            
            }
        }
        else if(other.tag=="Text")
        {
            other.gameObject.GetComponent<TextLogic>().playerBlood -= bulletDamage;
            other.gameObject.GetComponent<TextLogic>().playerHurt(bulletDamage);
            #region ispanni
            other.GetComponent<TextLogic>().ispanni = false;
            if (isZhaLie) other.GetComponent<TextLogic>().ispanni = true;
            if(gameObject.name== "大招3瑞科(Clone)")
            {
                other.GetComponent<TextLogic>().ispanni = true;
            }
            else if(gameObject.name == "大招11格尔1(Clone)")
            {
                other.GetComponent<TextLogic>().ispanni = true;

                if (!other.GetComponent<TextLogic>().被控制)
                {
                    other.GetComponent<TextLogic>().被控制 = true;
                    other.GetComponent<TextLogic>().控制时间 = 0.5f;
                    other.GetComponent<TextLogic>().当前英雄 = HeroName.GeEr;

                    other.GetComponent<TextLogic>().格尔子弹 = gameObject;
                }
            }
           else if(gameObject.name == "大招13柯尔特(Clone)")
            {
                other.GetComponent<TextLogic>().ispanni = true;
            }
           else  if (gameObject.name== "子弹6卡牌(Clone)")
            {
                other.GetComponent<TextLogic>().ispanni = true;
            }
            #endregion
            else if (gameObject.name== "子弹8乌鸦(Clone)")
            {
                other.GetComponent<TextLogic>().isPoisoning = true;
            }
            else if (gameObject.name == "子弹16小蜜蜂(Clone)")
            {
                if (HYLDStaticValue.Players[bulletOnwerID].beeReady)
                {
                    HYLDStaticValue.Players[bulletOnwerID].body.transform.Find("Canvas").Find("Bee").gameObject.SetActive(false);
                    HYLDStaticValue.Players[bulletOnwerID].beeReady = false;
                    bulletDamage += 1200;
                }
                else
                {
                    HYLDStaticValue.Players[bulletOnwerID].body.transform.Find("Canvas").Find("Bee").gameObject.SetActive(true);
                    HYLDStaticValue.Players[bulletOnwerID].beeReady = true;
                }
            }
            else if (gameObject.name == "子弹12钱袋(Clone)")
            {
        
                Die();
                other.GetComponent<TextLogic>().ispanni = true;
                GameObject go= Instantiate(PanNiZhaLiXiaoGuo, transform.position, transform.rotation);
                
            }
            else if (gameObject.name == "子弹17仙人掌(Clone)")
            {
                Die();
                other.GetComponent<TextLogic>().ispanni = true;
                Instantiate(PanNiZhaLiXiaoGuo, transform.position, transform.rotation);
            }
            //Logging.HYLDDebug.LogError(击退timer);
            else if (gameObject.name == "大招14雪莉(Clone)")
            {
                other.GetComponent<TextLogic>().ispanni = true;
                if(!other.GetComponent<TextLogic>().被控制)
                {
                    other.GetComponent<TextLogic>().被控制 = true;
                    other.GetComponent<TextLogic>().控制时间 = 0.4f;
                   

                    other.GetComponent<TextLogic>().子弹位置 = gameObject.transform.position;
                }


            }
            else if(gameObject.name== "大招16小蜜蜂(Clone)")
            {

               
                    
              
                other.gameObject.GetComponent<TextLogic>().减速();
                Die();
                //Logging.HYLDDebug.LogError((other.gameObject.GetComponent<shell>().bulletDamage));
            }

            if (other.gameObject.GetComponent<TextLogic>().ispanni == false)
            { Die(); }
        }
    }
    public void ShowTrailer()
    {
        
        GameObject go = Instantiate(trailer, this.gameObject.transform.position, this.gameObject.transform.rotation);
        float tamp=0;
        if (trailerName=="KaPai")
        {
            go.transform.Translate(Vector3.down * UnityEngine.Random.Range(-0.5f, 0.5f));
            go.transform.localScale = new Vector3(UnityEngine.Random.Range(0.05f, 0.17f), UnityEngine.Random.Range(0.05f, 0.17f),UnityEngine.Random.Range(0.05f, 0.17f));
           
        }

        else if(trailerName=="GeEr")
        {
           
            go.transform.Translate(Vector3.down * UnityEngine.Random.Range(-0.5f, 0.5f));
            go.transform.localScale = new Vector3(UnityEngine.Random.Range(0.1f, 0.3f), UnityEngine.Random.Range(0.1f, 0.3f), UnityEngine.Random.Range(0.1f, 0.3f));
            
        }
        else if(trailerName=="GeErDaZhao")
        {
            go.transform.Translate(Vector3.down * UnityEngine.Random.Range(-0.5f, 0.5f));
            go.transform.localScale = new Vector3(UnityEngine.Random.Range(0.4f, 0.9f), UnityEngine.Random.Range(0.4f, 0.9f), UnityEngine.Random.Range(0.4f, 0.9f));
            GameObject go1 = Instantiate(trailer, this.gameObject.transform.position, this.gameObject.transform.rotation);
            go1.transform.Translate(Vector3.right * UnityEngine.Random.Range(-1f, -0.5f));
            go1.transform.localScale = new Vector3(UnityEngine.Random.Range(0.4f, 0.9f), UnityEngine.Random.Range(0.4f, 0.9f), UnityEngine.Random.Range(0.4f, 0.9f));
            GameObject go2 = Instantiate(trailer, this.gameObject.transform.position, this.gameObject.transform.rotation);
            go2.transform.Translate(Vector3.right * UnityEngine.Random.Range(0.5f, 1f));
            go2.transform.localScale = new Vector3(UnityEngine.Random.Range(0.4f, 0.9f), UnityEngine.Random.Range(0.4f, 0.9f), UnityEngine.Random.Range(0.4f, 0.9f));
            Destroy(go1, 0.2f);
            Destroy(go2, 0.2f);
        }
        else if(trailerName=="JingBi")
        {
            go.transform.Translate(Vector3.down * UnityEngine.Random.Range(-0.2f, 0));
            temp = 0.5f;
            go.transform.localScale = new Vector3(1,1,1)*temp;
            tamp += 0.2f;
        }
        else if(trailerName=="PeiPei")
        {

            go.transform.Translate(Vector3.back);
            go.transform.localScale = new Vector3(1,1,1)*temp;
            temp += 0.01f;
            bulletDamage += 60;
        }
        
        if(trailerName!="PeiPei")
        Destroy(go, 0.2f+tamp);
    }
    public virtual void ShowSkill()
    {

    }
    private void Die()
    {
        diecallBack?.Invoke(this);
        //到达死亡时间
        Destroy(gameObject);
    }
}


