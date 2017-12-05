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
    public Texture2D _picture;

    private Vector2 _scrollPos = Vector2.zero ;
    public List<string> _picPathList;
    private List<PicShowInfo> _picInfoList;
    public Dictionary<string ,string[]> _dicOneToMany;

    /*
    private void OnGUI()
    {
        _picture = EditorGUILayout.ObjectField(_picture,typeof(Texture2D) ) as Texture2D;

        GUIStyle GS = new GUIStyle();
        GS.fixedHeight = 60;
        _scrollPos = EditorGUILayout.BeginScrollView( _scrollPos);
        if (_picture != null)
        {
            //  GUI.DrawTexture(new Rect(0, 270, 60, 60), _picture);
            for (int i = 0; i < 10; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Button(_picture);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("名称:123213");
                EditorGUILayout.LabelField("路径:easdfasdfasdf");
                EditorGUILayout.LabelField("大小:5KB");
                EditorGUILayout.EndVertical();

                GUILayout.Button(_picture);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("名称:5841621");
                EditorGUILayout.LabelField("路径:easdfasdfasdf");
                EditorGUILayout.LabelField("大小:5KB");
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
        }
        GUI.EndScrollView();
    }
    */

    private void OnGUI()
    {
        GUIStyle GS = new GUIStyle();
        GS.fixedHeight = 64;
        GS.fixedWidth = 64;
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,true,true);

        if (_picInfoList == null) return;

        for (int i = 0; i < _picInfoList.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            if (GUILayout.Button(_picInfoList[i]._tex2D, GS))
            {
                Debug.Log("xiwang ");
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
                    Debug.Log("xiwang ");
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("名称:" + _picInfoList[i]._name);
                GUILayout.Label("路径:" + _picInfoList[i]._nodes[j]._path);
                EditorGUILayout.LabelField("大小:" + _picInfoList[i]._nodes[j]._size);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }



        GUI.EndScrollView();
    }

    public void LoadPicture()
    {
        if (_picPathList == null || _dicOneToMany == null ) return;

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
        }

        _picPathList.Clear();
        _dicOneToMany.Clear();
        
    }
}
