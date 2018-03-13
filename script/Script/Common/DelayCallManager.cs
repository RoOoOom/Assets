// ***************************************************************
//  FileName: DelayCallManager.cs
//  Version : 1.0
//  Date    : 2016/4/27
//  Author  : cjzhanying 
//  Copyright (C) 2016 - JZWL Digital Technology Co.,Ltd. All rights reserved.	��Ȩ����
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
    /** @brief: -------------------------------------------��������------------------------------------------------------- */
    /** @brief: -------------------------------------------��������------------------------------------------------------- */
    /** @brief: -------------------------------------------��������------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region ��������
    // ȫ��Ψһʵ��
    private static DelayCallManager gInstance = null;

    // �ص��������Ͷ���
    public delegate void MsgCallback(System.Object msgArg);

    // �ص��������Ͷ���
    public delegate Repeat MsgCallbackRepeat(System.Object msgArg);

    // ����״̬
    private enum OperateState
    {
        None = 0,
        Handle = 1,
        Remove = 2
    };

    // �Ƿ��ظ�����
    public enum Repeat
    {
        Continue,
        Exit,
    }

    // ��Ϣ���
    private class DelayMsgHandler
    {
        public OperateState state = OperateState.None;
        public MsgCallback callback = null;
        public MsgCallbackRepeat callbackRepeat = null;
        public object msgArg = null;
        public int delayframe = 1;
        public int maxframe = 1;

        // �Ƿ���TargetΪ��
        private bool isNeedCheckTarget = false;
#if UNITY_EDITOR
        private string targetName = "";
#endif // UNITY_EDITOR

        #region ��ʼ������
        /*********************************
         * ����˵��: ��ʼ������
         * �� �� ֵ: void
         * ע������: ��
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

        #region ִ�и��»ص�����
        /*********************************
         * ����˵��: ִ�и��»ص�����
         * �� �� ֵ: void
         * ע������: ��
         *********************************/
        public void OnUpdate()
        {
            // ���ص���״̬(����״̬�Ļ�������)
            if (state == OperateState.None)
            {
                // ������ʱ֡��
                if (delayframe > 1)
                {
                    delayframe -= 1;
                    return;
                }
                // ���»ص�
                state = OperateState.Handle;
                delayframe = maxframe;

                #region ���ص�������Target
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

                #region ִ�лص�
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
                            // ֻ��Ϊɾ���ĺ����ż���ִ��
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

    // ��¼�ص�����    
    private List<DelayMsgHandler> delayHandlerList = new List<DelayMsgHandler>();
    #endregion

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------�ⲿ����------------------------------------------------------- */
    /** @brief: -------------------------------------------�ⲿ����------------------------------------------------------- */
    /** @brief: -------------------------------------------�ⲿ����------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region ȫ��ʵ��
    /************************************
     * ����˵��: ȫ��ʵ��
     * �� �� ֵ: void
     * ע������: 
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

    #region ���һ����ʱ�ص�
    /*********************************
     * ����˵��: ���һ����ʱ�ص�
     * �� �� ֵ: void
     * ����˵��: callback �ص�����(ֻ�ص�һ��)
     * ����˵��: msgArg �ص������Ĳ���
     * ����˵��: delayFrame ��ʱ֡�� ����һ֡
     * ����˵��: isSameFunctionCallOnce ����һ���ص�������û��ִ�е���ʱ�����ͬһ���ص�����
     * ����˵��: isSameFunctionChangeArg ���һ֮֡�ڵ�ͬһ���ص����� ��ô�����Ƿ���²���
     * ����˵��: isResetDelay �����һ���ص������ȴ�ִ��,�Ƿ�������ʱ�ص�
     * ע������: 
     *********************************/
    public void AddDelay(MsgCallback callback, object msgArg, int delayFrame = 1, bool isSameFunctionCallOnce = true, bool isSameFunctionChangeArg = false, bool isResetDelay = true)
    {
        AddDelay(callback, null, msgArg, delayFrame, isSameFunctionCallOnce, isSameFunctionChangeArg, isResetDelay);
    }

    /*********************************
     * ����˵��: ���һ����ʱ�ص�
     * �� �� ֵ: void
     * ����˵��: callbackRepeat �ص�����(���ݷ���ֵȷ���Ƿ��ظ��ص� ����Repeat.Destroy ���� ����Repeat.Continue �����������ִ֡��)
     * ����˵��: msgArg �ص������Ĳ���
     * ����˵��: delayFrame ��ʱ֡�� ����һ֡
     * ����˵��: isSameFunctionCallOnce ����һ���ص�������û��ִ�е���ʱ�����ͬһ���ص�����
     * ����˵��: isSameFunctionChangeArg ���һ֮֡�ڵ�ͬһ���ص����� ��ô�����Ƿ���²���
     * ����˵��: isResetDelay �����һ���ص������ȴ�ִ��,�Ƿ�������ʱ�ص�
     * ע������: ��
     *********************************/
    public void AddDelay(MsgCallbackRepeat callbackRepeat, object msgArg, int delayFrame = 1, bool isSameFunctionCallOnce = true, bool isSameFunctionChangeArg = false, bool isResetDelay = true)
    {
        AddDelay(null, callbackRepeat, msgArg, delayFrame, isSameFunctionCallOnce, isSameFunctionChangeArg, isResetDelay);
    }
    #endregion

    #region ɾ���ص�
    /*********************************
     * ����˵��: ɾ���ص�
     * �� �� ֵ: void
     * ����˵��: callback
     * ע������: ��
     *********************************/
    public void RemoveDelay(MsgCallback callback)
    {
        RemoveDelay(callback, null);
    }

    /*********************************
     * ����˵��: ɾ���ص�
     * �� �� ֵ: void
     * ����˵��: callbackRepeat
     * ע������: ��
     *********************************/
    public void RemoveDelay(MsgCallbackRepeat callbackRepeat)
    {
        RemoveDelay(null, callbackRepeat);
    }
    #endregion

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------�ڲ�����------------------------------------------------------- */
    /** @brief: -------------------------------------------�ڲ�����------------------------------------------------------- */
    /** @brief: -------------------------------------------�ڲ�����------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region �ű�����ʼ��
    /************************************
     * ����˵��: �ű�����ʼ��
     * �� �� ֵ: void
     * ע������: 
    ************************************/
    void Awake()
    {
        gInstance = this;
    }
    #endregion

    #region �ű�����
    /************************************
     * ����˵��: �ű�����
     * �� �� ֵ: void
     * ע������: 
    ************************************/
    void Update()
    {
        // �����¼������ĳ��� ȷ������ֻ����ǰ�Ļص� ����ӵĲ�����
        int iCount = delayHandlerList.Count;
        for (int i = 0; i < iCount; i++)
        {
            delayHandlerList[i].OnUpdate();
        }
    }

    /*********************************
     * ����˵��: �ű�����
     * �� �� ֵ: void
     * ע������: ��
     *********************************/
    void LateUpdate()
    {
        // ����һ����ʱ�ص����
        int iCount = delayHandlerList.Count;
        for (int i = iCount - 1; i >= 0; i--)
        {
            // ���ص���״̬(����״̬�Ļ�ɾ��)
            if (delayHandlerList[i].state == OperateState.Remove)
            {
                delayHandlerList.RemoveAt(i);
            }
        }

        // ����Ƿ��лص����� ���û�нű��ƿ�
        if (delayHandlerList.Count == 0)
        {
            this.enabled = false;
        }
    }
    #endregion

    #region �ű�����
    /************************************
     * ����˵��: �ű�����
     * �� �� ֵ: void
     * ע������: 
    ************************************/
    void OnDestroy()
    {
        gInstance = null;
    }
    #endregion

    #region ���һ����ʱ�ص�
    /*********************************
     * ����˵��: ���һ����ʱ�ص�
     * �� �� ֵ: void
     * ����˵��: callback �ص�����
     * ����˵��: callbackRepeat �ص�����
     * ����˵��: msgArg �ص������Ĳ���
     * ����˵��: delayFrame ��ʱ֡�� ����һ֡
     * ����˵��: isSameFunctionCallOnce ����һ���ص�������û��ִ�е���ʱ�����ͬһ���ص�����
     * ����˵��: isSameFunctionChangeArg ���һ֮֡�ڵ�ͬһ���ص����� ��ô�����Ƿ���²���
     * ����˵��: isResetDelay �����һ���ص������ȴ�ִ��,�Ƿ�������ʱ�ص�
     * ע������: ��
     *********************************/
    void AddDelay(MsgCallback callback, MsgCallbackRepeat callbackRepeat, object msgArg, int delayFrame = 1, bool isSameFunctionCallOnce = true, bool isSameFunctionChangeArg = false, bool isReSetDelay = true)
    {
        // ����һ����ʱ�ص����
        DelayMsgHandler delayMsgHandler = default(DelayMsgHandler);

        // ������
        if (callback == null && callbackRepeat == null)
        {
            Debug.LogError("You can not add a null Delay Callback , check your args!");
            return;
        }

        // ����Ƿ��Ѿ���ӹ�
        if (isSameFunctionCallOnce == true)
        {
            int iCount = delayHandlerList.Count;
            for (int i = 0; i < iCount; i++)
            {
                delayMsgHandler = delayHandlerList[i];
                // ����״̬�Ļ�������
                if (delayMsgHandler.state == OperateState.Remove)
                {
                    continue;
                }
                // ���ص���״̬
                if ((callback != null && delayMsgHandler.callback == callback) || (callbackRepeat != null && delayMsgHandler.callbackRepeat == callbackRepeat))
                {
                    // �Ƴ���һ����Ϣ ������ֱ�Ӹ�����Ϣ����Ϊ ��ֹ��Update��ʱ�������Ϣ,���ͬһִ֡��
                    delayMsgHandler.state = OperateState.Remove;

                    // ������Ϣ����
                    msgArg = isSameFunctionChangeArg == true ? msgArg : delayMsgHandler.msgArg;

                    // �Ƿ�������ʱ
                    delayFrame = isReSetDelay == true ? delayFrame : delayMsgHandler.delayframe;
                    break;
                }
            }
        }

        // ���һ���µ���Ϣ����β
        delayMsgHandler = new DelayMsgHandler();
        delayMsgHandler.callback = callback;
        delayMsgHandler.callbackRepeat = callbackRepeat;
        delayMsgHandler.msgArg = msgArg;
        delayMsgHandler.state = OperateState.None;
        delayMsgHandler.delayframe = Mathf.Max(1, delayFrame);
        delayMsgHandler.maxframe = delayMsgHandler.delayframe;
        delayMsgHandler.Initialization();
        delayHandlerList.Add(delayMsgHandler);
        // ���ýű�
        this.enabled = true;
    }
    #endregion

    #region ɾ���ص�
    /*********************************
     * ����˵��: ɾ���ص�
     * �� �� ֵ: void
     * ����˵��: callback
     * ����˵��: callbackRepeat
     * ע������: ��
     *********************************/
    void RemoveDelay(MsgCallback callback, MsgCallbackRepeat callbackRepeat)
    {
        // ����һ����ʱ�ص����
        DelayMsgHandler delayMsgHandler = default(DelayMsgHandler);

        // ������
        if (callback == null && callbackRepeat == null)
        {
            return;
        }

        // ����Ƿ��Ѿ���ӹ�
        int iCount = delayHandlerList.Count;
        for (int i = 0; i < iCount; i++)
        {
            delayMsgHandler = delayHandlerList[i];
            // ����״̬�Ļ�������
            if (delayMsgHandler.state == OperateState.Remove)
            {
                continue;
            }
            // ���ص���״̬
            if ((callback != null && delayMsgHandler.callback == callback) || (callbackRepeat != null && delayMsgHandler.callbackRepeat == callbackRepeat))
            {
                // �Ƴ���һ����Ϣ ������ֱ�Ӹ�����Ϣ����Ϊ ��ֹ��Update��ʱ�������Ϣ,���ͬһִ֡��
                delayMsgHandler.state = OperateState.Remove;
                continue;
            }
        }

        // ���ýű�
        this.enabled = true;
    }
    #endregion

    #region ��Ϸ�˳�
    /*********************************
     * ����˵��: ��Ϸ�˳�
     * �� �� ֵ: void
     * ע������: ��
     *********************************/
    void OnApplicationQuit()
    {
        delayHandlerList.Clear();
    }
    #endregion
}