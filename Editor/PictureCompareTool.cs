using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


//用于对比图片资源是否有差异或者有更新
public class PictureCompareTool : Editor {
    public const string TargetPath = "/Editor/PicFile.txt";
    public static List<string> _picPathSet = new List<string>();

    [MenuItem("MyEditor/测试用")]
    public static void TestList()
    {
        List<string> sd = new List<string>();
        sd.Add("111");
        sd.Add("111");
        Debug.Log(sd.Count);
        
    }

    [MenuItem("MyEditor/生成批量图片MD5文件")]
    public static void CreateMD5File()
    {
        var selectDirectory = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(selectDirectory);

        Debug.Log(path);

        _picPathSet.Clear();

        BuildMD5VersionByPath(path, Application.dataPath + TargetPath);

        _picPathSet.Clear();
    }
    
    [MenuItem("MyEditor/进行图片资源对比")]
    public static void CompareContent()
    {
        string path = "";

        path = EditorUtility.OpenFolderPanel("选择Art文件目录", path, "");

        bool sureMark = false;
        string comPath = Path.Combine(path, "PicFile.txt");
        if (!File.Exists(comPath))
        {
            Debug.LogWarning("对比文件不存在");
            sureMark = EditorUtility.DisplayDialog("注意", "是否确定为该文件夹该文件夹为图片资源合集", "OK", "Cancel");
        }

        if (!sureMark) return;

        CollectAllFilesPath(path);

        Dictionary<string, string> dicNameMD5 = new Dictionary<string, string>();

        for (int i = 0; i < _picPathSet.Count; ++i)
        {
            EditorUtility.DisplayProgressBar("正在计算图片MD5值", _picPathSet[i], (float)i / (float)_picPathSet.Count);

            string md5 = CommonAlgorithm.md5file(_picPathSet[i]);
            string name = Path.GetFileName(_picPathSet[i]);

            if (dicNameMD5.ContainsKey(name))
            {
                dicNameMD5[name] = dicNameMD5[name] + '/' + md5;
            }
            else
            {
                dicNameMD5.Add(name, md5);
            }
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

        string tips = string.Format("有%d个图片存在差异，是否进行替换",diffSet.Count);
        if (sureMark = EditorUtility.DisplayDialog("注意", tips, "ok", "cancel"))
        {

        }
    }
    /*
    [MenuItem("MyEditor/对比内容")]
    public static void CompareContent()
    {
        string path = "";

        path = EditorUtility.OpenFolderPanel("选择Art文件目录",path,"");

        //var selectDirectory = Selection.activeObject;
        //var path = AssetDatabase.GetAssetPath(selectDirectory);
        bool sureMark = false;
        if (!File.Exists(Path.Combine(path , "PicFile.txt")))
        {
            Debug.LogWarning("对比文件不存在");
           sureMark =  EditorUtility.DisplayDialog("注意","该文件夹并非图片资源合集，或者丢失对比信息文件，是否确定为该文件夹","OK","Cancel");
        }

        if (!sureMark) return;


        List<string> filePathSet = new List<string>();
        List<string> md5Set = new List<string>();
        List<string> filePathSet2 = new List<string>();
        List<string> md5Set2 = new List<string>();
        List<string> diffSet = new List<string>();

        StreamReader sr = new StreamReader(path);

        while (!sr.EndOfStream)
        {
            filePathSet.Add(sr.ReadLine());
            md5Set.Add(sr.ReadLine());
        }

        sr.Close();

        string sourcePath = Application.dataPath + TargetPath;
        if (!File.Exists(sourcePath))
        {
            Debug.LogWarning("源文件不存在");
            return;
        }

        StreamReader sourceStream = new StreamReader(sourcePath);

        while (!sourceStream.EndOfStream)
        {
            filePathSet2.Add(sourceStream.ReadLine());
            md5Set2.Add(sourceStream.ReadLine());
        }

        sourceStream.Close();

        for (int i = 0; i < md5Set.Count; i++)
        {
            if (!string.Equals(md5Set[i], md5Set2[i]))
            {
                diffSet.Add(filePathSet[i]);
            }
        }

        for (int j = 0; j < diffSet.Count; ++j)
        {
            Debug.Log(diffSet[j]);
        }
    }
    */

    public static void BuildMD5VersionByPath(string sourceDirectory, string targetPath)
    {
        if (sourceDirectory == null || !Directory.Exists(sourceDirectory))
        {
            Debug.LogWarning("目录不存在");
            return;
        }

        CollectAllFilesPath(sourceDirectory);

        if (!File.Exists(targetPath))
        {
            File.Create(targetPath);
            AssetDatabase.Refresh();
        }

        StreamWriter sw = new StreamWriter(targetPath);

        Debug.Log("文件数量:" + _picPathSet.Count);


        for ( int i=0;i<_picPathSet.Count ;i++ )
        {
            EditorUtility.DisplayProgressBar("正在生成MD5信息文件", _picPathSet[i], (float)i / (float)_picPathSet.Count);
            sw.WriteLine(_picPathSet[i]);
            string md5 = CommonAlgorithm.md5file(_picPathSet[i]);
            sw.WriteLine(md5);
            sw.Flush();
        }
        EditorUtility.ClearProgressBar();

        sw.Close();

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
