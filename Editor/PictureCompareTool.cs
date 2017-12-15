using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


//用于对比图片资源是否有差异或者有更新
public class PictureCompareTool : Editor {
    public const string MD5FileName = "/PicFile.txt";
    public const string TargetPath = "/Editor/PicFile.txt";
    public static List<string> _picPathSet = new List<string>();
    
    [MenuItem("MyEditor/生成项目内批量图片MD5文件")]
    public static void CreateMD5File()
    {
        string path = Application.dataPath + "/Game/Package/UI";

        path = EditorUtility.OpenFolderPanel("选择图片合集目录", path, "");

        Debug.Log(path);

        BuildMD5VersionByPath(path, Application.dataPath + TargetPath);
    }

    [MenuItem("MyEditor/创建外部美术图片MD5文件")]
    public static void CreateOutsideMD5File()
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
    /*
    /// <summary>
    /// 先计算目标文件夹内图片的MD5值，再与预先创建好的项目内的MD5信息文件对比,然后打开图片对比窗口
    /// </summary>
    [MenuItem("MyEditor/进行图片资源对比")]
    public static void CompareContent()
    {
        string path = EditorUtility.OpenFolderPanel("选择Art文件目录", "", "");

        if (string.IsNullOrEmpty(path)) return;

        bool sureMark = true;
        string comPath = Path.Combine(path, "PicFile.txt");
        if (!File.Exists(comPath))
        {
            Debug.LogWarning("对比文件不存在");
            sureMark = EditorUtility.DisplayDialog("注意", "是否确定为该文件夹该文件夹为图片资源合集", "OK", "Cancel");
        }

        if (!sureMark) return;

        CollectAllFilesPath(path);

        Dictionary<string, string> dicNameMD5 = new Dictionary<string, string>();
        Dictionary<string, string> dicMD5Path = new Dictionary<string, string>();
        for (int i = 0; i < _picPathSet.Count; ++i)
        {
            EditorUtility.DisplayProgressBar("正在计算图片MD5值", _picPathSet[i], (float)i / (float)_picPathSet.Count);

            string md5 = Util.md5file(_picPathSet[i]);
            string name = Path.GetFileName(_picPathSet[i]);

            if (dicNameMD5.ContainsKey(name))
            {
                dicNameMD5[name] = dicNameMD5[name] + '/' + md5;
            }
            else
            {
                dicNameMD5.Add(name, md5);
            }
            if(!dicMD5Path.ContainsKey(md5))
             dicMD5Path.Add(md5, _picPathSet[i]);
        }
        _picPathSet.Clear();

        List<string> fileSet = new List<string>();
        List<string> md5Set = new List<string>();
        List<string> diffSet = new List<string>();

        StreamReader sr = new StreamReader(Application.dataPath + TargetPath);

        while (!sr.EndOfStream)
        {
            fileSet.Add(sr.ReadLine());
            md5Set.Add(sr.ReadLine());
        }
        sr.Close();

        Dictionary<string, string[]> dicStrDouble = new Dictionary<string, string[]>();
        for (int i = 0; i < fileSet.Count; ++i)
        {
            EditorUtility.DisplayProgressBar("正在对比", fileSet[i], (float)i / (float)fileSet.Count);
            string fileName = Path.GetFileName(fileSet[i]);
            string compareContent = "";
            dicNameMD5.TryGetValue(fileName, out compareContent);

            if (compareContent == null)
            {
                continue;
            }

            string[] muliMD5 = compareContent.Split('/');
            bool anyOneEqual = false;
            for (int j = 0; j < muliMD5.Length; ++j)
            {
                if (string.Equals(muliMD5[j], md5Set[i]))
                {
                    anyOneEqual = true;
                    break;
                }
            }

            if (!anyOneEqual)
            {
                diffSet.Add(fileSet[i]);
                string[] strArray = new string[muliMD5.Length];
                for (int n =0; n<muliMD5.Length ;++n )
                {
                    dicMD5Path.TryGetValue(muliMD5[n], out strArray[n]);
                }

                dicStrDouble.Add( fileSet[i] , strArray);
            }
        }
        EditorUtility.ClearProgressBar();

        if (diffSet.Count == 0)
        {
            Debug.Log("并没有差异");
            return;
        }

        StreamWriter diffSr = new StreamWriter(Application.dataPath + "/Editor/DifferentPictures.txt");
        for (int i = 0; i < diffSet.Count; ++i)
        {
            //Debug.Log(diffSet[i]);
            diffSr.WriteLine(diffSet[i]);
            diffSr.Flush();
        }
        diffSr.Close();
        AssetDatabase.Refresh();

        PictureCompareWindow pcw = CreateInstance<PictureCompareWindow>();
        pcw._picPathList = diffSet;
        pcw._dicOneToMany = dicStrDouble;
        pcw.LoadPicture();
        pcw.Show();
    }
    */

    /// <summary>
    /// 先计算目标文件夹内图片的MD5值，再与预先创建好的项目内的MD5信息文件对比,然后打开图片对比窗口
    /// </summary>
    [MenuItem("MyEditor/进行图片资源对比")]
    public static void CompareContent2()
    {
        string path = EditorUtility.OpenFolderPanel("选择Art文件目录", "", "");

        if (path == null) return;

        bool sureMark = true;
        bool existsMark = true;
        string comPath = Path.Combine(path, "PicFile.txt");
        if (!File.Exists(comPath))
        {
            existsMark = false;
            Debug.LogWarning("对比文件不存在");
            sureMark = EditorUtility.DisplayDialog("注意", "是否确定为该文件夹该文件夹为图片资源合集", "OK", "Cancel");
        }

        if (!sureMark) return;

        if (!existsMark)
        {
            BuildMD5VersionByPath(path, comPath);
        }

        ReadMD5FileByPath(comPath);
    }
    /// <summary>
    /// 根据路径读取美术资源的MD5信息文件
    /// </summary>
    /// <param name="path"></param>
    public static void ReadMD5FileByPath(string path)
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
    public static void CompareWithMD5( Dictionary<string , string> nameMD5Set, Dictionary<string, string> md5PathSet , List<string> nameList , List<string> md5List )
    {
        List<string> diffSet = new List<string>();
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
                diffSet.Add(nameList[i]);
                string[] strArray = new string[muliMD5.Length];
                for (int n = 0; n < muliMD5.Length; ++n)
                {
                    md5PathSet.TryGetValue(muliMD5[n], out strArray[n]);
                }
                dicStrDouble.Add(nameList[i], strArray);
            }
        }
        EditorUtility.ClearProgressBar();

        if (diffSet.Count == 0)
        {
            Debug.Log("没有差异");
            return;
        }

        StreamWriter diffSr = new StreamWriter(Application.dataPath + "/Editor/DifferentPictures.txt");
        for (int i = 0; i < diffSet.Count; ++i)
        {
            diffSr.WriteLine(diffSet[i]);
            diffSr.Flush();
        }
        diffSr.Close();
        AssetDatabase.Refresh();

        OpenCompareWindow(diffSet, dicStrDouble);
    }

    public static void OpenCompareWindow( List<string> diffList , Dictionary<string, string[]> dicStrDouble)
    {
        PictureCompareWindow pcw = CreateInstance<PictureCompareWindow>();
        pcw._picPathList = diffList;
        pcw._dicOneToMany = dicStrDouble;
        pcw.LoadPicture();
        pcw.Show();
    }

    /// <summary>
    /// 计算文件夹下图片的MD5值，把数据收录到目标路径下
    /// </summary>
    /// <param name="sourceDirectory"></param>
    /// <param name="targetPath"></param>
    public static void BuildMD5VersionByPath(string sourceDirectory, string targetPath)
    {
        if (sourceDirectory == null || !Directory.Exists(sourceDirectory))
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
    public static void CollectAllFilesPath( string path )
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
