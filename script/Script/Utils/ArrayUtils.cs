// ***************************************************************
//  FileName: ListUtils.cs
//  Version : 1.0
//  Date    : 2016/5/31
//  Author  : cjzhanying 
//  Copyright (C) 2016 - JZWL Digital Technology Co.,Ltd. All rights reserved.	��Ȩ����
//  --------------------------------------------------------------
//  Description: Array����
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

public class ArrayUtils
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
     * ����˵��: mArray
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static void RemoveAll<T>(ref T[] mArray, Predicate<T> match)
    {
        if (mArray == null || mArray.Length == 0)
        {
            return;
        }
        int iCount = mArray.Length;
        for (int i = iCount -1 ; i >= 0; i --)
        {
            if (match(mArray[i]) == true)
            {
                iCount--;
            }
        }

        if (mArray.Length == iCount)
        {
            return;
        }

        T[] array = new T[iCount];        
        for (int i = 0 ; i < iCount; i ++)
        {
            array[i] = mArray[i];
        }
        mArray = array;
    }
    #endregion

    #region ��������
    /*********************************
     * ����˵��: ��������
     * �� �� ֵ: void
     * ����˵��: mArray
     * ����˵��: action
     * ע������: ��
     *********************************/
    public static void ForEach<T>(ref T[] mArray , Action<T> action)
    {
        if (mArray == null || mArray.Length == 0)
        {
            return;
        }
        int iCount = mArray.Length;
        for (int i = 0 ; i < iCount; i ++)
        {
            action(mArray[i]);
        }
    }
    public static void ForEach<T>(ref T[] mArray, Action<T ,int> action)
    {
        if (mArray == null || mArray.Length == 0)
        {
            return;
        }
        int iCount = mArray.Length;
        for (int i = 0; i < iCount; i++)
        {
            action(mArray[i] , i);
        }
    }
    #endregion

    #region ���ҵ�һ��λ��
    /*********************************
     * ����˵��: ���ҵ�һ��λ��
     * �� �� ֵ: int
     * ����˵��: mArray
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static int FindIndex<T>(ref T[] mArray, Predicate<T> match)
    {
        return FindIndex(ref mArray, 0, -1, match);
    }

    public static int FindIndex<T>(ref T[] mArray, int startIndex, Predicate<T> match)
    {
        return FindIndex(ref mArray, startIndex, -1, match);
    }

    public static int FindIndex<T>(ref T[] mArray, int startIndex, int count, Predicate<T> match)
    {
        if (mArray == null || mArray.Length == 0)
        {
            return -1;
        }
        int iCount = mArray.Length;
        count = count < 0 ? iCount : count;
        for (int i = startIndex; i < iCount && i < startIndex + count; i++)
        {
            if (match(mArray[i]) == true)
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
     * ����˵��: mArray
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static int FindLast<T>(ref T[] mArray, Predicate<T> match)
    {
        return FindLastIndex(ref mArray, 0, -1, match);
    }

    public static int FindLastIndex<T>(ref T[] mArray, int startIndex, Predicate<T> match)
    {
        return FindLastIndex(ref mArray, startIndex, -1, match);
    }

    public static int FindLastIndex<T>(ref T[] mArray, int startIndex, int count, Predicate<T> match)
    {
        if (mArray == null || mArray.Length == 0)
        {
            return -1;
        }
        int iCount = mArray.Length;
        count = count < 0 ? 0 : count;
        for (int i = iCount - 1; i >= 0 && i >= startIndex + count; i--)
        {
            if (match(mArray[i]) == true)
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
     * �� �� ֵ: T[]
     * ����˵��: mArray
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static T[] FindAll<T>(ref T[] mArray ,Predicate<T> match)
    {
        List<T> list = new List<T>();
        if (mArray == null || mArray.Length == 0)
        {
            return list.ToArray();
        }
        T node = default(T);        
        int iCount = mArray.Length;
        for (int i = 0 ; i < iCount; i ++)
        {
            node = mArray[i];
            if (match(node) == true)
            {
                list.Add(node);
            }
        }
        return list.ToArray();
    }
    #endregion

    #region ѡ������
    /*********************************
     * ����˵��: ѡ������
     * �� �� ֵ: T[]
     * ����˵��: mArray
     * ����˵��: match
     * ע������: ��
     *********************************/
    public static T Find<T>(ref T[] mArray, Predicate<T> match)
    {
        T node = default(T);
        if (mArray == null || mArray.Length == 0)
        {
            return node;
        }
        int iCount = mArray.Length;
        for (int i = 0; i < iCount; i++)
        {
            node = mArray[i];
            if (match(node) == true)
            {
                return node;
            }
        }
        node = default(T);
        return node;
    }
    #endregion

    public static bool IsNullOrEmpty(System.Object[] array)
    {
        return null == array || array.Length <= 0;
    }

    public static bool IsNullOrEmpty(UnityEngine.Object[] array)
    {
        return null == array || array.Length <= 0;
    }
}