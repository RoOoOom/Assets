using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class PicShowInfo
{
    public string _path;
    public string _name;
    public int _size;
    public Texture2D _tex2D;
    public SmallPicShowInfo[] _nodes;
}

public class SmallPicShowInfo
{
    public string _path;
    public int _size;
    public Texture2D _tex2D;
}

public class PictureCompareWindow : EditorWindow {
    public const int PicBtnWidth = 64;
    public const int PicBtnHeight = 64;
    public Texture2D _gouIcon;
    public Texture2D _crossIcon;

    private string _searchContent ;
    private Vector2 _scrollPos = Vector2.zero ;
    private List<bool> _insteadMarkList;
    public List<string> _picPathList;
    private List<PicShowInfo> _picInfoList;
    public Dictionary<string ,string[]> _dicOneToMany;
    
    private void OnGUI()
    {
        GUIStyle GS = new GUIStyle();
        GS.fixedHeight = 64;
        GS.fixedWidth = 64;

        _searchContent = EditorGUILayout.TextField("搜索",_searchContent);

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,true,true);

        if (_picInfoList == null) return;
        bool operationSearch = false;
        if (_searchContent != null && !_searchContent.Equals(""))
        {
            operationSearch = true;
            _searchContent = _searchContent.ToLower();
        }

        for (int i = 0; i < _picInfoList.Count; ++i)
        {
            if (operationSearch)
            {
                string nameToLower = _picInfoList[i]._name.ToLower();
                if (!nameToLower.Contains(_searchContent))
                {
                    continue;
                }
            }

            EditorGUILayout.BeginHorizontal(GUI.skin.box);  //level_1
            if (GUILayout.Button(_picInfoList[i]._tex2D, GS))
            {
                try {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                    psi.Arguments = "/e,/select," + _picInfoList[i]._path;
                    System.Diagnostics.Process.Start(psi);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("名称:" + _picInfoList[i]._name);
            GUILayout.Label("路径:" + _picInfoList[i]._path);
            EditorGUILayout.LabelField("大小:" + _picInfoList[i]._size);
            EditorGUILayout.EndVertical();

            

            EditorGUILayout.BeginVertical();
            for ( int j = 0;j<_picInfoList[i]._nodes.Length ;++j )
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(_picInfoList[i]._nodes[j]._tex2D, GS))
                {
                    try
                    {
                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                        psi.Arguments = "/e,/select," + _picInfoList[i]._nodes[j]._path;
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("名称:" + _picInfoList[i]._name);
                GUILayout.Label("路径:" + _picInfoList[i]._nodes[j]._path);
                EditorGUILayout.LabelField("大小:" + _picInfoList[i]._nodes[j]._size);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(); //level_6
            GUILayout.Label("是否已替换");
            if (GUILayout.Button(_insteadMarkList[i] ? _gouIcon : _crossIcon))
            {
                _insteadMarkList[i] = !_insteadMarkList[i];
            }

            EditorGUILayout.EndVertical();   //level_6

            EditorGUILayout.EndHorizontal();//level_1

            EditorGUILayout.Space();
        }



        GUI.EndScrollView();
    }

    //关闭对比窗口后会将修改过的对比结果保存到文件中。
    private void OnDestroy()
    {
        List<string> diffContent = new List<string>();
        StreamReader sr = new StreamReader(Application.dataPath + PictureCompareTool.DifferentFilePath);
        while(!sr.EndOfStream)
        {
            diffContent.Add(sr.ReadLine());
        }
        sr.Close();
        int n = 0;
        for (int i = 0; i < diffContent.Count; ++i)
        {
            if (diffContent[i].EndsWith("0") || diffContent[i].EndsWith("1"))
            {
                char[] charArray = diffContent[i].ToCharArray();
                charArray[diffContent[i].Length - 1] = _insteadMarkList[n] ? '1' : '0';
                diffContent[i] = new string(charArray);
                n++;
            }
        }

        StreamWriter sw = new StreamWriter(Application.dataPath + PictureCompareTool.DifferentFilePath);

        for ( int i = 0; i<diffContent.Count ;++i )
        {
            sw.WriteLine( diffContent[i]);
            sw.Flush();
        }
        sw.Dispose();

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 加载差异信息文件
    /// </summary>
    /// <param name="path">差异信息文件路径</param>
    public void LoadDifferentFile( string path )
    {
        _picPathList = new List<string>();
        _dicOneToMany = new Dictionary<string, string[]>();
        _insteadMarkList = new List<bool>();

        StreamReader sr = new StreamReader(path);
        while (!sr.EndOfStream)
        {
            string[] picInfo = sr.ReadLine().Split(PictureCompareTool.SplitChar);

            if (picInfo.Length < 3) continue;
                
            _picPathList.Add(picInfo[0]);

            string[] nodeInfo = new string[int.Parse(picInfo[1])];

            for (int i = 0; i < nodeInfo.Length; ++i)
            {
                nodeInfo[i] = sr.ReadLine();
            }
            _dicOneToMany.Add(picInfo[0], nodeInfo);

            _insteadMarkList.Add(picInfo[2].Equals("0") ? false :true);
        }
        sr.Close();
        sr.Dispose();
        
    }
  
    /// <summary>
    /// 根据提供的路径展示图片
    /// </summary>
    public void LoadPicture(bool recompare = true )
    {
        if (_picPathList == null || _dicOneToMany == null ) return;

        if (_insteadMarkList == null) _insteadMarkList = new List<bool>();

        _picInfoList = new List<PicShowInfo>();

        for (int i = 0; i < _picPathList.Count; ++i)
        {
            FileStream fs = new FileStream(_picPathList[i], FileMode.Open, FileAccess.Read);

            byte[] bytes = new byte[fs.Length];

            fs.Read(bytes, 0, (int)fs.Length);

            fs.Close();
            fs.Dispose();
            fs = null;

            Texture2D temp = new Texture2D(PicBtnWidth, PicBtnHeight);
            temp.LoadImage(bytes);

            PicShowInfo psi = new PicShowInfo();
            psi._path = _picPathList[i];
            psi._name = Path.GetFileName(_picPathList[i]);
            psi._size = bytes.Length;
            psi._tex2D = temp;

            string[] pathArray;
            _dicOneToMany.TryGetValue( psi._path , out pathArray);

            psi._nodes = new SmallPicShowInfo[pathArray.Length];

            for (int j = 0; j < pathArray.Length; ++j)
            {
                SmallPicShowInfo tempInfo = new SmallPicShowInfo();

                FileStream filestream = new FileStream(pathArray[j], FileMode.Open, FileAccess.Read);

                byte[] bytesTmp = new byte[filestream.Length];

                filestream.Read(bytesTmp, 0, (int)filestream.Length);

                filestream.Close();
                filestream.Dispose();
                filestream = null;

                Texture2D tempTex = new Texture2D(PicBtnWidth, PicBtnHeight);
                tempTex.LoadImage(bytesTmp);

                tempInfo._path = pathArray[j];
                tempInfo._size = bytesTmp.Length;
                tempInfo._tex2D = tempTex;

                psi._nodes[j] = tempInfo;
            }

            _picInfoList.Add(psi);

            if (recompare) _insteadMarkList.Add(!recompare);
        }

        _picPathList.Clear();
        _dicOneToMany.Clear();
        
    }
}
