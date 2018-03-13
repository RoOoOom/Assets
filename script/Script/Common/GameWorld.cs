// ***************************************************************
//  FileName: GameWorld.cs
//  Version : 1.0
//  Date    : 2016/6/22
//  Author  : cjzhanying 
//  Copyright (C) 2016 - Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: 游戏主循环
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
using JZWL;

public class GameWorld : MonoBehaviour
{
    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /* ====================================================================================================================*/

    public static GameWorld instance { get { return m_instance; } }
    private static GameWorld m_instance = null;

    // Layer层枚举
    public enum LayerEnum
    {
        // 主摄像机开启层
        Default = 0,
        TransparentFX = 1, // 透明层 主摄像机不显示
        IgnoreRaycast = 2,
        Water = 4,

        // 主摄像机屏蔽层
        UIManager = 8,     // 默认UI层
        UIHide = 10,       // 隐藏UI使用的层
    }

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /* ====================================================================================================================*/

    public static GameObject GetGameObject()
    {
        if (m_instance == null)
        {
            GameObject go = GameObject.Find("GameWorld");
            if (go == null)
            {
                go = new GameObject("GameWorld");
                go.AddComponent<GameWorld>();
                go.AddComponent<NetSpeed>();
            }
            return go;
        }
        else
        {
            return m_instance.gameObject;
        }
    }

    private class ApkVersionInfo
    {
        public string version;
        public string url;
        public string notice;
    }

    /// <summary>
    /// 检测apk版本，确认是否需要强制更新
    /// </summary>
    /// <returns> false：需要强更； true：不需要强更 </returns>
    private static bool CheckAPKVersion()
    {
        string url = ServerInfoConfig.GetApkURL();
        if (string.IsNullOrEmpty(url))
        {
            return true;
        }

        url += "?sign=" + SDK.GetInstance().GetBundleID() + "&game=" + ServerInfoConfig.GetGame();
        string result = HttpUtil.Get(url);
        if (string.IsNullOrEmpty(result))
        {
            return true;
        }

        ApkVersionInfo versionInfo = JsonFx.Json.JsonReader.Deserialize<ApkVersionInfo>(result);
        if (null == versionInfo)
        {
            return true;
        }

        int toVersion = Util.ParseVersion(versionInfo.version);
        int curVersion = Util.ParseVersion(ServerInfoConfig.GetVersion());
        if (curVersion < toVersion)
        {
            UpdateNoticePanel.Show(versionInfo.notice, () =>
            {
                Application.OpenURL(versionInfo.url);
            });
            return false;
        }

        return true;
    }

    public static void Startup(ServerInfoType serverInfoType)
    {
        if (m_instance == null)
        {
            GameObject go = GameWorld.GetGameObject();
            m_instance = GameUtils.GetScript<GameWorld>(go);
        }

        ServerInfoConfig.Init(serverInfoType);

        if (GameConfig.showFPS)
        {
            m_instance.gameObject.AddComponent<FPS>();
        }

        m_instance.gameObject.AddComponent<Loom>();

        m_instance.StartGame();
    }

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /* ====================================================================================================================*/

    void Awake()
    {
        // 禁止场景切换的时候销毁
        DontDestroyOnLoad(gameObject);
        m_instance = this;
        InitListener();
    }

    void Start()
    {
        // 设置随机数种子
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

#if UNITY_EDITOR
        if (null == GameObject.Find("GMTools"))
        {
            new GameObject("GMTools").AddComponent<GMTools>();
        }
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LuaManager.instance.CallFunction("PlayerManager.Exit");
            return;
        }

        // 网络模块更新
        NetworkManager.instance.Update();
        // 消息模块更新
        MessageSystem.Instance().Update();
    }

    void InitListener()
    {
        MessageSystem.Instance().AddListener(MessageId.DECOMPRESS_CONFIG_PGOGRESS, this.OnDecompress);
        MessageSystem.Instance().AddListener(MessageId.DOWNLOAD_CONFIG_PROGRESS, this.OnDownload);
    }

    // <<1>>
    void StartGame()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (GameConfig.showLogConsole)
        {
            Consolation.LogConsole.Show();
            Consolation.LogConsole.instance.onGmCmd = GMTools.Send;
        }

        LoadingScene.Init(() =>
        {
            LoadingScene.instance.StartProgress(1, 0.99f, "正在加载版本文件");
            VersionHelper.instance.LoadVersion(OnComplateLoadVersion, OnLoadVersionError);
        });
    }

    IEnumerator LoadLua()
    {
        yield return new WaitForEndOfFrame();
        LuaManager.instance.InitLuaVM();
    }

    void OnComplateLoadVersion()
    {
        if (VersionHelper.instance.NeedUpdateAPK())
        {
            //更新apk处理
            Debug.LogError("走apk更新流程");
        }
        else
        {
            LoadingScene.instance.StartProgress(1, 0f, "正在检测游戏版本");

            if (CheckAPKVersion())
            {
                VersionHelper.instance.LoadConfig(OnComplateLoadConfig, OnLoadConfigError);
            }
        }
    }

    void OnLoadVersionError()
    {
        //没有加载到版本文件，检查网络环境或者其他处理
        Debug.LogError("没有加载到版本文件，检查网络环境或者其他处理");
    }

    void OnComplateLoadConfig()
    {
        LoadingScene.instance.StartProgress(1, 0.7f, "正在进入游戏...");
        StartCoroutine(Init());
    }

    void OnLoadConfigError()
    {
        Debug.LogError("下载版本文件失败");
    }

    void OnDecompress(System.Object obj)
    {
        float v = (float)obj;
        LoadingScene.instance.StartProgress(1, 0f, "正在解压更新包，解压不消耗流量！");
        if (v >= 1)
        {
            VersionHelper.instance.LoadConfig(OnComplateLoadConfig, OnLoadConfigError);
        }
    }

    void OnDownload(System.Object obj)
    {
        DownloadTips tips = (DownloadTips)obj;
        if (tips.progress == 0)
        {
            LoadingScene.instance.StartProgress(1, 0f, tips.tips);
        }
        else
        {
            LoadingScene.instance.SetEndValue(tips.progress, tips.tips + "  " + tips.speed);
        }
    }

    IEnumerator Init()
    {
        object obj = LuaManager.instance;
        obj = AssetBundleManager.instance;
        yield return null;

        Dictionary<string, AssetBundle> luaAssetBundles = new Dictionary<string, AssetBundle>();
        int loadNum = 0;
        List<string> luaBundles = VersionHelper.instance.GetAllLuaBundle();
        loadNum += luaBundles.Count;
        for (int i = 0; i < luaBundles.Count; i++)
        {
            string bundleName = luaBundles[i];
            AssetBundleManager.instance.GetBundleAsync(bundleName, (name, bundle) =>
            {
                if (luaAssetBundles.ContainsKey(name))
                {
                    luaAssetBundles[name] = bundle;
                }
                else
                {
                    luaAssetBundles.Add(name, bundle);
                }
                loadNum--;
            });
        }

        while (loadNum > 0)
        {
            yield return null;
        }

        int count = luaAssetBundles.Keys.Count;
        foreach(KeyValuePair<string, AssetBundle> keyVal in luaAssetBundles)
        {
            Dictionary<string, byte[]> dic = new Dictionary<string, byte[]>();
            AssetBundleRequest request = keyVal.Value.LoadAllAssetsAsync<TextAsset>();
            yield return request;
            for (int i = 0; i < request.allAssets.Length; i++)
            {
                TextAsset asset = (TextAsset)request.allAssets[i];
                dic.Add(asset.name, asset.bytes);
                Resources.UnloadAsset(asset);
            }
            keyVal.Value.Unload(false);
            AssetBundleManager.instance.luaBundleBytes.Add(keyVal.Key.Substring(0, keyVal.Key.IndexOf(".")), dic);
            count--;
        }

        while (count > 0)
        {
            yield return null;
        }

        luaAssetBundles.Clear();

        Loom.RunAsync(() => {
            LuaManager.instance.InitLuaVM();

            Loom.QueueOnMainThread(() => {
                // 初始化声音管理器
                AudioPlay2D.SetMute(AudioPlay2D.AudioType.BGM, false);
                AudioPlay2D.SetMute(AudioPlay2D.AudioType.Audio, false);

                M_ManagerInited();
            });
        });
    }

    private void M_ManagerInited()
    {
        LoadingScene.instance.SetEndValue(1f, null, () =>
        {
            Scene.onSceneLoaded += M_LoadingEnd;
            SceneHelper.instance.SetTransitionType(TransitionType.SMFadeTransition);
            SceneHelper.instance.LoadLevel(ResConfig.SCENE_ACCOUNT_LOGIN);
        });
    }

    private void M_LoadingEnd()
    {
        Scene.onSceneLoaded -= M_LoadingEnd;
        //Util.GetChildByUrl(GameObject.Find("/Canvas").gameObject, "Waiting").gameObject.SetActive(true);
        if (GameConfig.showLogConsole)
        {
            Consolation.LogConsole.Show();
            Consolation.LogConsole.instance.onGmCmd = GMTools.Send;
        }
        //ResHelper.Instance().StartSW();
        LuaManager.instance.InitStart();
    }

    public static bool isQuit = false;

    void OnApplicationQuit()
    {
        isQuit = true;
        JZLog.Log("Quit GameWorld");
        NetworkManager.instance.Close();
        ResHelper.Instance().StopDownloadHttp();
    }
}