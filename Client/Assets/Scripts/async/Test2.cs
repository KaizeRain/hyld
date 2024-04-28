/****************************************************
    Author:            龙之介
    CreatTime:    2021/11/13 18:44:36
    Description:     Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;



namespace LongZhiJie
{
    public class Human
    {
        public string name;
        public Weapon weapon;
        public void Get()
        {
            Logging.HYLDDebug.LogError($"name:{name} weapon: {weapon.name}  {weapon.atk}");
        }
    }
    public class Weapon
    {
        public string name;
        public int atk;
    }

	public class Test2 :MonoBehaviour
	{
        private void Start()
        {
            var human = new Human();
            human.name = "lzj1";
            human.weapon = new Weapon();
            human.weapon.name = "npy1";
            human.weapon.atk = 1;
            human.Get();
            var human1 = new Human
            {
                name = "lzj",
                weapon = new Weapon
                {
                    name = "npy",
                    atk = 1
                }
            };
            human1.Get();
        }
    }
}