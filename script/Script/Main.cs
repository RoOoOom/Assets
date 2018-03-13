// ***************************************************************
//  FileName: AppStart.cs
//  Version : 1.0
//  Date    : 2016/6/22
//  Author  : cjzhanying 
//  Copyright (C) 2016 - Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: 游戏启动脚本
//  -------------------------------------------------------------
//  History:
//  -------------------------------------------------------------
// ***************************************************************
using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    //是否使用本地资源
    public bool useLocalRes = true;
    //是否显示帧率
    public bool showFPS = false;
    //游戏默认帧率
    public int defaultFPS = 30;
    //是否输出log
    public bool useLog = true;
    //log等级
    public JZLog.JZLogLevel logLevel = JZLog.JZLogLevel.Info;
    //是否上次错误日志到服务器
    public bool reportError = true;
    //初始语言
    public Language language = Language.ZH_CN;
    //是否显示log-console
    public bool showLogConsole = false;
    //服务器配置
    public ServerInfoType serverInfoType = ServerInfoType.inner;

#if UNITY_EDITOR
    public TransitionType type = TransitionType.SMFadeTransition;
#endif
    void Awake()
    {
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
#endif
        Application.runInBackground = true;
        Application.targetFrameRate = defaultFPS;
        JZLog.Init(useLog, reportError, logLevel);
        GameConfig.useLocalRes = useLocalRes;
        GameConfig.showFPS = showFPS;
        GameConfig.language = language;
        GameConfig.showLogConsole = showLogConsole;
        PathUtils.Init();
#if UNITY_EDITOR
        SceneHelper.instance.SetTransitionType(type);
        //Debug.Log(Application.persistentDataPath);
#endif
    }

    void Start()
    {
#if UNITY_EDITOR
        GameStart();
#elif UNITY_ANDROID || UNITY_IOS
        StartCoroutine(AppStart());
#endif
    }

    IEnumerator AppStart()
    {
#if UNITY_ANDROID || UNITY_IOS
        UnityEngine.Handheld.PlayFullScreenMovie("welcome.mp4", Color.black, UnityEngine.FullScreenMovieControlMode.CancelOnInput);
#endif
        yield return null;
        GameStart();
    }

    void GameStart()
    {
        GameWorld.Startup(serverInfoType);
        Destroy(gameObject);
    }
}