/****************************************************
    Author:            龙之介
    CreatTime:    2021/11/13 19:35:52
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
    public class Info
    {
        public string name;
        public int year;
        public Info(string name, int year)
        {
            this.name = name;
            this.year = year;
        }
    }
    public class DB
    {
        public List<Info> Products=new List<Info>();
    }
	public class Tes3 :MonoBehaviour
	{
        public void Start()
        {
            DB dB = new DB();
            for (int i = 0; i < 10; i++)
                dB.Products.Add(new Info((i).ToString(),100-i));
            var offers =
                from product in dB.Products
                where product.year < 95
                orderby product.name descending
                select new
                {
                    product.name,
                    product.year
                };
            foreach (var x in offers)
            {
                Logging.HYLDDebug.LogError(x.name+"   "+x.year);
            }
        }
    }
}