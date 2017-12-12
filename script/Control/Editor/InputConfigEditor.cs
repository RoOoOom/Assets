using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(InputConfig))]
public class InputConfigEditor : Editor {
    public const string FoldPath = "/Resources/";
    public const float ButtonWidth = 50f;

    InputConfig _inputConfig;

    [MenuItem("MyEditor/CreateInputConfig")]
    public static void CreateInputConfig()
    {
        string path = Application.dataPath + FoldPath + InputConfig.ConfigName + ".asset";

        if (File.Exists(path))
        {
            Debug.LogWarning("不需要创建多个配置文件");
            return;
        }
        path = "Assets" + FoldPath + InputConfig.ConfigName + ".asset"; 

        InputConfig instance = CreateInstance<InputConfig>( );
        AssetDatabase.CreateAsset(instance, path);

        Debug.Log("成功创建配置文件");
    }

    private void OnEnable()
    {
        _inputConfig = (InputConfig)target;

        int len = _inputConfig.TryGetSetLength();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical( GUI.skin.box );

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("指令");
        EditorGUILayout.LabelField("键位");
        EditorGUILayout.EndHorizontal();

        int len = _inputConfig.TryGetSetLength() ;

        for ( int i = 0; i<len ;++i )
        {
            EditorGUILayout.BeginHorizontal();

            _inputConfig._keySet[i] = EditorGUILayout.TextField(_inputConfig._keySet[i]);
            // _inputConfig._valueSet[i] = EditorGUILayout.TextField(_inputConfig._valueSet[i]);
            _inputConfig._valueSet[i] = (KeyCode)EditorGUILayout.EnumPopup(_inputConfig._valueSet[i]);

            if (GUILayout.Button("-",GUILayout.Width(ButtonWidth)))
            {
                RemoveKeyValue(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+",GUILayout.Width(ButtonWidth)))
        {
            AddKeyValue();
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Save", GUILayout.Width(ButtonWidth )))
        {
            SaveInputConfig();
        }

        if (GUILayout.Button("ReadData",GUILayout.Width(ButtonWidth*2)))
        {
            ReadData();
        }
    }

    /// <summary>
    /// 保存编辑界面上的数据到本地配置文件中
    /// </summary>
    void SaveInputConfig()
    {
        EditorUtility.SetDirty(_inputConfig);
        AssetDatabase.SaveAssets();

        
        string json =  JsonUtility.ToJson(InputConfig.Instance);
        string path = Application.dataPath + FoldPath + InputConfig.ConfigName + ".json";
        StreamWriter file = new StreamWriter(path);
        file.Write(json);
        file.Flush();
        file.Close();

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 增加一个指令键位词条
    /// </summary>
    void AddKeyValue()
    {
        _inputConfig.AddKeyValue("", KeyCode.Space);

    }

    /// <summary>
    /// 删除一个指令键位词条
    /// </summary>
    /// <param name="index"></param>
    void RemoveKeyValue(int index)
    {
        _inputConfig.RemoveKey(index);
    }


    void ReadData()
    {
        int len = _inputConfig.TryGetSetLength();

        for (int i = 0; i < len; ++i) 
        {
            string key = _inputConfig._keySet[i];
            Debug.Log("key : " + key + "  value:" + _inputConfig._valueSet[i]);
        }
    }
}
