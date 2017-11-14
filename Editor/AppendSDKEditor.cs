﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;

public class AppendSDKEditor : Editor {
    public const string SrcFolder = "Mods";
    public const string DstFolder = "/Xuporter/Mods";
    public const string XMLpath = "/Editor/SDK_Reflection.xml";
    public const string RootName = "Root";
    public const string NodeKey = "sdk";
    public const string NodeAttr = "id";


    [MenuItem("MyEditor/CreateSDKReflection")]
    public static void CreateSDKReflection()
    {
        EditorWindow.GetWindow<AppendSDKWindow>().Show();
    }

    [MenuItem("MyEditor/添加SDK资源/添加刀刀烈火SDK")]
    public static void AppendDDLH()
    {
        CopySourceByID("ddlh");
    }

    /// <summary>
    /// 根据映射表中的id获取文件夹名，然后整个文件夹的内容copy到指定路径中
    /// </summary>
    /// <param name="attrId"></param>
    public static void CopySourceByID( string attrId )
    {
        string srcPath = Application.dataPath.Replace("Assets",SrcFolder);
        string dstPath = Application.dataPath + DstFolder;

        string xmlPath = Application.dataPath + XMLpath;

        if (!File.Exists(xmlPath)) {
            Debug.LogWarning("映射表xml文件不存在");
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);

        XmlElement root = xmlDoc.DocumentElement;

        string nodeName ="/"+ RootName + "/" + NodeKey + "[@" + NodeAttr + "='" + attrId + "']";

        Debug.Log(nodeName);

        XmlElement element = (XmlElement)root.SelectSingleNode(nodeName);

        srcPath = Path.Combine(srcPath , element.InnerText);
        srcPath = srcPath.Replace('/','\\');

        dstPath = Path.Combine(dstPath, element.InnerText);
        dstPath = dstPath.Replace('/', '\\');
        Debug.Log("源路径:" + srcPath);

        CopyFolderTo(srcPath, dstPath);
    }


    /// <summary>
    /// 将第一个目录下的文件复制到第二个目录下
    /// </summary>
    /// <param name="directorySource"></param>
    /// <param name="directoryTarget"></param>
    public static void CopyFolderTo( string directorySource , string directoryTarget )
    {
        if (!Directory.Exists(directorySource))
        {
            Debug.LogWarning("要复制的目录不存在");
        }

        if (Directory.Exists(directoryTarget))
        {
            Directory.Delete(directoryTarget);
        }

        Directory.CreateDirectory(directoryTarget);


        DirectoryInfo dirInfo = new DirectoryInfo(directorySource);

        FileInfo[] files = dirInfo.GetFiles();

        foreach ( FileInfo file in files )
        {
            file.CopyTo(Path.Combine(directoryTarget, file.Name));
        }

        DirectoryInfo[] directoryInfoArray = dirInfo.GetDirectories();
        foreach (DirectoryInfo di in directoryInfoArray)
        {
            CopyFolderTo(Path.Combine(directorySource, di.Name), Path.Combine(directoryTarget, di.Name));
        }
    }


}