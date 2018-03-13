using UnityEngine;
using System.Collections;
using LuaInterface;
using System.Collections.Generic;

public class LuaManager : SimleManagerTemplate<LuaManager>
{
    private LuaState lua;
    public LuaState Lua { get { return lua; } }
    private LuaLoader loader;
    private LuaLooper loop = null;

    // Use this for initialization
    protected override void OnInit() 
    {
        try
        {
            loader = new LuaLoader();
            lua = new LuaState();
            this.OpenLibs();
            lua.LuaSetTop(0);
        }
        catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }

        
    }

    public void InitLuaVM()
    {
        try
        {
            LuaBinder.Bind(lua);
            LuaCoroutine.Register(lua, GameWorld.instance);
            this.lua.Start();    //启动LUAVM
            lua.DoFile("Main.lua");
        }
        catch(System.Exception e)
        {
            Loom.QueueOnMainThread(() => {
                Debug.Log(e.Message + "\n" + e.StackTrace);
            });
        }
    }

    public void InitStart() 
    {
        this.StartMain();
        this.StartLooper();
    }

    void StartLooper() 
    {
        if (GameWorld.instance == null) return;
        loop = GameUtils.GetScript<LuaLooper>(GameWorld.instance.gameObject);
        loop.luaState = lua;
    }

    void StartMain() {
        LuaFunction main = lua.GetFunction("Main");
        main.Call();
        main.Dispose();
        main = null;
    }
        
    /// <summary>
    /// 初始化加载第三方库
    /// </summary>
    void OpenLibs() {
#if UNITY_EDITOR && !UNITY_IOS
        lua.OpenLibs(LuaDLL.luaopen_snapshot);
#endif
        //lua.OpenLibs(LuaDLL.luaopen_pb);
        //lua.OpenLibs(LuaDLL.luaopen_lpeg);
        //lua.OpenLibs(LuaDLL.luaopen_cjson);
        //lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
        //lua.OpenLibs(LuaDLL.luaopen_bit);
        //lua.OpenLibs(LuaDLL.luaopen_socket_core);
        lua.BeginPreLoad();
        lua.RegFunction("cjson", LuaDLL.luaopen_cjson); 
        lua.EndPreLoad();

        OpenLuaSocket();
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int LuaOpen_Socket_Core(System.IntPtr L)
    {
        return LuaDLL.luaopen_socket_core(L);
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int LuaOpen_Mime_Core(System.IntPtr L)
    {
        return LuaDLL.luaopen_mime_core(L);
    }

    protected void OpenLuaSocket()
    {
        lua.BeginPreLoad();
        lua.RegFunction("socket.core", LuaOpen_Socket_Core);
        lua.RegFunction("mime.core", LuaOpen_Mime_Core);     
        lua.EndPreLoad();
    }

    public object[] DoFile(string filename) {
        return lua.DoFile(filename);
    }

    // Update is called once per frame
    public object[] CallFunction(string funcName, params object[] args) {
        LuaFunction func = lua.GetFunction(funcName);
        if (func != null) {
            return func.Call(args);
        }
        return null;
    }

    public void CallFunctionNoGC(LuaFunction func, bool dispose, params object[] objs)
    {
        if (null == objs || objs.Length < 1)
        {
            func.Call();
        }
        else
        {
            func.BeginPCall();
            for (int i = 0; i < objs.Length; i++)
            {
                func.Push(objs[i]);
            }
            func.PCall();
            func.EndPCall();
        }
        
        if (dispose)
        {
            func.Dispose();
            func = null;
        }
    }

    public void CallFunctionNoGC(LuaFunction func, bool dispose, params UnityEngine.Object[] objs)
    {
        if (null == objs || objs.Length < 1)
        {
            func.Call();
        }
        else
        {
            func.BeginPCall();
            for (int i = 0; i < objs.Length; i++)
            {
                func.Push(objs[i]);
            }
            func.PCall();
            func.EndPCall();
        }

        if (dispose)
        {
            func.Dispose();
            func = null;
        }
    }

    public void LuaGC() {
        if (null != lua)
        {
            lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }
    }

    public void Close() 
    {
        if (null != loop)
        {
            loop.enabled = false;
            loop.Destroy();
            loop = null;
        }

        if (null != lua) 
        {
            lua.Dispose();
            lua = null;
        }
        
        loader = null;
    }
}
