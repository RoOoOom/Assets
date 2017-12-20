using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


//用于对比图片资源是否有差异或者有更新
public class PictureCompareTool : EditorWindow {
    public const string MD5FileName = "/PicFile.txt";
    public const string TargetPath = "/Editor/PictureCompare/PicFile.txt";
    public const string DifferentFilePath = "/Editor/PictureCompare/DifferentPictures.txt";
    public const char SplitChar = '@';
    public static List<string> _picPathSet = new List<string>();
    private bool _ignoreExsitFile = false;
    [MenuItem("Tools/图片对比工具")]
    public static void OpenCompareToolWindow()
    {
        EditorWindow.CreateInstance<PictureCompareTool>().Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        _ignoreExsitFile = EditorGUILayout.Toggle("忽略已存在的MD5信息文件", _ignoreExsitFile);
        EditorGUILayout.Space();

        if (GUILayout.Button("清理旧数据",GUILayout.Height(30)))
        {
            ClearOldFile();
        }

        if (GUILayout.Button("生成项目内批量图片MD5文件", GUILayout.Height(30)))
        {
            CreateMD5File();
        }

        if (GUILayout.Button("创建外部美术图片MD5文件", GUILayout.Height(30)))
        {
            CreateOutsideMD5File();
        }

        if (GUILayout.Button("进行图片资源对比", GUILayout.Height(30)))
        {
            CompareContent();
        }

        if(GUILayout.Button("打开上次对比结果",GUILayout.Height(30)))
        {
            OpenCompareWindow(Application.dataPath + DifferentFilePath);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.HelpBox("重新使用时建议清理，在进行对比之前，要保证已经为项目内图片生成MD5信息文件",MessageType.Info);
    }

    /// <summary>
    /// 清理旧的MD5信息文件和对比结果文件
    /// </summary>
    public void ClearOldFile()
    {
        string path = Application.dataPath + TargetPath;
        if (File.Exists(path))
        {
            File.Delete(path);
            AssetDatabase.Refresh();
        }

        path = Application.dataPath + DifferentFilePath;

        if (File.Exists(path))
        {
            File.Delete(path);
            AssetDatabase.Refresh();
        }
        
    }

    /// <summary>
    /// 生成项目内批量图片MD5文件
    /// </summary>
    public void CreateMD5File()
    {
        string path = Application.dataPath + "/Game/Package/UI";

        path = EditorUtility.OpenFolderPanel("选择图片合集目录", path, "");

        Debug.Log(path);

        BuildMD5VersionByPath(path, Application.dataPath + TargetPath);
    }

    /// <summary>
    /// 创建外部美术图片MD5文件
    /// </summary>
    public void CreateOutsideMD5File()
    {
        /*
        var selectDirectory = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(selectDirectory);
        */

        string path = "";

        path = EditorUtility.OpenFolderPanel("选择Art文件目录", path, "");

        Debug.Log(path);

        BuildMD5VersionByPath(path, path + MD5FileName);

    }

    /// <summary>
    /// 先计算目标文件夹内图片的MD5值，再与预先创建好的项目内的MD5信息文件对比,然后打开图片对比窗口
    /// </summary>
    public void CompareContent()
    {
        string path = EditorUtility.OpenFolderPanel("选择Art文件目录", null, "");
        if (path.Equals("")||path == null) return;

        bool sureMark = true;
        bool existsMark = true;
        string comPath = Path.Combine(path, "PicFile.txt");

        if (!File.Exists(comPath) && !_ignoreExsitFile)
        {
            existsMark = false;
            Debug.LogWarning("对比文件不存在");
            sureMark = EditorUtility.DisplayDialog("注意", "是否确定为该文件夹该文件夹为图片资源合集", "OK", "Cancel");
        }

        if (!sureMark) return;

        if (!existsMark || _ignoreExsitFile)
        {
            BuildMD5VersionByPath(path, comPath);
        }

        ReadMD5FileByPath(comPath);
    }
    /// <summary>
    /// 根据路径读取美术资源的MD5信息文件
    /// </summary>
    /// <param name="path"></param>
    public void ReadMD5FileByPath(string path)
    {
        StreamReader sr = new StreamReader(path);

        Dictionary<string, string> dicNameMD5 = new Dictionary<string, string>();
        Dictionary<string, string> dicMD5Path = new Dictionary<string, string>();

        while ( !sr.EndOfStream )
        {
            string filePath = sr.ReadLine();
            string md5Value = sr.ReadLine();
            string fileName = Path.GetFileName(filePath);

            if (dicNameMD5.ContainsKey(fileName))
            {
                dicNameMD5[fileName] = dicNameMD5[fileName] + '/' + md5Value;
            }
            else
            {
                dicNameMD5.Add(fileName, md5Value);
            }

            if (!dicMD5Path.ContainsKey(md5Value))
            {
                dicMD5Path.Add(md5Value, filePath);
            }
        }

        sr.Close();
        sr = null;


        List<string> fileSet = new List<string>();
        List<string> md5Set = new List<string>();

        sr = new StreamReader(Application.dataPath + TargetPath);

        while (!sr.EndOfStream)
        {
            fileSet.Add(sr.ReadLine());
            md5Set.Add(sr.ReadLine());
        }
        sr.Close();
        sr = null;

       CompareWithMD5(dicNameMD5, dicMD5Path, fileSet, md5Set);
    }

    /// <summary>
    /// 进行对比，第一个字典是名字-MD5合集，第二个字典是md5值-绝对路径
    /// </summary>
    /// <param name="nameMD5Set"></param>
    /// <param name="md5PathSet"></param>
    /// <param name="nameList"></param>
    /// <param name="md5List"></param>
    public void CompareWithMD5( Dictionary<string , string> nameMD5Set, Dictionary<string, string> md5PathSet , List<string> nameList , List<string> md5List )
    {
        List<string> diffList = new List<string>();
        StringBuilder streamBuffer = new StringBuilder();

        Dictionary<string, string[]> dicStrDouble = new Dictionary<string, string[]>();

        for (int i = 0; i < nameList.Count; ++i)
        {
            EditorUtility.DisplayProgressBar("正在对比", nameList[i], (float)i / (float)nameList.Count);
            string fileName = Path.GetFileName(nameList[i]);
            string compareContent = "";
            nameMD5Set.TryGetValue(fileName, out compareContent);

            if (compareContent == null)
            {
                continue;
            }
            string[] muliMD5 = compareContent.Split('/');
            bool anyOneEqual = false;
            for (int j = 0; j < muliMD5.Length; ++j)
            {
                if (string.Equals(muliMD5[j], md5List[i]))
                {
                    anyOneEqual = true;
                    break;
                }
            }
            if (!anyOneEqual)
            {
                streamBuffer.AppendLine(nameList[i] + SplitChar + muliMD5.Length + SplitChar + "0");
                diffList.Add(nameList[i]);
                string[] strArray = new string[muliMD5.Length];
                for (int n = 0; n < muliMD5.Length; ++n)
                {
                    md5PathSet.TryGetValue(muliMD5[n], out strArray[n]);
                    streamBuffer.AppendLine(strArray[n]);
                }
                dicStrDouble.Add(nameList[i], strArray);
            }
        }
        EditorUtility.ClearProgressBar();

        if (streamBuffer.Length == 0)
        {
            Debug.Log("没有差异");
            return;
        }

        StreamWriter diffSr = new StreamWriter(Application.dataPath +DifferentFilePath);

        diffSr.Write(streamBuffer.ToString());
        diffSr.Flush();
        
        diffSr.Close();
        AssetDatabase.Refresh();

        OpenCompareWindow(diffList, dicStrDouble);
    }

    public void OpenCompareWindow( List<string> diffList , Dictionary<string, string[]> dicStrDouble)
    {
        PictureCompareWindow pcw = CreateInstance<PictureCompareWindow>();
        pcw._picPathList = diffList;
        pcw._dicOneToMany = dicStrDouble;
        pcw.LoadPicture();
        pcw.Show();
    }

    /// <summary>
    /// 打开对比展示窗口
    /// </summary>
    /// <param name="path">差异信息文件的路径</param>
    public void OpenCompareWindow(string path)
    {
        if(!File.Exists(path))
        {
            Debug.LogWarning("差异文件不存在");
            return;
        }

        PictureCompareWindow pcw = CreateInstance<PictureCompareWindow>();
        pcw.LoadDifferentFile(path);
        pcw.LoadPicture();
        pcw.Show();
    }

    /// <summary>
    /// 计算文件夹下图片的MD5值，把数据收录到目标路径下
    /// </summary>
    /// <param name="sourceDirectory"></param>
    /// <param name="targetPath"></param>
    public void BuildMD5VersionByPath(string sourceDirectory, string targetPath)
    {
        if (sourceDirectory == null ||sourceDirectory.Equals("")|| !Directory.Exists(sourceDirectory))
        {
            Debug.LogWarning("目录不存在");
            return;
        }

        CollectAllFilesPath(sourceDirectory);

        StreamWriter sw = new StreamWriter(targetPath);

        Debug.Log("文件数量:" + _picPathSet.Count);


        for (int i = 0; i < _picPathSet.Count; i++)
        {
            EditorUtility.DisplayProgressBar("正在生成MD5信息文件", _picPathSet[i], (float)i / (float)_picPathSet.Count);
            sw.WriteLine(_picPathSet[i]);
            string md5 = CommonAlgorithm.md5file(_picPathSet[i]);
            sw.WriteLine(md5);
            sw.Flush();
        }
        EditorUtility.ClearProgressBar();

        sw.Close();
        _picPathSet.Clear();

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 收集path文件夹下所有的图片资源
    /// </summary>
    /// <param name="path"></param>
    public  void CollectAllFilesPath( string path )
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);

        FileInfo[] filesInfo = dirInfo.GetFiles();
        
        for ( int i =0; i<filesInfo.Length ;++i )
        {
            string ext = Path.GetExtension(filesInfo[i].FullName);
            if (!string.Equals(ext , ".png") && !string.Equals(ext , ".jpg"))
            {
                continue;
            }

            _picPathSet.Add(filesInfo[i].FullName);
        }

        DirectoryInfo[] dirSet = dirInfo.GetDirectories();

        for (int j = 0; j < dirSet.Length; ++j)
        {
            CollectAllFilesPath(Path.Combine(path, dirSet[j].Name));
        }
    }

}
