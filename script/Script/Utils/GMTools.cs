using LuaInterface;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
public class GMTools : MonoBehaviour
{
    public List<string> GMs = new List<string>()
    {
        "!all_open",
        "!xp_full 80",
        "!kill_all",
        "!laohuji 0",
        "!nb",
        "!refresh",
        "!war 3",
        "!fly 1010001",
        "!ts 2016 07 16 23 55 00",
        "!now"
    };

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private LuaState lua;

    public static void Send(string gm)
    {
        NetJZWL.PackageOut packageOut = NetworkManager.instance.NewPackagetOut(MessageId.GM_CMD);
        packageOut.PushString(gm);
        NetworkManager.instance.Send(packageOut);
    }

    public void RefreshPlayerBodyShow(string resId)
    {
        //lua.DoFile("Main.lua");
        lua = LuaManager.instance.Lua;
        LuaFunction main = lua.GetFunction("GM");
        main.Call(2000000, resId);
        main.Dispose();
        main = null;
    }

    public void RefreshPlayerWingShow(string resId)
    {
        //lua.DoFile("Main.lua");
        lua = LuaManager.instance.Lua;
        LuaFunction main = lua.GetFunction("GM");
        main.Call(2000001, resId);
        main.Dispose();
        main = null;
    }

    public void RefreshPlayerWeaponShow(string resId)
    {
        //lua.DoFile("Main.lua");
        lua = LuaManager.instance.Lua;
        LuaFunction main = lua.GetFunction("GM");
        main.Call(2000002, resId);
        main.Dispose();
        main = null;
    }

    public void RefreshPlayerBodySize(int size)
    {
        lua = LuaManager.instance.Lua;
        LuaFunction main = lua.GetFunction("GM");
        main.Call(20000101, size);
        main.Dispose();
        main = null;
    }

    public void FindPath(int from, int to)
    {
        lua = LuaManager.instance.Lua;
        LuaFunction main = lua.GetFunction("FindPath");
        main.Call(from, to);
        main.Dispose();
        main = null;
    }

    public void ShowRes()
    {
        string infoString = "";
        int count = 1;
        Dictionary<string, List<string>> datas = AssetBundleManager.instance.GetAllLoadedResInfo();
        foreach (string k in datas.Keys)
        {
            List<string> data = datas[k];
            infoString += "[ " + count + " ] =======>> ab: " + k + ", ref: " + data[0] + "\n\n";
            data.RemoveAt(0);
            infoString += "     ";
            data.ForEach(it =>
            {
                infoString += it + ", ";
            });
            infoString += "\n\n";
            count++;
        }
        Debug.LogWarning("================================================== \n" + infoString);
        LogToFile(infoString);
    }

    string GetTimeString(DateTime time)
    {
        string month = string.Format("{0:D2}", time.Month);
        string day = string.Format("{0:D2}", time.Day);
        string hour = string.Format("{0:D2}", time.Hour);
        string minute = string.Format("{0:D2}", time.Minute);
        return month + day + hour + minute;
    }

    void LogToFile(string str)
    {
        string file = "F://log/" + GetTimeString(System.DateTime.Now);
        if (System.IO.File.Exists(file))
        {
            System.IO.File.Delete(file);
        }

        StreamWriter sw;
        FileInfo fInfo = new FileInfo(file);
        if (!fInfo.Exists)
        {
            sw = fInfo.CreateText();
        }
        else
        {
            sw = fInfo.CreateText();
            sw.Write("=============================================\n");
        }
        sw.Write(str);
        sw.Close();
        sw.Dispose();
    }

    public void ShowMemory()
    {
        LuaManager.instance.DoFile("Main.lua");
        LuaManager.instance.CallFunction("Snapshot");
    }

    public void TestGC()
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        LuaManager.instance.LuaGC();
    }
}
