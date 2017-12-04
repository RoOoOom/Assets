using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEditor;

public enum SDKType {
    ddlhzb,
    hlcszb,
    hlcsyy,
    npcqyy,
    empty,
}

public class ImportSDKWindow : EditorWindow {
    public const string RootDirectory = "Mods";
    public const string SetDirectory = "SDK_Mods";
    public const string pictureDirectory = "Picture";
    public const string copeDirecotry = "Cope";
    public const string sdkDirectory = "SDK";

    bool[] toggleArray;
    SDKType m_selectType = SDKType.empty;
    string m_path;

    Dictionary<string, string> _statement;

    [MenuItem("MyEditor/打开渠道选择窗口")]
    public static void OpenPlatfomSelectWindow()
    {
        EditorWindow.GetWindow<ImportSDKWindow>().Show();
    }

    private void Awake()
    {
        toggleArray = new bool[Enum.GetValues(typeof(SDKType)).Length];

		m_path = Application.dataPath;
		m_path = m_path.Replace("Assets", "");
		m_path = Path.Combine(m_path, RootDirectory);

        _statement = new Dictionary<string, string>();

        LoadStatement();
    }

    private void OnGUI()
    {
        int i = 0;

        if(GUILayout.Button("创建渠道对应的文件夹"))
        {
            CreateAllPlatformDirectory();
        }

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.LabelField("选择平台");
        
        EditorGUI.indentLevel++;
        foreach ( SDKType toggleLabel in Enum.GetValues(typeof(SDKType)) )
        {
            string packageName = "无平台";
            switch (toggleLabel)
            {
                case SDKType.ddlhzb:
                    packageName = "刀刀烈火正版(ddlhzb)";
                    break;
                case SDKType.hlcsyy:
                    packageName = "火龙传说越狱(hlcsyy)";
                    break;
                case SDKType.hlcszb:
                    packageName = "火龙传说正版(hlcszb)";
                    break;
                case SDKType.npcqyy:
                    packageName = "涅槃传奇越狱(npcqyy)";
                    break;
            }

            toggleArray[i] = EditorGUILayout.ToggleLeft(packageName, toggleArray[i]);
            if (toggleArray[i])
            {
                m_selectType = toggleLabel;
                SetOtherFalse(i);
            }
            
            i++;   
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("导入sdk"))
        {
            CopySourceByType(m_selectType);
        }

        string info = "";
        _statement.TryGetValue( m_selectType.ToString() , out info);

        GUIStyle gS = new GUIStyle();
        gS.fontSize = 20;
       
        GUI.TextField(new Rect(70, 230, 400, 100),info, gS);
        //EditorGUILayout.HelpBox(info,MessageType.Info);

    }

    void SetOtherFalse(int index)
    {
        for (int i = 0; i < toggleArray.Length; ++i)
        {
            if (index == i) continue;
            toggleArray[i] = false;
        }
    }

    /// <summary>
    /// 创建Assets同级目录下的一系列所需的文件夹
    /// </summary>
    void CreateAllPlatformDirectory()
    {
        if (!Directory.Exists(m_path))
        {
            Directory.CreateDirectory(m_path);
        }

        foreach (string type in Enum.GetNames(typeof(SDKType)))
        {
            string sonPath = Path.Combine(m_path, type);
            if (!Directory.Exists(sonPath))
            {
                Directory.CreateDirectory(sonPath);
            }

            string pic = Path.Combine(sonPath, pictureDirectory);
            if (!Directory.Exists(pic))
            {
                Directory.CreateDirectory(pic);
            }

            pic = Path.Combine(sonPath, copeDirecotry);
            if (!Directory.Exists(pic))
            {
                Directory.CreateDirectory(pic);
            }

            pic = Path.Combine(sonPath, sdkDirectory);
            if ( !Directory.Exists(pic) )
            {
                Directory.CreateDirectory(pic);
            }
        }

    }

    public void CopySourceByType( SDKType sdkType )
    {
		Debug.Log (m_path);

        string setPath = Path.Combine(m_path, sdkType.ToString());
        string sdkPath = Path.Combine(setPath, sdkDirectory);
        string picPath = Path.Combine(setPath, pictureDirectory);
        string copePath = Path.Combine(setPath, copeDirecotry);
        string destPath = Path.Combine(Application.dataPath, SetDirectory);
        string replacePath = Application.dataPath.Replace("Assets", "ReplaceXCode");

        //ClearOtherSDK(sdkType);

		DeleteDirByPath (destPath);

        CopyFolderTo(sdkPath, destPath , "sdk资源");

        CopyFolderTo(copePath, replacePath,"");

        InsteadPiacture(picPath);

		AssetDatabase.Refresh ();
    }


    /// <summary>
    /// 清理传入的平台之外的其他平台的SDK
    /// </summary>
    /// <param name="sdkType"></param>
    public void ClearOtherSDK( SDKType sdkType ) 
    {
        string rootPath = Path.Combine(Application.dataPath, SetDirectory);

        foreach ( SDKType t in Enum.GetValues(typeof(SDKType)) )
        {
            if (t != sdkType)
            {
                string dicPath = t.ToString();
                if (Directory.Exists(Path.Combine(rootPath,dicPath)))
                {
                    DeleteDir(dicPath);
                }
            }
        }
    }

    /// <summary>
    /// 将第一个目录下的文件复制到第二个目录下
    /// </summary>
    /// <param name="directorySource"></param>
    /// <param name="directoryTarget"></param>
    public void CopyFolderTo(string directorySource, string directoryTarget , string statement = "")
    {
        if (!Directory.Exists(directorySource))
        {
            Debug.LogWarning("要复制的目录不存在");
        }

        /*
        if (Directory.Exists(directoryTarget))
        {
            
            bool choose = EditorUtility.DisplayDialog("注意", statement + "目录已存在，确定要替换吗?", "确定", "取消");
            if (choose)
                DeleteDir(directoryTarget);
            else
                return;
        }
        
        Directory.CreateDirectory(directoryTarget);
        */

        if (!Directory.Exists(directoryTarget))
        {
            Directory.CreateDirectory(directoryTarget);
        }

        DirectoryInfo dirInfo = new DirectoryInfo(directorySource);

        FileInfo[] files = dirInfo.GetFiles();

        int i = 1;
        foreach (FileInfo file in files)
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
    public void DeleteDir(string srcpath)
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

	public void DeleteDirByPath(string srcpath)
	{
		DirectoryInfo dirInfo = new DirectoryInfo(srcpath);

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
	}

    /// <summary>
    /// 替换图片资源
    /// </summary>
    /// <param name="sourcePath"></param>
    public void InsteadPiacture(string sourcePath)
    {
        string bcPath = Application.dataPath + "/BuildConfig";
		string iconPath = Path.Combine (sourcePath, "icon.png");
		string splashPath = Path.Combine (sourcePath, "splash.jpg");

		if (File.Exists (iconPath)) {
			File.Copy (iconPath, Path.Combine (bcPath, "icon.png"), true);
		} else {
			Debug.LogError ("icon资源不存在");
		}

		if (File.Exists (splashPath)) {
			File.Copy (splashPath, Path.Combine (bcPath, "splash.jpg"), true);
		} else {
			Debug.LogError ("splash资源不存在");
		}
       
    }


    void LoadStatement()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Application.dataPath + "/Editor/SDK_statement.xml");

        XmlElement root = xmlDoc.DocumentElement;
        XmlNodeList nodeList = root.ChildNodes;
        
        foreach(XmlElement xn in nodeList )
        {
            string key = xn.SelectSingleNode("Key").InnerText;
            string val = xn.SelectSingleNode("Value").InnerText;
            _statement.Add(key,val);
        }
    }
}
