// ***************************************************************
//  FileName: ListUtils.cs
//  Version : 1.0
//  Date    : 2016/5/23
//  Author  : cjzhanying 
//  Copyright (C) 2016 - JZWL Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: List方法
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
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /* ====================================================================================================================*/

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region 移除单个
    /*********************************
     * 函数说明: 移除单个
     * 返 回 值: void
     * 参数说明: mList
     * 参数说明: match
     * 注意事项: 无
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

    #region 移除所有
    /*********************************
     * 函数说明: 移除所有
     * 返 回 值: void
     * 参数说明: mList
     * 参数说明: match
     * 注意事项: 无
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

    #region 遍历所有
    /*********************************
     * 函数说明: 遍历所有
     * 返 回 值: void
     * 参数说明: mList
     * 参数说明: action
     * 注意事项: 无
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

    #region 查找第一个位置
    /*********************************
     * 函数说明: 查找第一个位置
     * 返 回 值: int
     * 参数说明: mList
     * 参数说明: match
     * 注意事项: 无
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

    #region 查找最后一个的位置
    /*********************************
     * 函数说明: 查找最后一个的位置
     * 返 回 值: int
     * 参数说明: mList
     * 参数说明: match
     * 注意事项: 无
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

    #region 选出所有
    /*********************************
     * 函数说明: 选出所有
     * 返 回 值: List<T>
     * 参数说明: mList
     * 参数说明: match
     * 注意事项: 无
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

    #region 选出单个
    /*********************************
     * 函数说明: 选出单个
     * 返 回 值: List<T>
     * 参数说明: mList
     * 参数说明: match
     * 注意事项: 无
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
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /* ====================================================================================================================*/
}