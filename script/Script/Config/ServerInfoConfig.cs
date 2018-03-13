using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum ServerInfoType
{
    inner,
    sout,
    sout_test,
    android,
    android_test, 
	ios,
}

public class ServerInfoConfig
{
    private const string KEY_GAME_NAME = "gameName";
    private const string KEY_GAME = "game";
    private const string KEY_IP = "account";
    private const string KEY_PORT = "port";
    private const string KEY_VERSION = "version";
    private const string KEY_RES_VERSION = "resVersion";
    private const string KEY_AREA_IDS = "areaIds";
    private const string KEY_PLATFORM = "platform";
    private const string KEY_AGENT_ID = "AgentID";
    private const string KEY_RES_URL = "resUrl";
    private const string KEY_RES_ZIP_URL = "resZipUrl";
    private const string KEY_APK_URL = "apkUrl";
    private const string KEY_VOICE_URL = "voiceUrl";
    private const string KEY_PAY_URL = "payUrl";
    private const string KEY_NOTIFY_URL = "nolifyInfoUrl";
    private const string KEY_LEAVL_URL = "leaveMessageUrl";
    private const string KEY_STATISTICSURL = "statisticsUrl";
    private const string KEY_IP_ALLOW_URL = "ipAllowUrl";
    private const string KEY_ERROR_URL = "errorUrl";
    private const string KEY_CHECK_URL = "checkUrl";
    private const string KEY_SDK_CLASS = "sdkClass";
    private const string KEY_UNITY_EXIT = "exit";

    private static Dictionary<string, string> _infos = new Dictionary<string,string>();
    private static ServerInfoType curType;

    public static void Init(ServerInfoType sit)
    {
        curType = sit;
        switch (sit)
        {
			case ServerInfoType.ios:
            case ServerInfoType.android:
			case ServerInfoType.android_test:
                SDK.GetInstance().Init();
                break;
            case ServerInfoType.inner:
                _InitInner();
                break;
            case ServerInfoType.sout:
                _InitSout();
                break;
            case ServerInfoType.sout_test:
                _InitSoutTest();
                break;
            default:
                break;
        }
    }

    public static ServerInfoType GetServerInfoType()
    {
        return curType;
    }

    private static readonly Regex httpRegex = new Regex(@"http://(.+?)/web/order");
    private static readonly Regex httpsRegex = new Regex(@"https://(.+?)/web/order");

    private static void _AddInfo(string key, string value)
    {
        string val = value;
        if (KEY_PAY_URL.Equals(key))
        {
            Regex regex = value.StartsWith("https") ? httpsRegex : httpRegex;
            foreach (Match match in regex.Matches(value))
            {
                var group = match.Groups[1];
                string dns = group.Value;
                string ip = HttpUtil.GetIpByDNS(dns);
                val = value.Replace(dns, ip);
               // UnityEngine.Debug.Log(val);
            }
        }

        _infos.Add(key, val);
    }

    private static string _GetInfo(string key, string defaultV = "")
    {
        if(_infos.ContainsKey(key))
        {
            return _infos[key];
        }

        return defaultV;
    }

    public static string GetGame()
    {
        return _GetInfo(KEY_GAME);
    }

    public static string GetIP()
    {
        return _GetInfo(KEY_IP);
    }

    public static int GetPort()
    {
        return int.Parse(_GetInfo(KEY_PORT, "0"));
    }

    public static string GetAreaIDs()
    {
        return _GetInfo(KEY_AREA_IDS);
    }

    public static string GetVersion()
    {
        return _GetInfo(KEY_VERSION);
    }

    public static string GetResVersion()
    {
        return _GetInfo(KEY_RES_VERSION);
    }

    public static string GetPlatform()
    {
        return _GetInfo(KEY_PLATFORM);
    }

    public static string GetAgentID()
    {
        return _GetInfo(KEY_AGENT_ID);
    }

    public static string GetResURL()
    {
        return _GetInfo(KEY_RES_URL);
    }

    public static string GetResZipURL()
    {
        string ret = _GetInfo(KEY_RES_ZIP_URL);
        if (string.IsNullOrEmpty(ret))
        {
            ret = _GetInfo(KEY_RES_URL);
        }
        return ret;
    }

    public static string GetErrorURL()
    {
        return _GetInfo(KEY_ERROR_URL);
    }

    public static string GetGameName()
    {
        return _GetInfo(KEY_GAME_NAME);
    }

    public static string GetSdkClass()
    {
        return _GetInfo(KEY_SDK_CLASS);
    }

    public static string GetPayUrl()
    {
        return _GetInfo(KEY_PAY_URL);
    }

    public static int GetUnityExit()
    {
        return int.Parse(_GetInfo(KEY_UNITY_EXIT, "1"));
    }

    public static string GetApkURL()
    {
        return _GetInfo(KEY_APK_URL);
    }

    public static void AddConfig(SDKConfig config)
    { 
        if (config == null)
        {
            return;
        }
        _AddInfo(KEY_GAME, config.game);
        _AddInfo(KEY_IP, config.ip);
        _AddInfo(KEY_PORT, config.port);
        _AddInfo(KEY_AREA_IDS, config.serverAreaIds);
        _AddInfo(KEY_PLATFORM, config.platform);
        _AddInfo(KEY_AGENT_ID, config.agent);
        _AddInfo(KEY_RES_URL, config.resUrl);
        _AddInfo(KEY_VOICE_URL, config.voiceUrl);
        _AddInfo(KEY_ERROR_URL, config.errorReportUrl);
        _AddInfo(KEY_IP_ALLOW_URL, config.ipAllowUrl);
        _AddInfo(KEY_PAY_URL, config.payUrl);
        _AddInfo(KEY_STATISTICSURL, config.statisticsUrl);
        _AddInfo(KEY_SDK_CLASS, config.sdkClass);
        _AddInfo(KEY_UNITY_EXIT, config.exit);
        _AddInfo(KEY_APK_URL, config.apkUrl);
        _AddInfo(KEY_RES_ZIP_URL, config.resZipUrl);
        _AddInfo(KEY_VERSION, config.version);
    } 

    private static void _InitInner()
    {
        _AddInfo(KEY_GAME_NAME, "王者内网");
        _AddInfo(KEY_GAME, "legend");
        _AddInfo(KEY_IP, "192.168.1.220");
        _AddInfo(KEY_PORT, "9601");
        _AddInfo(KEY_AREA_IDS, "0,1,2");
        _AddInfo(KEY_VERSION, "1.0.0");
        _AddInfo(KEY_PLATFORM, "jzwl");
        _AddInfo(KEY_RES_URL, "http://192.168.1.220/res/hz_map_res/");
        _AddInfo(KEY_RES_ZIP_URL, "http://192.168.1.220/res/hz_2.2.0/");
        _AddInfo(KEY_PAY_URL, "http://192.168.1.220/web/order");
    }

    private static void _InitSout()
    {
        _AddInfo(KEY_GAME_NAME, "王者外网");
        _AddInfo(KEY_GAME, "hzml");
        _AddInfo(KEY_IP, "hzmllogin.11dragon.com");
        _AddInfo(KEY_PORT, "7352");
        _AddInfo(KEY_AREA_IDS, "0");
        _AddInfo(KEY_VERSION, "2.2.0");
        _AddInfo(KEY_PLATFORM, "jzwl");
        _AddInfo(KEY_RES_URL, "http://hzcdn.11dragon.com/res/hz_res_2.2.0/");
        _AddInfo(KEY_RES_ZIP_URL, "http://hzcdn.11dragon.com/res/hz_2.2.0/");
        _AddInfo(KEY_ERROR_URL, "http://interface.11dragon.com/ms_web/game/client/error");
        _AddInfo(KEY_PAY_URL, "http://pay.11dragon.com/web/order");
        _AddInfo(KEY_APK_URL, "http://ms.11dragon.com/ms_web/updateCilent/info");
        _AddInfo(KEY_SDK_CLASS, "com.sz.jzwl.yijie.YiJieSdk");
    }

    private static void _InitSoutTest()
    {
        _AddInfo(KEY_GAME, "cqkj");
        _AddInfo(KEY_IP, "113.107.150.51");
        _AddInfo(KEY_PORT, "7321");
        _AddInfo(KEY_AREA_IDS, "0");
        _AddInfo(KEY_VERSION, "1.0.0");
        _AddInfo(KEY_PLATFORM, "test");
        _AddInfo(KEY_RES_URL, "http://hzcdn.11dragon.com/res/hz_res_2.2.0/");
        _AddInfo(KEY_RES_ZIP_URL, "http://113.107.150.51/res/cq_1.0.0/");
        _AddInfo(KEY_ERROR_URL, "http://113.107.150.51/ms_web/game/client/error");
        _AddInfo(KEY_APK_URL, "http://113.107.150.51/ms_web/updateCilent/info");
        _AddInfo(KEY_IP_ALLOW_URL, "http://113.107.150.51/ms_web/whiteController/ipTrue");
    }
}
