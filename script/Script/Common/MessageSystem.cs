// ***************************************************************
//  FileName: MessageSystem.cs
//  Version : 1.0
//  Date    : 2016/6/27
//  Author  : cjzhanying 
//  Copyright (C) 2016 - Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: 消息系统
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

// 消息系统
public class MessageSystem
{
    public delegate void MsgCallback(System.Object msgArg);

    private enum OperateState
    {
        None = 0,
        Handle = 1,
        Remove = 2
    };

    private struct MsgHandler
    {
        public OperateState state;
        public MsgCallback callback;
        public int priority;
    };

    private class DelayMsgHandler
    {
        public float currentTime;
        public float delayTime;
        public int message;
        public object msgArg;
    }

    private class DelayCallback
    {
        public MsgCallback callback;
        public float delayTime;
        public object msgArg;
        public float currentTime;
    }

    private static MessageSystem _instance = null;
    private Dictionary<int, List<MsgHandler>> _handlerTable = new Dictionary<int, List<MsgHandler>>();
    private List<DelayMsgHandler> delayHandlerList = new List<DelayMsgHandler>();
    private List<DelayMsgHandler> delayRemoveList = new List<DelayMsgHandler>();
    private List<DelayCallback> delayCallbackList = new List<DelayCallback>();
    private List<DelayCallback> removeCallbackList = new List<DelayCallback>();

    /* 禁止外部实例化 */
    private MessageSystem()
    {

    }

    /*********************************
     * 函数说明: 获取消息系统实例
     * 返 回 值: void
     * 参数说明: void
     * 注意事项: 无
     *********************************/
    static public MessageSystem Instance()
    {
        if (_instance == null)
        {
            _instance = new MessageSystem();
        }
        return _instance;
    }

    /*********************************
     * 函数说明: 监听某消息
     * 返 回 值: void
     * 参数说明: message@消息名，handler@消息响应函数
     * 注意事项: 无
     *********************************/
    public void AddListener(int message, MsgCallback callback)
    {
        MsgHandler handler;
        handler.state = OperateState.None;
        handler.callback = callback;
        handler.priority = 0;

        List<MsgHandler> handlerList;
        if (!_handlerTable.TryGetValue(message, out handlerList))
        {
            handlerList = new List<MsgHandler>();
            handlerList.Add(handler);
            _handlerTable[message] = handlerList;
        }
        else
        {
            handlerList.Add(handler);
        }
    }

    /*********************************
     * 函数说明: 按优先级监听某消息
     * 返 回 值: void
     * 参数说明: message@消息名，handler@消息响应函数
     * 注意事项: 无
     *********************************/
    public void AddListener(int message, MsgCallback callback, int priority)
    {
        MsgHandler handler;
        handler.state = OperateState.None;
        handler.callback = callback;
        handler.priority = priority;

        List<MsgHandler> handlerList;
        if (!_handlerTable.TryGetValue(message, out handlerList))
        {
            handlerList = new List<MsgHandler>();
            handlerList.Add(handler);
            _handlerTable[message] = handlerList;
        }
        else
        {
            for (int i = 0; i < handlerList.Count; i++)
            {
                if (handlerList[i].priority < priority)
                {
                    handlerList.Insert(i, handler);
                    return;
                }
            }
            handlerList.Add(handler);
        }
    }

    /*********************************
     * 函数说明: 移除监听者
     * 返 回 值: void
     * 参数说明: message@消息ID，handler@消息响应函数
     * 注意事项: 无
     *********************************/
    public void RemoveListener(int message, MsgCallback callback)
    {
        List<MsgHandler> handlerList;
        if (_handlerTable.TryGetValue(message, out handlerList))
        {
            for (int i = 0; i < handlerList.Count; i++)
            {
                MsgHandler handler = handlerList[i];
                if (handler.callback == callback)
                {
                    if (handler.state == OperateState.Handle)
                    {
                        handler.state = OperateState.Remove;
                    }
                    else
                    {
                        handlerList.Remove(handler);
                    }
                    break;
                }
            }

            if (handlerList.Count == 0)
            {
                _handlerTable.Remove(message);
            }
        }
    }

    /*********************************
     * 函数说明: 移除某对象的所有监听者
     * 返 回 值: void
     * 参数说明: listenerHolder@监听消息的类实例
     * 注意事项: 无
     *********************************/
    public void RemoveListener(object listenerHolder)
    {
        if (listenerHolder == null)
        {
            return;
        }

        Dictionary<int, MsgCallback> removeTable = new Dictionary<int, MsgCallback>();
        foreach (KeyValuePair<int, List<MsgHandler>> pair in _handlerTable)
        {
            List<MsgHandler> handlerList = pair.Value;
            for (int i = 0; i < handlerList.Count; i++)
            {
                if (handlerList[i].callback.Target == listenerHolder)
                {
                    removeTable.Add(pair.Key, handlerList[i].callback);
                }
            }
        }

        foreach (KeyValuePair<int, MsgCallback> pair in removeTable)
        {
            RemoveListener(pair.Key, pair.Value);
        }
    }

    /*********************************
     * 函数说明: 发送消息
     * 返 回 值: void
     * 参数说明: message@消息ID，msgArg@消息附加参数
     * 注意事项: 无
     *********************************/
    public void SendMessage(int message, object msgArg)
    {
        List<MsgHandler> handlerList;
        if (_handlerTable.TryGetValue(message, out handlerList))
        {
            List<MsgHandler> removeList = null;
            for (int i = 0; i < handlerList.Count; i++)
            {
                MsgHandler handler = handlerList[i];
                handler.state = OperateState.Handle;
                try
                {
                    handler.callback(msgArg);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
                if (handler.state == OperateState.Remove)
                {
                    if (removeList == null)
                    {
                        removeList = new List<MsgHandler>();
                    }
                    removeList.Add(handler);
                }
                else
                {
                    handler.state = OperateState.None;
                }
            }
            if (removeList != null)
            {
                for (int i = 0; i < removeList.Count; i++)
                {
                    handlerList.Remove(removeList[i]);
                }
            }
            if (handlerList.Count == 0)
            {
                _handlerTable.Remove(message);
            }
        }
    }

    /*********************************
     * 函数说明: 邮寄消息（延迟发送消息）
     * 返 回 值: void
     * 参数说明: message@消息ID，msgArg@消息附加参数，delay@延时时间
     * 注意事项: 无
     *********************************/
    public void PostMessage(int message, object msgArg, float delay)
    {
        DelayMsgHandler delayMsg = new DelayMsgHandler();
        delayMsg.message = message;
        delayMsg.msgArg = msgArg;
        delayMsg.delayTime = delay;
        delayMsg.currentTime = 0;
        delayHandlerList.Add(delayMsg);
    }

    public void PostCallback(float delay, object msgArg, MsgCallback callback)
    {
        DelayCallback delayCallback = new DelayCallback();
        delayCallback.callback = callback;
        delayCallback.msgArg = msgArg;
        delayCallback.delayTime = delay;
        delayCallback.currentTime = 0;
        delayCallbackList.Add(delayCallback);
    }

    /* 更新延时消息 */
    public void Update()
    {
        // 延时发送消息
        for (int i = 0; i < delayHandlerList.Count; i++)
        {
            DelayMsgHandler msgObject = delayHandlerList[i];
            if (null == msgObject)
            {
                delayHandlerList.RemoveAt(i);
                i -= 1;
            }
            else
            {
                msgObject.currentTime += Time.deltaTime;
                if (msgObject.currentTime >= msgObject.delayTime)
                {
                    SendMessage(msgObject.message, msgObject.msgArg);
                    delayHandlerList.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        // 调度延时回调
        for (int i = 0; i < delayCallbackList.Count; i++)
        {
            DelayCallback delayCallback = delayCallbackList[i];
            delayCallback.currentTime += Time.deltaTime;
            if (delayCallback.currentTime >= delayCallback.delayTime)
            {
                delayCallback.callback(delayCallback.msgArg);
                delayCallbackList.RemoveAt(i);
                i -= 1;
            }
        }
    }
}