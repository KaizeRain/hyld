/****************************************************
    Author:            龙之介
    CreatTime:    2022/4/23 16:48:15
    Description:     命令模式
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

interface Commad 
{
    void Execute();
}



/// <summary>
/// 命令模式
/// </summary>
public class CommandManger
{
    private static CommandManger instance;
    public static CommandManger Instance
    {
        get
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (instance == null)
            {
                instance = new CommandManger();
            }
            return instance;
        }
    }
    private CommandManger()
    {
    }

    private List<Commad> allCommad=new List<Commad>();
    private int commadID = 0;
    /// <summary>
    /// 移动命令，使玩家移动
    /// </summary>
    public class AttackCommad : Commad
    {
        private float dx, dy;
        public AttackCommad(float x, float y)
        {
            dx = x;
            dy = y;
        }
        public void Execute()
        {
            Manger.BattleData.Instance.selfOperation.BulletInfo = new SocketProto.BulletInfo();// = dx;
            Manger.BattleData.Instance.selfOperation.BulletInfo.Towardx = dx;
            Manger.BattleData.Instance.selfOperation.BulletInfo.Towardy = dy;
           // Manger.BattleData.Instance.selfOperation.PlayerMoveY = dy;
            //HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireState = FireState.PstolNormal;
            //HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireState = FireState.ShotgunSuper;
            
           // Vector3 temp = xAndY2UnitVector3(FirePositionY, FirePositionX);
            //temp.x *= -1;

           // HYLDStaticValue.Players[HYLDStaticValue.playerSelfIDInServer].fireTowards = temp;
        }
    }
    public void AddCommad_Attack(float dx, float dy)
    {
        Commad commad = new AttackCommad(dx, dy);
        allCommad.Add(commad);
    }
    /// <summary>
    /// 移动命令，使玩家移动
    /// </summary>
    public class MoveCommad : Commad
    {
        private float dx, dy;
        public MoveCommad(float x, float y)
        {
            dx = x;
            dy = y;
        }
        public void Execute()
        {

            //Manger.BattleData.Instance.selfOperation.OpType = SocketProto.OperationType.Move;
            Manger.BattleData.Instance.selfOperation.PlayerMoveX = dx;
            Manger.BattleData.Instance.selfOperation.PlayerMoveY = dy;
            //Logging.HYLDDebug.Log($"addCommad_move float:{Manger.BattleData.Instance.selfOperation.PlayerMoveX} {Manger.BattleData.Instance.selfOperation.PlayerMoveY}");
            //Logging.HYLDDebug.LogError("Execute:" + Manger.BattleData.Instance.selfOperation);
        }
    }
    public void AddCommad_Move(float dx,float dy)
    {
        Commad commad = new MoveCommad(dx, dy);
        allCommad.Add(commad);
    }
    public void AddCommad_Move(LZJ.Fixed dx, LZJ.Fixed dy)
    {

        Commad commad = new MoveCommad(dx.ToFloat(), dy.ToFloat());
        //Logging.HYLDDebug.Log($"addCommad_move Fixed:{dx} {dy}");
        allCommad.Add(commad);
    }
    public void Execute()
    {
        int n = allCommad.Count;
        for (; commadID < n; commadID++)
        {
            allCommad[commadID].Execute();
        }
    }
}
