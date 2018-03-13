// ***************************************************************
//  FileName: DelayCallManager.cs
//  Version : 1.0
//  Date    : 2016/4/27
//  Author  : cjzhanying 
//  Copyright (C) 2016 - JZWL Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: 
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

public class DelayCallManager : MonoBehaviour
{
    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region 参数定义
    // 全局唯一实例
    private static DelayCallManager gInstance = null;

    // 回调函数类型定义
    public delegate void MsgCallback(System.Object msgArg);

    // 回调函数类型定义
    public delegate Repeat MsgCallbackRepeat(System.Object msgArg);

    // 操作状态
    private enum OperateState
    {
        None = 0,
        Handle = 1,
        Remove = 2
    };

    // 是否重复运行
    public enum Repeat
    {
        Continue,
        Exit,
    }

    // 消息句柄
    private class DelayMsgHandler
    {
        public OperateState state = OperateState.None;
        public MsgCallback callback = null;
        public MsgCallbackRepeat callbackRepeat = null;
        public object msgArg = null;
        public int delayframe = 1;
        public int maxframe = 1;

        // 是否检测Target为空
        private bool isNeedCheckTarget = false;
#if UNITY_EDITOR
        private string targetName = "";
#endif // UNITY_EDITOR

        #region 初始化数据
        /*********************************
         * 函数说明: 初始化数据
         * 返 回 值: void
         * 注意事项: 无
         *********************************/
        public void Initialization()
        {
            isNeedCheckTarget = false;
            if (callback != null)
            {
                if (callback.Target != null && callback.Target is MonoBehaviour)
                {
                    isNeedCheckTarget = true;
#if UNITY_EDITOR
                    targetName = callback.Target.ToString();
#endif // UNITY_EDITOR
                }
            }
            else if (callbackRepeat != null)
            {
                if (callbackRepeat.Target != null && callbackRepeat.Target is MonoBehaviour)
                {
                    isNeedCheckTarget = true;
#if UNITY_EDITOR
                    targetName = callbackRepeat.Target.ToString();
#endif // UNITY_EDITOR
                }
            }
        }
        #endregion

        #region 执行更新回调函数
        /*********************************
         * 函数说明: 执行更新回调函数
         * 返 回 值: void
         * 注意事项: 无
         *********************************/
        public void OnUpdate()
        {
            // 检查回调的状态(销毁状态的话不处理)
            if (state == OperateState.None)
            {
                // 更新延时帧数
                if (delayframe > 1)
                {
                    delayframe -= 1;
                    return;
                }
                // 更新回调
                state = OperateState.Handle;
                delayframe = maxframe;

                #region 检测回调函数的Target
                if (isNeedCheckTarget == true)
                {
                    if ((callback != null && callback.Target == null) || (callbackRepeat != null && callbackRepeat.Target == null))
                    {
                        state = OperateState.Remove;
#if UNITY_EDITOR
                        Debug.LogError("DelayCallManager : Your CallBack Target is Delete or Destroy , Check your script and Try to avoid CallBack Target [" + targetName + "] == null!!!");
#endif // UNITY_EDITOR
                        return;
                    }
                }
                #endregion

                #region 执行回调
                try
                {
                    if (callback != null)
                    {
                        callback(msgArg);
                        state = OperateState.Remove;
                    }
                    else if (callbackRepeat != null)
                    {
                        if (Repeat.Exit == callbackRepeat(msgArg))
                        {
                            state = OperateState.Remove;
                        }
                        else
                        {
                            // 只有为删除的函数才继续执行
                            if (state == OperateState.Handle)
                            {
                                state = OperateState.None;
                            }
                        }
                    }
                    else
                    {
                        state = OperateState.Remove;
                    }
                }
                catch (System.Exception ex)
                {
                    state = OperateState.Remove;
                    Debug.LogError("CallBack Exception : " + ex.ToString());
                }
                #endregion
            }
        }
        #endregion
    };

    // 记录回调函数    
    private List<DelayMsgHandler> delayHandlerList = new List<DelayMsgHandler>();
    #endregion

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
    public static DelayCallManager Instance()
    {
        if (gInstance == null)
        {
            if (GameWorld.instance == null) return null;

            GameObject goRoot = GameWorld.instance.gameObject;
            if (goRoot == null)
            {
                return null;
            }
            gInstance = goRoot.GetComponent<DelayCallManager>();
            if (gInstance == null)
            {
                gInstance = goRoot.AddComponent<DelayCallManager>();
            }
        }
        return gInstance;
    }
    #endregion

    #region 添加一个延时回调
    /*********************************
     * 函数说明: 添加一个延时回调
     * 返 回 值: void
     * 参数说明: callback 回调函数(只回调一次)
     * 参数说明: msgArg 回调函数的参数
     * 参数说明: delayFrame 延时帧数 最少一帧
     * 参数说明: isSameFunctionCallOnce 在上一个回调函数还没有执行的是时候忽略同一个回调函数
     * 参数说明: isSameFunctionChangeArg 如果一帧之内的同一个回调函数 那么我们是否更新参数
     * 参数说明: isResetDelay 如果上一个回调函数等待执行,是否重置延时回调
     * 注意事项: 
     *********************************/
    public void AddDelay(MsgCallback callback, object msgArg, int delayFrame = 1, bool isSameFunctionCallOnce = true, bool isSameFunctionChangeArg = false, bool isResetDelay = true)
    {
        AddDelay(callback, null, msgArg, delayFrame, isSameFunctionCallOnce, isSameFunctionChangeArg, isResetDelay);
    }

    /*********************************
     * 函数说明: 添加一个延时回调
     * 返 回 值: void
     * 参数说明: callbackRepeat 回调函数(根据返回值确认是否重复回调 返回Repeat.Destroy 销毁 返回Repeat.Continue 继续间隔多少帧执行)
     * 参数说明: msgArg 回调函数的参数
     * 参数说明: delayFrame 延时帧数 最少一帧
     * 参数说明: isSameFunctionCallOnce 在上一个回调函数还没有执行的是时候忽略同一个回调函数
     * 参数说明: isSameFunctionChangeArg 如果一帧之内的同一个回调函数 那么我们是否更新参数
     * 参数说明: isResetDelay 如果上一个回调函数等待执行,是否重置延时回调
     * 注意事项: 无
     *********************************/
    public void AddDelay(MsgCallbackRepeat callbackRepeat, object msgArg, int delayFrame = 1, bool isSameFunctionCallOnce = true, bool isSameFunctionChangeArg = false, bool isResetDelay = true)
    {
        AddDelay(null, callbackRepeat, msgArg, delayFrame, isSameFunctionCallOnce, isSameFunctionChangeArg, isResetDelay);
    }
    #endregion

    #region 删除回调
    /*********************************
     * 函数说明: 删除回调
     * 返 回 值: void
     * 参数说明: callback
     * 注意事项: 无
     *********************************/
    public void RemoveDelay(MsgCallback callback)
    {
        RemoveDelay(callback, null);
    }

    /*********************************
     * 函数说明: 删除回调
     * 返 回 值: void
     * 参数说明: callbackRepeat
     * 注意事项: 无
     *********************************/
    public void RemoveDelay(MsgCallbackRepeat callbackRepeat)
    {
        RemoveDelay(null, callbackRepeat);
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
        gInstance = this;
    }
    #endregion

    #region 脚本更新
    /************************************
     * 函数说明: 脚本更新
     * 返 回 值: void
     * 注意事项: 
    ************************************/
    void Update()
    {
        // 这里记录下链表的长度 确保我们只处理当前的回调 新添加的不处理
        int iCount = delayHandlerList.Count;
        for (int i = 0; i < iCount; i++)
        {
            delayHandlerList[i].OnUpdate();
        }
    }

    /*********************************
     * 函数说明: 脚本更新
     * 返 回 值: void
     * 注意事项: 无
     *********************************/
    void LateUpdate()
    {
        // 定义一个延时回调句柄
        int iCount = delayHandlerList.Count;
        for (int i = iCount - 1; i >= 0; i--)
        {
            // 检查回调的状态(销毁状态的话删除)
            if (delayHandlerList[i].state == OperateState.Remove)
            {
                delayHandlerList.RemoveAt(i);
            }
        }

        // 检查是否还有回调函数 如果没有脚本制空
        if (delayHandlerList.Count == 0)
        {
            this.enabled = false;
        }
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
        gInstance = null;
    }
    #endregion

    #region 添加一个延时回调
    /*********************************
     * 函数说明: 添加一个延时回调
     * 返 回 值: void
     * 参数说明: callback 回调函数
     * 参数说明: callbackRepeat 回调函数
     * 参数说明: msgArg 回调函数的参数
     * 参数说明: delayFrame 延时帧数 最少一帧
     * 参数说明: isSameFunctionCallOnce 在上一个回调函数还没有执行的是时候忽略同一个回调函数
     * 参数说明: isSameFunctionChangeArg 如果一帧之内的同一个回调函数 那么我们是否更新参数
     * 参数说明: isResetDelay 如果上一个回调函数等待执行,是否重置延时回调
     * 注意事项: 无
     *********************************/
    void AddDelay(MsgCallback callback, MsgCallbackRepeat callbackRepeat, object msgArg, int delayFrame = 1, bool isSameFunctionCallOnce = true, bool isSameFunctionChangeArg = false, bool isReSetDelay = true)
    {
        // 定义一个延时回调句柄
        DelayMsgHandler delayMsgHandler = default(DelayMsgHandler);

        // 检查参数
        if (callback == null && callbackRepeat == null)
        {
            Debug.LogError("You can not add a null Delay Callback , check your args!");
            return;
        }

        // 检查是否已经添加过
        if (isSameFunctionCallOnce == true)
        {
            int iCount = delayHandlerList.Count;
            for (int i = 0; i < iCount; i++)
            {
                delayMsgHandler = delayHandlerList[i];
                // 销毁状态的话不处理
                if (delayMsgHandler.state == OperateState.Remove)
                {
                    continue;
                }
                // 检查回调的状态
                if ((callback != null && delayMsgHandler.callback == callback) || (callbackRepeat != null && delayMsgHandler.callbackRepeat == callbackRepeat))
                {
                    // 移除上一条消息 而不是直接更新消息是因为 防止在Update的时候添加消息,造成同一帧执行
                    delayMsgHandler.state = OperateState.Remove;

                    // 更新消息参数
                    msgArg = isSameFunctionChangeArg == true ? msgArg : delayMsgHandler.msgArg;

                    // 是否重置延时
                    delayFrame = isReSetDelay == true ? delayFrame : delayMsgHandler.delayframe;
                    break;
                }
            }
        }

        // 添加一条新的消息到结尾
        delayMsgHandler = new DelayMsgHandler();
        delayMsgHandler.callback = callback;
        delayMsgHandler.callbackRepeat = callbackRepeat;
        delayMsgHandler.msgArg = msgArg;
        delayMsgHandler.state = OperateState.None;
        delayMsgHandler.delayframe = Mathf.Max(1, delayFrame);
        delayMsgHandler.maxframe = delayMsgHandler.delayframe;
        delayMsgHandler.Initialization();
        delayHandlerList.Add(delayMsgHandler);
        // 启用脚本
        this.enabled = true;
    }
    #endregion

    #region 删除回调
    /*********************************
     * 函数说明: 删除回调
     * 返 回 值: void
     * 参数说明: callback
     * 参数说明: callbackRepeat
     * 注意事项: 无
     *********************************/
    void RemoveDelay(MsgCallback callback, MsgCallbackRepeat callbackRepeat)
    {
        // 定义一个延时回调句柄
        DelayMsgHandler delayMsgHandler = default(DelayMsgHandler);

        // 检查参数
        if (callback == null && callbackRepeat == null)
        {
            return;
        }

        // 检查是否已经添加过
        int iCount = delayHandlerList.Count;
        for (int i = 0; i < iCount; i++)
        {
            delayMsgHandler = delayHandlerList[i];
            // 销毁状态的话不处理
            if (delayMsgHandler.state == OperateState.Remove)
            {
                continue;
            }
            // 检查回调的状态
            if ((callback != null && delayMsgHandler.callback == callback) || (callbackRepeat != null && delayMsgHandler.callbackRepeat == callbackRepeat))
            {
                // 移除上一条消息 而不是直接更新消息是因为 防止在Update的时候添加消息,造成同一帧执行
                delayMsgHandler.state = OperateState.Remove;
                continue;
            }
        }

        // 启用脚本
        this.enabled = true;
    }
    #endregion

    #region 游戏退出
    /*********************************
     * 函数说明: 游戏退出
     * 返 回 值: void
     * 注意事项: 无
     *********************************/
    void OnApplicationQuit()
    {
        delayHandlerList.Clear();
    }
    #endregion
}