using System.Collections;
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


    [MenuItem("MyEditor/编辑映射关系表")]
    public static void CreateSDKReflection()
    {
        EditorWindow.GetWindow<AppendSDKWindow>().Show();
    }

    [MenuItem("MyEditor/添加SDK资源/添加刀刀烈火SDK")]
    public static void AppendDDLH()
    {
        CopySourceByID("ddlh");

        AssetDatabase.Refresh();
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

        if (element == null)
        {
            Debug.LogWarning("映射关系不对，检查id，文件夹名是否正确");
            return;
        }

        srcPath = Path.Combine(srcPath , element.InnerText);
        
        srcPath = srcPath.Replace('/','\\');//在MAC系统下这条语句要注释掉

        dstPath = Path.Combine(dstPath, element.InnerText);
        dstPath = dstPath.Replace('/', '\\');//在MAC系统下这条语句要注释掉
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
            bool choose = EditorUtility.DisplayDialog("注意", "SDK资源目录已存在，确定要替换吗?", "确定", "取消");
            if (choose)
               DeleteDir(directoryTarget);
            else
                return;
        }

        Directory.CreateDirectory(directoryTarget);


        DirectoryInfo dirInfo = new DirectoryInfo(directorySource);

        FileInfo[] files = dirInfo.GetFiles();

        int i = 1;
        foreach ( FileInfo file in files )
        {
            EditorUtility.DisplayProgressBar("正在复制...", file.Name, (float)i / (float)files.Length);
            file.CopyTo(Path.Combine(directoryTarget, file.Name));
            i++;
        }
        EditorUtility.ClearProgressBar();
        
        DirectoryInfo[] directoryInfoArray = dirInfo.GetDirectories();
        foreach (DirectoryInfo di in directoryInfoArray)
        {
            CopyFolderTo(Path.Combine(directorySource, di.Name), Path.Combine(directoryTarget, di.Name));
        }
    }
    /// <summary>
    /// 删除已存在的文件夹
    /// </summary>
    /// <param name="srcpath"></param>
    public static void DeleteDir(string srcpath)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(srcpath);
        dirInfo.Delete(true);

        /*
        FileSystemInfo[] fileList = dirInfo.GetFileSystemInfos();

        for (int i = 0; i < fileList.Length; ++i)
        {
            if (fileList[i] is DirectoryInfo)
            {
                DirectoryInfo tempInfo = new DirectoryInfo(fileList[i].FullName);
                tempInfo.Delete(true);
            }
            else {
                File.Delete(fileList[i].FullName);
            }
        }
        */ 
    }



}
