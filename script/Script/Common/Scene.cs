using UnityEngine;
using System;
using LuaInterface;
using System.Collections.Generic;
public class Scene
{
    public static Action onSceneLoaded;
    public static GameObject UIRoot { get { return m_UIRoot; } }
    private static GameObject m_UIRoot = null;

    public static void SetSceneTransitionEnd()
    {
        M_Init();
        if (null != onSceneLoaded)
        {
            onSceneLoaded();
            onSceneLoaded = null;
        }
    }

    public static void OnSceneLoaded(LuaFunction func)
    {
        if (null != func)
        {
            onSceneLoaded = () => { func.Call(); };
        }
    }

    private static void M_Init()
    {
        m_UIRoot = GameObject.Find("Canvas");
    }
}
