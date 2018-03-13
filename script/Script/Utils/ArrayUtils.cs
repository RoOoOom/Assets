// ***************************************************************
//  FileName: ListUtils.cs
//  Version : 1.0
//  Date    : 2016/5/31
//  Author  : cjzhanying 
//  Copyright (C) 2016 - JZWL Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: Array方法
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
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /* ====================================================================================================================*/

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region 移除所有
    /*********************************
     * 函数说明: 移除所有
     * 返 回 值: void
     * 参数说明: mArray
     * 参数说明: match
     * 注意事项: 无
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

    #region 遍历所有
    /*********************************
     * 函数说明: 遍历所有
     * 返 回 值: void
     * 参数说明: mArray
     * 参数说明: action
     * 注意事项: 无
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

    #region 查找第一个位置
    /*********************************
     * 函数说明: 查找第一个位置
     * 返 回 值: int
     * 参数说明: mArray
     * 参数说明: match
     * 注意事项: 无
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

    #region 查找最后一个的位置
    /*********************************
     * 函数说明: 查找最后一个的位置
     * 返 回 值: int
     * 参数说明: mArray
     * 参数说明: match
     * 注意事项: 无
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

    #region 选出所有
    /*********************************
     * 函数说明: 选出所有
     * 返 回 值: T[]
     * 参数说明: mArray
     * 参数说明: match
     * 注意事项: 无
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

    #region 选出单个
    /*********************************
     * 函数说明: 选出单个
     * 返 回 值: T[]
     * 参数说明: mArray
     * 参数说明: match
     * 注意事项: 无
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