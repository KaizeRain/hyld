/****************************************************
    Author:            龙之介
    CreatTime:    2022/4/22 20:57:54
    Description:     Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using SocketProto;

namespace Manger
{
    public class HYLDPlayerManger : MonoBehaviour
    {
        public enum PlayerType
        {
            none,
            Self,
            Teammate,
            Enemy,
        }

        public bool initFinish { get; private set; }
        private GameObject PlayerClone;
        private Dictionary<int, int> dic_battleID_map_Playeridx;
        private List<PlayerLogic> list_playerlogics;

        public void InitData()
        {
           

            initFinish = false;
            PlayerClone = HYLDResourceManger.Load(HYLDResourceManger.Type.Player);
            List<BattlePlayerPack> list_battleUser = BattleData.Instance.list_battleUsers;
            list_playerlogics = new List<PlayerLogic>();
            dic_battleID_map_Playeridx = new Dictionary<int, int>();
            int teamid = BattleData.Instance.teamID;
            int battleid = BattleData.Instance.battleID;

            /*****************************单机模式************************************************/
            if (HYLDStaticValue.isNet == false)
            {
                list_battleUser = new List<BattlePlayerPack>();
                BattlePlayerPack user = new BattlePlayerPack();
                user.Battleid = 1;
                user.Hero = SocketProto.Hero.PeiPei;
                user.Id = 1;
                user.Teamid = 1;
                user.Playername = "单机";
                list_battleUser.Add(user);
                battleid = 1;
            }
            /*******************生成两个队伍的出生点(自己永远在紫色方系列)*********************/
            Queue<Vector3> myteam = new Queue<Vector3>();
            myteam.Enqueue(new Vector3(15, 1, -5));
            myteam.Enqueue(new Vector3(15, 1, 0));
            myteam.Enqueue(new Vector3(15, 1, 5));
            Queue<Vector3> otherTeam = new Queue<Vector3>();
            //敌人位置镜像生成
            otherTeam.Enqueue(new Vector3(-15, 1, 5));
            otherTeam.Enqueue(new Vector3(-15, 1, 0));
            otherTeam.Enqueue(new Vector3(-15, 1, -5));


            /*********************************初始化荒野乱斗玩家信息********************************************/
            int k = 0;
            foreach (var player in list_battleUser)
            {
                if (player.Battleid == battleid)
                {
                    //是player
                    HYLDStaticValue.Players.Add(new PlayerInformation(myteam.Dequeue(), player.Playername, HYLDStaticValue.Heros[(HeroName)((int)player.Hero)], player.Teamid, global::PlayerType.Self));
                }
                else if (player.Teamid == teamid)
                {
                    //和player一个队伍的
                    HYLDStaticValue.Players.Add(new PlayerInformation(myteam.Dequeue(), player.Playername, HYLDStaticValue.Heros[(HeroName)((int)player.Hero)], player.Teamid, global::PlayerType.Teammate));
                }
                else
                {
                    HYLDStaticValue.Players.Add(new PlayerInformation(otherTeam.Dequeue(), player.Playername, HYLDStaticValue.Heros[(HeroName)((int)player.Hero)], player.Teamid, global::PlayerType.Enemy));
                }
                Logging.HYLDDebug.LogError(player);
                dic_battleID_map_Playeridx.Add(player.Battleid, k++);

            }


            /**************生成player预制体实例***************/
            StartCoroutine(CreatePlayers());
        }
        /// <summary>
        /// 针对全体单位的同步
        /// </summary>
        public void OnLogicUpdate()
        {
            /*********UI数据同步**************/
            for (int i = 0; i < list_playerlogics.Count; i++)
            {
                list_playerlogics[i].OnUpdateLogic();
            }
        }
        /// <summary>
        /// 针对个体操作的同步
        /// </summary>
        /// <param name="opt"></param>
        public void OnLogicUpdate(PlayerOperation opt)
        {
            //Debug.LogError("logicupdate");
            int sign = 1;
            if (HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].teamID != HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].teamID)
            {
                sign = -1;//操作镜像
            }
            /**移动同步**/
            
            HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerMoveX = sign*opt.PlayerMoveX;
            HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerMoveY = sign*opt.PlayerMoveY;
                
            LZJ.Fixed3 tempDir= new LZJ.Fixed3(-HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerMoveX, 0f, HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerMoveY);
            LZJ.Fixed tempMagnitude = tempDir.magnitude;
        

            HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerMoveDir = tempDir.ToVector3();

            HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerMoveMagnitude = tempMagnitude.ToFloat();

            //Logging.HYLDDebug.Log($"Fixed:{temp.magnitude.ToFloat()}   float:{ t.magnitude}");

            LZJ.Fixed3 move = tempDir * (HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].移动速度+5) * HYLDStaticValue.fixedDeltaTime;
            //移动的同步是用定点数
            //逻辑位置 
            HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerPositon=(new LZJ.Fixed3(HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerPositon)+move).ToVector3();
            

            
            
            //Debug.LogError(HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerPositon.x+","+ HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].playerPositon.z); ;// * (new LZJ.Fixed(HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].移动速度) - 4) * new LZJ.Fixed(Time.fixedDeltaTime)); ;
            //Debug.LogError("logicupdate  over");
            //selfTransform.Translate((HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].移动速度 - 4) * Time.fixedDeltaTime * (temp), Space.World);
            //selfTransform.LookAt(selfTransform.position + temp);
            //= ;
            //selfTransform.position = HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].playerPositon;

            /*******子弹同步发射捏******/
            if (opt.BulletInfo != null)
            {
                //Logging.HYLDDebug.LogError(opt);

                HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].fireState = FireState.PstolNormal;

                Vector3 temp = LZJ.MathFixed.xAndY2UnitVector3(opt.BulletInfo.Towardy, opt.BulletInfo.Towardx);
                temp.x *= -1 * sign;
                temp.z *= sign;
                //temp.z = FirePositionX > 0 ? temp.z : -temp.z;

                HYLDStaticValue.Players[dic_battleID_map_Playeridx[opt.Battleid]].fireTowards = temp;
            }
           


        }
        private IEnumerator CreatePlayers()
        {
            int k = 0;
            foreach (var player in HYLDStaticValue.Players)
            {
                yield return new WaitForEndOfFrame();
                GameObject temp = Instantiate(PlayerClone,Vector3.zero, Quaternion.identity);
                temp.transform.GetChild(0).position = player.playerPositon;
                //Logging.HYLDDebug.LogError(player.playerPositon);
                if (player.playerType == global::PlayerType.Self)
                {
                    temp.GetComponent<HYLDPlayerController>().isSelf = true;
                    HYLDStaticValue.playerSelfIDInServer = k;
                }
                else
                {
                    temp.GetComponent<HYLDPlayerController>().isSelf = false;
                }
                temp.GetComponent<HYLDPlayerController>().playerID = k;
                list_playerlogics.Add(temp.GetComponent<PlayerLogic>());
                list_playerlogics[list_playerlogics.Count-1].playerID = k;
                HYLDStaticValue.Players[k].body = temp;
                k++;
                //FactoryManager.CharacterFactory.CreateCharacter(WeaponType.Gun, new Vector3(0, 0, 0), HeroName.RuiKe);
            }

            for (int i = 0; i < k; i++)
            {
                Collider c1 = HYLDStaticValue.Players[i].body.transform.GetChild(0).GetComponent<BoxCollider>();
                for (int j = i + 1; j < k; j++)
                {
                    Collider c2 = HYLDStaticValue.Players[j].body.transform.GetChild(0).GetComponent<BoxCollider>();
                    Physics.IgnoreCollision(c1,c2,true);
                }
            }
            initFinish = true;
        }
    }
}