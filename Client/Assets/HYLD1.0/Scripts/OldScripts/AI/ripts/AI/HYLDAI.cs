/*
****************
 * Author:        赵元恺
 * CreatTime:  2020/7/4 22:41 
 * Description: AI
 **************** 
*/
using UnityEngine;
using System.Collections;
using BT;
using System.Collections.Generic;

public class HYLDAI : BTTree {
	private static string DESTINATION = "Destination";
	private static string 攻击目标 = "攻击目标";
	public  PlayerInformation[] enemy;
	//public  PlayerInformation[] boss;
	private static string GOBLIN_NAME = "Goblin";
	private static string RUN_ANIMATION = "Run";
	private static string FIGHT_ANIMATION = "Fight";
	private static string IDLE_ANIMATION = "Idle";
	[HideInInspector]
	public const string SELFTRANSFORM="selfTransform";
	public const string PLAYERID = "PlayerId";

	private static int _playerIDid;
	private static int _selfTransformId; 

	//public Transform selfTransform;
	//public float speed;
	//public float sightForEnemy;
	//public float sightForEnemyEscape;
	public float fightDistance;
	public float moveDistance;
	public int PlayerID = -1;
	protected Animator Ani;
	private void Start()
	{   //playerID=0      敌人是  1  3   5
		//Logging.HYLDDebug.LogError(PlayerID);
		return;
		Init();
		if (PlayerID == HYLDStaticValue.playerSelfIDInServer) return;
		if (HYLDStaticValue.ModenName == "HYLDTryGame") return;
		Logging.HYLDDebug.LogError(HYLDStaticValue.Players[PlayerID].playerName);
		if (HYLDStaticValue.Players[PlayerID].playerName != "A") return;
		//if (PlayerID == 2  || PlayerID == 4) return;
		//if (PlayerID == 1 || PlayerID == 3 || PlayerID == 5) return;
		_selfTransformId = database.GetDataId(SELFTRANSFORM);
		database.SetData<Transform>(_selfTransformId, selfTransform);
		_playerIDid = database.GetDataId(PLAYERID);
		database.SetData<int>(_playerIDid, PlayerID);
		Ani = GetComponent<PlayerLogic>().bodyAnimator;
		Transform[] go; 
		go = GameObject.Find("MAP").GetComponentsInChildren<Transform>();
		foreach(Transform gos in go)
		{
			//gos.name == "Plain01_4 (1)(Clone)"|| gos.name == "Plain02_4 (1)(Clone)"|| gos.name == "Plain_Trap_4 (1)(Clone)"||gos.name== "Plain_Poison_4 (1)(Clone)"
			if (gos.name== "Desert01_4 (1)(Clone)"|| gos.name == "Desert_Field_02(Clone)"|| gos.name == "Desert02_4 (1)(Clone)")
			{
				//Logging.HYLDDebug.LogError(gos.gameObject);
				Obstacles.Add(gos.gameObject);
			}
		}

		//Obstacles.Add(go.transform.Find())
		//Logging.HYLDDebug.LogError(enemy[1]);
		_root.Activate(database);
	}

	private void DoAniMove(float value)
	{
		//Logging.HYLDDebug.LogError(value);
		Ani.SetFloat("Speed", value);
	}
	protected override void Init () {
		return;
		base.Init ();

		PlayerID = database.transform.GetComponent<PlayerLogic>().playerID;
		if (PlayerID == HYLDStaticValue.playerSelfIDInServer) return;

		if (PlayerID % 2 == 0)
		{
			enemy = new PlayerInformation[] { HYLDStaticValue.Players[1], HYLDStaticValue.Players[3], HYLDStaticValue.Players[5] };
		}

		else
		{
			enemy = new PlayerInformation[] { HYLDStaticValue.Players[0], HYLDStaticValue.Players[2], HYLDStaticValue.Players[4] };
		}

		_root = new BTPrioritySelector();

		//Logging.HYLDDebug.LogError(HYLDStaticValue.Players[PlayerID].hero.攻击距离);
		CheckInSight checkEnemysInFightDistance = new CheckInSight(HYLDStaticValue.Players[PlayerID].hero.攻击距离, enemy,攻击目标);


		DoRun run = new DoRun(DESTINATION, HYLDStaticValue.Players[PlayerID].移动速度, DoAniMove);
		FindToTargetDestination findToTargetDestination = new FindToTargetDestination(enemy, DESTINATION, HYLDStaticValue.Players[PlayerID].hero.最小离敌人距离);

		BTSequence fight = new BTSequence(checkEnemysInFightDistance);
		{
			fight.AddChild(new Attack(攻击目标));
		}


		BTParallelFlexible MoveAndFight = new BTParallelFlexible();
		{
			BTParallel parallel = new BTParallel(BTParallel.ParallelFunction.Or);
			{
				parallel.AddChild(findToTargetDestination);
				parallel.AddChild(run);     // Reuse Run
			}
			MoveAndFight.AddChild(parallel);
			MoveAndFight.AddChild(fight);

		}
		_root.AddChild(MoveAndFight);



	}
}
public static class MyRandom
{

	//public static int seed = (int)System.DateTime.Now.Ticks;
	public static int seed = PlayerPrefs.GetInt("Seed");
	public static int MyRandomRange(int x,int y)
	{
		Random.InitState(seed);

		return Random.Range(x, y);
	}
	public static float MyRandomRange(float x, float y)
	{
		Random.InitState(seed);

		return Random.Range(x, y);
	}

}
