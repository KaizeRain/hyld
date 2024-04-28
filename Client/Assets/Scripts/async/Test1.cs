/****************************************************
    Author:            龙之介
    CreatTime:    2021/11/12 15:30:26
    Description:    w学习委托特性
*****************************************************/

using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Testssss
{
    public class Button
    {
        public delegate void EventHandle();
        public EventHandle click;
    }
    public class Test1 :MonoBehaviour
	{
        private Button btn=new Button();
        private void Start()
        {
            btn.click += new Button.EventHandle(Handle1);
            btn.click += Handle2;
            btn.click += delegate { Logging.HYLDDebug.LogError("Handle3"); };
            btn.click += () => { Logging.HYLDDebug.LogError("Handle4"); };

            btn.click();
        }
        void Handle1()
        {
            Logging.HYLDDebug.LogError("Handle1");
        }
        void Handle2()
        {
            Logging.HYLDDebug.LogError("Handle2");
        }
    }
}