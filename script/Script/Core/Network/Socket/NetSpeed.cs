using UnityEngine;
using System.Collections;

public class NetSpeed : MonoBehaviour {

    private static Ping ping;

    private bool isDone;

    private static int frameTimes;

    private static string ip;

    private int intervalFrame = 30;

    private static int times;

    private static int totalTime;

    private static int continuityOverTimeTimes;  //连续超时时间次数

    private static int lastSpeed;

    private int netType = JzwlNetworkInfo.TYPE_NONE;

    private JzwlNetworkInfo info = new JzwlNetworkInfo();

    // Use this for initialization
    void Start () {
	
	}

    public static void SetIp(string ip)
    {
        if (ping != null)
        {
            ping.DestroyPing();
        } 
        NetSpeed.ip = ip;
        times = 0;
        totalTime = 0;
        frameTimes = 0;
        continuityOverTimeTimes = 0;
        lastSpeed = 0;
        ping = new Ping(ip);
    }
	
	// Update is called once per frame
	void Update () {
	    if (ping == null && ip != null)
        {
            ping = new Ping(ip);
        } 
        if (frameTimes >= intervalFrame)
        {
            int curType = GetCurrentNetType();
            if (curType != netType)
            {
                netType = curType;
                NotifyGame(0);
            }
        }

        if (ping != null)
        { 
            if (isDone)
            {
                if (frameTimes >= intervalFrame)
                {
                    ping.DestroyPing();
                    isDone = false;
                    frameTimes = 0;
                    ping = new Ping(ip);
                } 
            } else
            { 
                isDone = ping.isDone;
                if (isDone)
                {
                    continuityOverTimeTimes = 0;
                    RecordTime(ping.time);
                    frameTimes = 0; 
                }
                //else
                //{
                //    if (frameTimes > intervalFrame)
                //    {
                //        frameTimes = 0;
                //        ping.DestroyPing();
                //        continuityOverTimeTimes++;
                //        if (continuityOverTimeTimes >= 5)
                //        {
                //            NotifyGame(1000); 
                //        }
                //        isDone = false;
                //        ping = new Ping(ip);
                //    }
                //}
            }
            frameTimes++;
        } 
	}


    private void RecordTime(int time)
    { 
        totalTime += time;
        times++;  

        if (times >= 5)
        {
            NotifyGame(0);
        } 
        else if (lastSpeed >= GameConfig.NetHintLimit && time < GameConfig.NetHintLimit)
        {
            NotifyGame(time);
        }
    }


    private void NotifyGame(int time)
    {
        if (times != 0)
        {
            if (time == 0)
            {
                lastSpeed = totalTime / times;
            } else
            {
                lastSpeed = time;
            } 
        }
        info.singnal = lastSpeed;
        info.type = GetCurrentNetType();
        LuaManager.instance.CallFunction("Network.PingTime", info);
        times = 0;
        totalTime = 0;
        continuityOverTimeTimes = 0;
        frameTimes = 0;
    }

    public static int GetCurrentNetType()
    {
        int type = JzwlNetworkInfo.TYPE_NONE;
        if (Application.internetReachability == NetworkReachability.NotReachable)
            type = JzwlNetworkInfo.TYPE_NONE;
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            type = JzwlNetworkInfo.TYPE_MOBILE;
        else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            type = JzwlNetworkInfo.TYPE_WIFI;
        return type;
    }

}
