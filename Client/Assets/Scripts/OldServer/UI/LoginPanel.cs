/****************************************************
    Author:            龙之介
    CreatTime:    #CreateTime#
    Description:     Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using SocketProto;
using System.Net;
using System.Net.Sockets;

namespace LongZhiJie
{
	public class LoginPanel :BasePanel
	{
        public LoginRequest loginRequest;
        public InputField user, pass;
        public Button loginBtn,swichBtn;

        private void Start()
        {
            loginBtn.onClick.AddListener(OnLoginClick);
            swichBtn.onClick.AddListener(OnSwitchLogonClick);
        }
        private void OnLoginClick()
        {
            if (user.text == "" || pass.text == "")
            {
                Logging.HYLDDebug.LogError("密码用户名不得空");
                return;
            }
            loginRequest.SendRequest(user.text, pass.text);
        }
        private void OnSwitchLogonClick()
        {
            UIManger.PushPanel(UIPanelType.Logon);
        }
        public override void OnEnter()
        {
            base.OnEnter();
            gameObject.SetActive(true);
            user.text=PlayerPrefs.GetString("UserName","");
            pass.text=PlayerPrefs.GetString("PassWord", "");
            
        }
        public override void OnExit()
        {
            base.OnExit();
            gameObject.SetActive(false);
        }
        public override void OnRecovery()
        {
            base.OnRecovery();
            gameObject.SetActive(true);
            user.text = PlayerPrefs.GetString("UserName", "");
            pass.text = PlayerPrefs.GetString("PassWord", "");
        }
        public override void OnPause()
        {
            base.OnPause();
            gameObject.SetActive(false);
        }
        public override void OnResponse(MainPack pack)
        {
            switch (pack.Returncode)
            {
                case ReturnCode.Succeed:
                    UIManger.ShowMessage("登陆成功");
                    UIManger.PushPanel(UIPanelType.Room);
                    //Logging.HYLDDebug.Log("注册成功");
                    break;
                case ReturnCode.Fail:
                    UIManger.ShowMessage("登陆失败");
                    //Logging.HYLDDebug.Log("注册失败");
                    break;
                default:
                    Logging.HYLDDebug.Log("Def");
                    break;
            }
            Logging.HYLDDebug.LogError(pack.Returncode.ToString());
        }
    }
}