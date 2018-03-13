using UnityEngine;
using System;

public class AppConst {
    /// <summary>
    /// 如果开启更新模式，前提必须启动框架自带服务器端。
    /// 否则就需要自己将StreamingAssets里面的所有内容
    /// 复制到自己的Webserver上面，并修改下面的WebUrl。
    /// </summary>
    public const bool LuaByteMode = false;                     //Lua字节码模式-默认关闭 

    public const int TimerInterval = 1;

    public const string AppName = "LuaFramework";               //应用程序名称
    public const string LuaTempDir = "lua/";                    //临时目录
    public const string ExtName = ".ab";                   //资源扩展名
    public const string AppPrefix = AppName + "_";              //应用程序前缀
    public const string WebUrl = "http://localhost:6688/";      //测试更新地址

    public static string FrameworkRoot = Application.dataPath + "/LuaFramework";
}
