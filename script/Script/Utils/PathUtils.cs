// ***************************************************************
//  FileName: PathUtils.cs
//  Version : 1.0
//  Date    : 2016/6/22
//  Author  : cjzhanying 
//  Copyright (C) 2016 - Digital Technology Co.,Ltd. All rights reserved.	版权申明
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

public class PathUtils
{
    public enum PathType
    {
        None,
        MobileDiskStreamAssert,
        MobileDiskWrite,
        MobileTCardStreamAssert,
        Http,
    }
    static string gRootMobileDiskStreamAssertPath = ""; //@brief: Bin文件内部 StraemAssert根目录 只读 
    static string gRootMobileDiskWritePath = ""; // @brief: Bin文件内部 读写目录 
    static string gRootMobileTCardStreamAssertPath = ""; // @brief: T卡下载的 StraemAssert根目录 读写 
    static string gMobileTcardDicName = "VisualCard/"; /** @brief: T卡资源的根文件夹名(根据需要,苹果下面可以忽略) */

    public static string streamingAssetsPath = "";
    public static string persistentDataPath = "";
    public static string dataPath = "";
    public static RuntimePlatform platform = RuntimePlatform.WindowsPlayer;

#if UNITY_STANDALONE
    public static string osDir = "Win";
#elif UNITY_ANDROID
    public static string osDir = "Android";            
#elif UNITY_IOS
    public static string osDir = "IOS";        
#else
    public static string osDir = "";        
#endif

    /* ============================================================================================================================*/
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数--------------------------------------------------------------- */
    /* ============================================================================================================================*/

    /// <summary>
    /// 仅限主线程调用
    /// </summary>
    public static void Init()
    {
        streamingAssetsPath = Application.streamingAssetsPath;
        persistentDataPath = Application.persistentDataPath;
        dataPath = Application.dataPath;
        platform = Application.platform;
        InitPath();
    }

    /************************************
     * 函数说明: 检查路径
     * 返 回 值: void
     * 注意事项: 
     ************************************/
    public static string CheckPath(string szPath)
    {
        if (string.IsNullOrEmpty(szPath))
        {
            return "";
        }
        szPath = szPath.Replace("\\", "/");

        /** @brief: 校验结尾 */
        if (szPath.EndsWith("/") == false)
        {
            szPath += "/";
        }
        return szPath;
    }

    public static string GetFileName(string szFileFullName)
    {
        if (string.IsNullOrEmpty(szFileFullName) == false)
        {
            return Path.GetFileName(szFileFullName);
        }
        return "";
    }

    public static string GetDirectoryName(string szFileFullName)
    {
        if (string.IsNullOrEmpty(szFileFullName) == false)
        {
            string szPath = Path.GetDirectoryName(szFileFullName);
            return CheckPath(szPath);
        }
        return "";
    }

    public static bool CreatDir(string szPath)
    {
        if (string.IsNullOrEmpty(szPath) == false)
        {
            szPath = GetDirectoryName(szPath);
            if (Directory.Exists(szPath) == false)
            {
                try
                {
                    Directory.CreateDirectory(szPath);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("CreatDir : System Error :" + ex.Message.ToString());
                }
                return false;
            }
            return true;
        }
        return false;
    }

    public static bool DelDir(string szPath)
    {
        if (string.IsNullOrEmpty(szPath) == false)
        {
            szPath = GetDirectoryName(szPath);
            if (Directory.Exists(szPath) == true)
            {
                try
                {
                    Directory.Delete(szPath, true);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("DelDir : System Error :" + ex.Message.ToString());
                }
                return false;
            }
            return true;
        }
        return false;
    }

    /************************************
     * 函数说明: 获取路径
     * 返 回 值: string
     * 参数说明: szFileName 相对路径的文件名 eg /Game/Res/1.xml 或者 Game/Res/1.xml
     * 参数说明: eType 路径类型
     * 注意事项: 文件名为空的时候返回Root路径
     ************************************/
    public static string MakeFilePath(string szFileName, PathType eType)
    {
        string szFileFullPath = "";
        switch (eType)
        {
            case PathType.MobileDiskStreamAssert:
                szFileFullPath = GetRootMobileDiskStreamAssertPath();
                break;
            case PathType.MobileDiskWrite:
                szFileFullPath = GetRootMobileDiskWritePath();
                break;
            case PathType.MobileTCardStreamAssert:
                szFileFullPath = GetRootMobileTCardStreamAssertPath();
                break;
        }

        if (string.IsNullOrEmpty(szFileName) == false)
        {
            szFileName.Replace("\\", "/");
            /** @brief: 纠正文件名错误 */
            if (szFileName.StartsWith("/"))
            {
                szFileName = szFileName.Substring(1);
            }
            szFileFullPath += szFileName;
        }
        return szFileFullPath;
    }

    /************************************
     * 函数说明: 获取路径深度
     * 返 回 值: int
     * 参数说明: szPath
     * 注意事项: eg E:/Test/Test/Test.png 是3层
     ************************************/
    public static int GetPathDepth(string szPath)
    {
        int iDeep = 0;
        if (string.IsNullOrEmpty(szPath) == false)
        {
            szPath = szPath.Replace("\\", "/");
            int iFindIndex = 0;
            for (; iFindIndex < szPath.Length; )
            {
                iFindIndex = szPath.IndexOf("/", iFindIndex);
                if (iFindIndex < 0)
                {
                    break;
                }
                else
                {
                    iFindIndex++;
                    iDeep++;
                }
            }
        }
        return iDeep;
    }
    
    /************************************
     * 函数说明: 获取路径第几层的名字
     * 返 回 值: string
     * 参数说明: szPath
     * 参数说明: iDepth
     * 注意事项: eg E:/Test1/Test2/Test.png 是3层 第零层名字 E: 第一层名字 Test1 第二层名字 Test2 没有第三层
     ************************************/
    public static string GetPathNameWithDepth(string szPath, int iDepth)
    {
        return GetPathNameWithDepth(szPath, iDepth, iDepth + 1);
    }
    public static string GetPathNameWithDepth(string szPath, int iSrcDepth, int iDstDetpth)
    {
        string szTemp = "";
        szPath = szPath.Replace("\\", "/");
        int iMaxDepth = GetPathDepth(szPath);
        if (iMaxDepth > iSrcDepth && iMaxDepth >= iDstDetpth && iSrcDepth >= 0 && iDstDetpth > iSrcDepth)
        {
            int iSrc = 0;
            int iDst = 0;
            int iFindIndex = 0;
            int iTempDeep = 0;
            for (; iFindIndex < szPath.Length; )
            {
                iFindIndex = szPath.IndexOf("/", iFindIndex);
                if (iFindIndex < 0)
                {
                    break;
                }
                else
                {
                    iFindIndex++;
                    iTempDeep++;
                    if (iTempDeep == iSrcDepth)
                    {
                        iSrc = iFindIndex;
                    }
                    if (iTempDeep == iDstDetpth)
                    {
                        iDst = iFindIndex - 1;
                        break;
                    }
                }
            }
            if (iSrc < iDst)
            {
                szTemp = szPath.Substring(iSrc, iDst - iSrc);
            }
        }
        return szTemp;
    }

    /* ============================================================================================================================*/
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数--------------------------------------------------------------- */
    /* ============================================================================================================================*/

    static void InitPath()
    {
        /** @brief: 判断是否初始化过 */
        if (string.IsNullOrEmpty(gRootMobileDiskStreamAssertPath) == false
            && string.IsNullOrEmpty(gRootMobileTCardStreamAssertPath) == false
            && string.IsNullOrEmpty(gRootMobileDiskWritePath) == false)
        {
            return;
        }
        /** @brief: 路径初始化 */
        switch (platform)
        {
            case RuntimePlatform.Android:
                {
                    gRootMobileDiskStreamAssertPath = string.Format("{0}!/assets/", dataPath);
                    gRootMobileDiskWritePath = string.Format("{0}/", persistentDataPath);
#if UNITY_ANDROID
                    // 通过SDK获取SD卡的路径 gRootMobileTCardStreamAssertPath
#else
                    // 其他平台或者测试环境使用默认配置
                    gRootMobileTCardStreamAssertPath = "/mnt/sdcard";
#endif
                }
                break;
            case RuntimePlatform.IPhonePlayer:
                {
                    gRootMobileDiskStreamAssertPath = string.Format("{0}/", streamingAssetsPath);
                    gRootMobileDiskWritePath = string.Format("{0}/", persistentDataPath);
                    gRootMobileTCardStreamAssertPath = string.Format("{0}/", persistentDataPath);
                }
                break;
            default:
                {
                    gRootMobileDiskStreamAssertPath = string.Format("{0}/StreamingAssets/", dataPath);
                    gRootMobileDiskWritePath = string.Format("{0}/../MobileTCard/", dataPath);
                    gRootMobileTCardStreamAssertPath = string.Format("{0}/VisualCard/", dataPath);
                }
                break;
        }
        /** @brief: 校验路径 */
        gRootMobileDiskStreamAssertPath = CheckPath(gRootMobileDiskStreamAssertPath);
        gRootMobileDiskWritePath = CheckPath(gRootMobileDiskWritePath);
        gRootMobileTCardStreamAssertPath = CheckPath(gRootMobileTCardStreamAssertPath);

        /** @brief: 建立文件夹 */
        gRootMobileTCardStreamAssertPath += gMobileTcardDicName;
        /** @brief: 建立T卡目录 */
        //if (!Directory.Exists(gRootMobileTCardStreamAssertPath))
        //{
        //    Directory.CreateDirectory(gRootMobileTCardStreamAssertPath);
        //}
    }

    /// <summary>
    /// 获取Bin文件内部资源路径（路径只读, StreamAssertPath）
    /// </summary>
    /// <returns></returns>
    static string GetRootMobileDiskStreamAssertPath()
    {
        return gRootMobileDiskStreamAssertPath;
    }

    /// <summary>
    /// 获取Bin内部可读写资源路径（路径可读写）
    /// </summary>
    /// <returns></returns>
    static string GetRootMobileDiskWritePath()
    {
        return gRootMobileDiskWritePath;
    }

    /// <summary>
    /// 获取Bin内部可读写资源路径
    /// </summary>
    /// <returns></returns>
    static string GetRootMobileTCardStreamAssertPath()
    {
        return gRootMobileTCardStreamAssertPath;
    }

    private static bool _IsDir(string path)
    {
        FileInfo fi = new FileInfo(path);
        if ((fi.Attributes & FileAttributes.Directory) != 0)
            return true;
        else
        {
            return false;
        }
    }

    /// <summary> 获取目录下的所有文件路径 </summary>
    public static void GetAllFiles(string dir, List<string> paths, bool deep = true, bool containMeta = false)
    {
        if (_IsDir(dir))
        {
            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                if (containMeta)
                {
                    paths.Add(file);
                }
                else if (!file.Contains(".meta"))
                {
                    paths.Add(file);
                }
            }
            string[] dirs = Directory.GetDirectories(dir);
            if (deep) foreach (string d in dirs) GetAllFiles(d, paths, deep, containMeta);
        }
        else
        {
            paths.Add(dir);
        }
    }

    #region 获取本地可读资源路径
    /************************************
     * 函数说明: 获取本地可读资源路径
     * 返 回 值: string
    ************************************/
    public static string GetReadablePath(string fileName, ref PathUtils.PathType eType, bool addPre = true)
    {
        string szUrl = "";
        do
        {
            //if (!GameConfig.useLocalRes)
            {
                if (eType == PathUtils.PathType.None)
                {
                    switch (platform)
                    {
                        case RuntimePlatform.OSXEditor:
                        case RuntimePlatform.OSXPlayer:
                        case RuntimePlatform.WindowsPlayer:
                        case RuntimePlatform.WindowsEditor:
                            szUrl = PathUtils.MakeFilePath(fileName, PathUtils.PathType.MobileDiskWrite);
                            break;
                        case RuntimePlatform.Android:
                            szUrl = PathUtils.MakeFilePath(fileName, PathUtils.PathType.MobileDiskWrite);
                            break;
                        case RuntimePlatform.IPhonePlayer:
                            szUrl = PathUtils.MakeFilePath(fileName, PathUtils.PathType.MobileDiskWrite);
                            break;
                        default:
                            JZLog.LogError("GetBundle : Error For Platform");
                            break;
                    }

                    if (GameUtils.FileExist(szUrl) == true)
                    {
                        eType = PathUtils.PathType.MobileDiskWrite;
                        break;
                    }
                }
            }

            /** @brief: 默认手机盘只读数据 */
            if (eType == PathUtils.PathType.None)
            {
                switch (platform)
                {
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.WindowsEditor:
                        szUrl = PathUtils.MakeFilePath(fileName, PathUtils.PathType.MobileDiskStreamAssert);
                        break;
                    case RuntimePlatform.Android:
                        szUrl = dataPath + "!assets/" + fileName;
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        szUrl = PathUtils.MakeFilePath(fileName, PathUtils.PathType.MobileDiskStreamAssert);
                        break;
                    default:
                        JZLog.LogError("GetBundle : Error For Platform");
                        break;
                }

                eType = PathUtils.PathType.MobileDiskStreamAssert;
            }
        } while (false);
        return szUrl;
    }
    #endregion

    /// <summary>
    /// 获取指定目录下的所有目录路径（不包含自身）
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="list"></param>
    public static void GetAllDirs(string dir, List<string> list)
    {
        string[] dirs = Directory.GetDirectories(dir);
        list.AddRange(dirs);

        for (int i = 0; i < dirs.Length; i++)
        {
            GetAllDirs(dirs[i], list);
        }
    }
}