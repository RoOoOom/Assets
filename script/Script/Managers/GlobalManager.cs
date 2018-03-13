// ***************************************************************
//  FileName: GlobalManager.cs
//  Version : 1.0
//  Date    : 2016/6/27
//  Author  : cjzhanying 
//  Copyright (C) 2016 - Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: 全局Manager管理器 负责统一的初始化 重置数据等方法调用
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

public class GlobalManager : MonoBehaviour
{
    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /* ====================================================================================================================*/

    // 全局唯一实例
    private static GlobalManager m_instance = null;

    // 管理器缓存
    private Dictionary<int, IManager> m_managers = new Dictionary<int, IManager>();

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region 全局实例
    /************************************
     * 函数说明: 全局实例
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    public static GlobalManager Instance()
    {
        if (m_instance == null)
        {
            if (GameWorld.instance == null) return null;
            GameObject go = GameWorld.instance.gameObject;
            m_instance = GameUtils.GetScript<GlobalManager>(go);
        }
        return m_instance;
    }
    #endregion

    #region 初始化全局数据
    /*********************************
     * 函数说明: 初始化全局数据
     * 返 回 值: void
     * 注意事项: 无
     *********************************/
    public void Init(Action complate)
    {
        if (m_instance == null)
        {
            return;
        }
        List<IManager> listInterface = new List<IManager>();
        listInterface.AddRange(m_instance.m_managers.Values);
        DelayCallManager.Instance().AddDelay((msgArg) =>
        {
            List<IManager> list = (List<IManager>)msgArg;
            if (list == null || list.Count == 0)
            {
                if (null != complate) complate();
                return DelayCallManager.Repeat.Exit;
            }
            list[0].Init();
            list.RemoveAt(0);
            return DelayCallManager.Repeat.Continue;
        }, listInterface);
     }
    #endregion

    #region 启用管理器数据
    /*********************************
     * 函数说明: 启用管理器数据
     * 返 回 值: void
     * 注意事项: 无
     *********************************/
    public static void EnableAll()
    {
        if (m_instance == null)
        {
            return;
        }
        foreach (KeyValuePair<int, IManager> pair in m_instance.m_managers)
        {
            pair.Value.Enable();
        }
    }
    #endregion

    #region 注销管理器数据
    /*********************************
     * 函数说明: 注销管理器数据
     * 返 回 值: void
     * 注意事项: 无
     *********************************/
    public static void DisableAll()
    {
        if (m_instance == null)
        {
            return;
        }
        foreach (KeyValuePair<int, IManager> pair in m_instance.m_managers)
        {
            pair.Value.Disable();
        }
    }
    #endregion

    #region 注销管理器数据
    /*********************************
     * 函数说明: 注销管理器数据
     * 返 回 值: void
     * 注意事项: 无
     *********************************/
    public static void DestroyAll()
    {
        if (m_instance == null)
        {
            return;
        }

        foreach (KeyValuePair<int, IManager> pair in m_instance.m_managers)
        {
            pair.Value.Destroy();
        }

        m_instance.m_managers.Clear();
    }
    #endregion

    #region 重置管理器数据
    /*********************************
     * 函数说明: 重置管理器数据
     * 返 回 值: void
     * 注意事项: 无
     *********************************/
    public static void ReSetAll()
    {
        if (m_instance == null)
        {
            return;
        }
        DisableAll();
    }
    #endregion

    #region 添加一个管理器
    /*********************************
     * 函数说明: 添加一个管理器
     * 返 回 值: void
     * 参数说明: manager
     * 注意事项: 无
     *********************************/
    public void AddManager(IManager manager)
    {
        if (manager == null)
        {
            JZLog.LogError("You can not add a null manager");
            return;
        }
        // 防止重复添加
        if (m_managers.ContainsKey(manager.GetNameHash()) == true)
        {
            JZLog.LogError("You can not add mulit manager : " + manager.GetName());
            return;
        }
        m_managers.Add(manager.GetNameHash(), manager);
    }
    #endregion

    #region 移除一个管理器
    /*********************************
     * 函数说明: 移除一个管理器
     * 返 回 值: void
     * 参数说明: manager
     * 注意事项: 无
     *********************************/
    public void RemoveManager(IManager manager)
    {
        if (manager == null)
        {
            JZLog.LogError("You can not remove a null " + name);
            return;
        }
        if (m_managers.ContainsKey(manager.GetNameHash()) == false)
        {
            JZLog.LogError("You have not add manager " + manager.GetName());
            return;
        }
        if (manager != m_managers[manager.GetNameHash()])
        {
            JZLog.LogError(manager.GetName() + " is not match the exist manager!!");
            return;
        }
        manager.Destroy();
        m_managers.Remove(manager.GetNameHash());
        manager = null;
    }
    #endregion

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region 脚本被初始化
    /************************************
     * 函数说明: 脚本被初始化
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    void Awake()
    {
        m_instance = this;
    }
    #endregion

    #region 脚本销毁
    /************************************
     * 函数说明: 脚本销毁
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    void OnDestroy()
    {
        DestroyAll();
        m_instance = null;
    }
    #endregion

    /*********************************
     * 函数说明: 初始化函数
     * 返 回 值: void
     * 注意事项: 这里初始化各个模块Manager的实例 调用各个Manager.Instance();
     *********************************/
    public void Initialization()
    {
    }
}