// ***************************************************************
//  FileName: ListUtils.cs
//  Version : 1.0
//  Date    : 2016/5/23
//  Author  : cjzhanying 
//  Copyright (C) 2016 - JZWL Digital Technology Co.,Ltd. All rights reserved.	��Ȩ����
//  --------------------------------------------------------------
//  Description: List����
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

public class ListUtils
{
    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------��������------------------------------------------------------- */
    /** @brief: -------------------------------------------��������------------------------------------------------------- */
    /** @brief: -------------------------------------------��������------------------------------------------------------- */
    /* ====================================================================================================================*/

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------�ⲿ����------------------------------------------------------- */
    /** @brief: -------------------------------------------�ⲿ����------------------------------------------------------- */
    /** @brief: -------------------------------------------�ⲿ����------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region �Ƴ�����
    /*********************************
     * ����˵��: �Ƴ�����
     * �� �� ֵ: void
     * ����˵��: mList
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static void Remove<T>(ref List<T> mList, Predicate<T> match)
    {
        if (mList == null || mList.Count == 0)
        {
            return;
        }
        int iCount = mList.Count;
        for (int i = iCount - 1; i >= 0; i--)
        {
            if (match(mList[i]) == true)
            {
                mList.RemoveAt(i);
                break;
            }
        }
    }
    #endregion

    #region �Ƴ�����
    /*********************************
     * ����˵��: �Ƴ�����
     * �� �� ֵ: void
     * ����˵��: mList
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static void RemoveAll<T>(ref List<T> mList , Predicate<T> match)
    {
        if (mList == null || mList.Count == 0)
        {
            return;
        }
        int iCount = mList.Count;
        for (int i = iCount -1 ; i >= 0; i --)
        {
            if (match(mList[i]) == true)
            {
                mList.RemoveAt(i);
            }
        }
    }
    #endregion

    #region ��������
    /*********************************
     * ����˵��: ��������
     * �� �� ֵ: void
     * ����˵��: mList
     * ����˵��: action
     * ע������: ��
     *********************************/
    public static void ForEach<T>(ref List<T> mList , Action<T> action)
    {
        if (mList == null || mList.Count == 0)
        {
            return;
        }
        int iCount = mList.Count;
        for (int i = 0 ; i < iCount; i ++)
        {
            action(mList[i]);
        }
    }
    public static void ForEach<T>(ref List<T> mList, Action<T ,int> action)
    {
        if (mList == null || mList.Count == 0)
        {
            return;
        }
        int iCount = mList.Count;
        for (int i = 0; i < iCount; i++)
        {
            action(mList[i] , i);
        }
    }
    #endregion

    #region ���ҵ�һ��λ��
    /*********************************
     * ����˵��: ���ҵ�һ��λ��
     * �� �� ֵ: int
     * ����˵��: mList
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static int FindIndex<T>(ref List<T> mList, Predicate<T> match)
    {
        return FindIndex(ref mList, 0, -1, match);
    }

    public static int FindIndex<T>(ref List<T> mList, int startIndex, Predicate<T> match)
    {
        return FindIndex(ref mList, startIndex, -1, match);
    }

    public static int FindIndex<T>(ref List<T> mList, int startIndex, int count, Predicate<T> match)
    {
        if (mList == null || mList.Count == 0)
        {
            return -1;
        }
        int iCount = mList.Count;
        count = count < 0 ? iCount : count;
        for (int i = startIndex; i < iCount && i < startIndex + count; i++)
        {
            if (match(mList[i]) == true)
            {
                return i;
            }
        }
        return -1;
    }
    #endregion

    #region �������һ����λ��
    /*********************************
     * ����˵��: �������һ����λ��
     * �� �� ֵ: int
     * ����˵��: mList
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static int FindLast<T>(ref List<T> mList, Predicate<T> match)
    {
        return FindLastIndex(ref mList, 0, -1, match);
    }

    public static int FindLastIndex<T>(ref List<T> mList, int startIndex, Predicate<T> match)
    {
        return FindLastIndex(ref mList, startIndex, -1, match);
    }

    public static int FindLastIndex<T>(ref List<T> mList, int startIndex, int count, Predicate<T> match)
    {
        if (mList == null || mList.Count == 0)
        {
            return -1;
        }
        int iCount = mList.Count;
        count = count < 0 ? 0 : count;
        for (int i = iCount - 1; i >= 0 && i >= startIndex + count; i--)
        {
            if (match(mList[i]) == true)
            {
                return i;
            }
        }
        return -1;
    }
    #endregion

    #region ѡ������
    /*********************************
     * ����˵��: ѡ������
     * �� �� ֵ: List<T>
     * ����˵��: mList
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static List<T> FindAll<T>(ref List<T> mList ,Predicate<T> match)
    {
        List<T> list = new List<T>();
        if (mList == null || mList.Count == 0)
        {
            return list;
        }
        T node = default(T);        
        int iCount = mList.Count;
        for (int i = 0 ; i < iCount; i ++)
        {
            node = mList[i];
            if (match(node) == true)
            {
                list.Add(node);
            }
        }
        return list;
    }
    #endregion

    #region ѡ������
    /*********************************
     * ����˵��: ѡ������
     * �� �� ֵ: List<T>
     * ����˵��: mList
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static T Find<T>(ref List<T> mList, Predicate<T> match)
    {
        T node = default(T);
        if (mList == null || mList.Count == 0)
        {
            return node;
        }
        int iCount = mList.Count;
        for (int i = 0; i < iCount; i++)
        {
            node = mList[i];
            if (match(node) == true)
            {
                return node;
            }
        }
        node = default(T);
        return node;
    }
    #endregion

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------�ڲ�����------------------------------------------------------- */
    /** @brief: -------------------------------------------�ڲ�����------------------------------------------------------- */
    /** @brief: -------------------------------------------�ڲ�����------------------------------------------------------- */
    /* ====================================================================================================================*/
}