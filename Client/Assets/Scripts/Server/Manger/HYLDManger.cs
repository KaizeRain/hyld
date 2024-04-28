/****************************************************
    Author:            龙之介
    CreatTime:    2021/9/23 21:6:33
    Description:     荒野乱斗总游戏管理类
                    管理TCP，UI，PingPong
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using Server;
using SocketProto;
using LongZhiJie;
using System.IO;
using Google.Protobuf;
using UnityEngine.SceneManagement;

public class HYLDManger : Singleton<HYLDManger>
{
    private TCPServerManger _socketManger;
    private UIBaseManger _uiManger;
    private PingPongManger _pingpongManger;
    public UIBaseManger UIBaseManger
    {
        get { return _uiManger; }
    }
    protected override void Awake()
    {

        if (HYLDStaticValue.isNet)
        {
            base.Awake();
           
            //获取本机IP
            NetConfigValue.ServiceIP = IPManager.GetIP(ADDRESSFAM.IPv4);

            OnInit();
            _socketManger = new TCPServerManger();
            _pingpongManger = new PingPongManger();

            _socketManger.OnInit();
            _pingpongManger.Init();
        }
     
    }

    private void Start()
    {
        // 订阅场景加载完成的事件  
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void Update()
    {
        if (!HYLDStaticValue.isNet) return;
        if (HYLDStaticValue.是否为连接状态)
        {
            if (pingPongPack != null)
            {
                pingPongPack = null;
                _pingpongManger.OnResponse(Time.time);
            }
            _pingpongManger.Excute();
        }


        if (_uiManger != null && _uiManger.IsInit)
        {
            _uiManger.Excute(Time.deltaTime);
        }
    }
    

    private MainPack pingPongPack = null;
    public void Pong(MainPack pack)
    {
        pingPongPack = pack;
    }
    public void AddBattleReview(MainPack pack)
    {
        Debug.LogError(pack);
        string SavePath = Application.streamingAssetsPath + "/Review/"+ DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss") +".txt";
        Stream _stream=null;
        if (string.IsNullOrEmpty(SavePath))
            return;
        if (_stream == null)
        {
            var dir = Path.GetDirectoryName(SavePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _stream = File.Open(SavePath, FileMode.OpenOrCreate, FileAccess.Write);
        }

        var bytes = pack.ToByteArray();
        _stream.Write(bytes, 0, bytes.Length);
        _stream.Flush();
    }
    public void GetBattleReview()
    {
        string SavePath = Application.streamingAssetsPath + "/Review.txt";
        if (string.IsNullOrEmpty(SavePath))
            return;
        Stream _stream = null;
        if (_stream == null)
        {
            var dir = Path.GetDirectoryName(SavePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _stream = File.Open(SavePath, FileMode.OpenOrCreate, FileAccess.Read);
        }
        byte[] bytes = new byte[_stream.Length];
        _stream.Read(bytes, 0, (int)_stream.Length);
        MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(bytes, 0, (int)_stream.Length);
        Debug.LogError(pack);
    }
    
    public void OnInit()
    {
        Server.RequestManger.RemoveAllRequest();
        _uiManger = GameObject.FindWithTag("UIManger").transform.GetComponent<UIBaseManger>();
        _uiManger.OnInit();

    }

    private void OnDestroy()
    {
        // 取消订阅事件，以避免内存泄漏  
        SceneManager.sceneLoaded -= OnSceneLoaded;


        Server.RequestManger.RemoveAllRequest();
        Logging.HYLDDebug.FlushTrace();
        if(HYLDStaticValue.isNet)
        _socketManger.CloseSocket();
        _socketManger.OnDestroy();
    }
    public void Send(MainPack pack)
    {
        //5.发送消息
        _socketManger.Send(pack);
    }
    public void CloseClient()
    {
    }
    public void ShowMessage(string str, bool sync = false)
    {
        _uiManger.ShowMessage(str, sync);
    }

    // 每次加载完场景后调用的新函数  
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _uiManger = null; 

        if (scene.buildIndex != 2) // 使用scene的buildIndex属性来获取场景索引  
        {
            Debug.Log("加载新场景: " + scene.name);
            OnInit(); // 调用初始化函数  
        }
    }

    // OnLevelWasLoaded 已经过时了，SceneManager.sceneLoaded 是新的加载场景调用函数
    /*
    void OnLevelWasLoaded(int scenelevel)//每次加载完场景调用的函数
    {
        _uiManger = null;
        //Logging.HYLDDebug.LogError(" OnLevelWasLoaded:" + scenelevel);

        Debug.Log("加载新场景");

        if (scenelevel!=2)
            OnInit();
    }
    */


}
