using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


// 接口类
public interface IManager
{
    void Init();
    void Enable();
    void Disable();
    void Destroy();
    int GetNameHash();
    string GetName();

}

public class ManagerTemplate<T> : IManager where T : ManagerTemplate<T>, new()
{
    private int m_nameHash = 0;
    private string m_name = "";

    private static T m_instance = null;
    public static T Instance()
    {
        if (null == m_instance)
        {
            m_instance = new T();
            m_instance.SetName();
            GlobalManager.Instance().AddManager(m_instance);
        }
        return m_instance;
    }

    public void Init()
    {
        try
        {
            OnInit();
        }
        catch (Exception e)
        {
            JZLog.LogError(GetName() + " Init Fail, Message:" + e.Message + ",StackTrace:" + e.StackTrace);
        }
    }

    public void Enable()
    {
        try
        {
            OnEnable();
        }
        catch (Exception e)
        {
            JZLog.LogError(GetName() + " Enable Fail, Message:" + e.Message + ",StackTrace:" + e.StackTrace);
        }
    }

    public void Disable()
    {
        try
        {
            OnDisable();
        }
        catch (Exception e)
        {
            JZLog.LogError(GetName() + " Disable Fail, Message:" + e.Message + ",StackTrace:" + e.StackTrace);
        }
    }

    /// <summary>
    /// 注销管理者，注意：该方法仅供GlobalManager调用，其他地方想要触发Destroy，请使用GlobalManager.RemoveManager方法
    /// </summary>
    public void Destroy()
    {
        try
        {
            OnDisable();
            OnDestroy();
        }
        catch (Exception e)
        {
            JZLog.LogError("Message:" + e.Message + ",StackTrace:" + e.StackTrace + ",Source:" + e.Source);
        }
    }

    public int GetNameHash()
    {
        return m_nameHash;
    }

    public string GetName()
    {
        return m_name;
    }

    void SetName()
    {
        string name = typeof(T).ToString();
        m_nameHash = name.GetHashCode();
        m_name = name;
    }

    public virtual void OnInit()
    {

    }

    public virtual void OnEnable()
    {

    }

    public virtual void OnDisable()
    {

    }

    public virtual void OnDestroy()
    {

    }
}