/****************************************************
    Author:            龙之介
    CreatTime:    2021/9/22 18:36:42
    Description:     静态类
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;



namespace Server
{
	public class NetConfigValue :MonoBehaviour
	{
        public const string RegexValue = "^(17[0-9]|13[0-9]|14[5|7]|15[0|1|2|3|4|5|6|7|8|9]|18[0|1|2|3|5|6|7|8|9])\\d{8}$";
        public static string ServiceIP = "";
        public static readonly int ServiceTCPPort = 7778;
        public static readonly int ServiceUDPPort = 7777;
        public static readonly float frameTime = 0.066f;
        public static readonly float canPlayerRestoreHealthTime = 2;
    }
}