using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;

public class AppendSDKWindow : EditorWindow {
    public bool _isLoad = false;
    XmlDocument _xmlDoc = new XmlDocument();
    XmlElement _root;
    XmlNodeList _xmlNodeList;

    string _path = "";
    List<string> _idSet = new List<string>();
    List<string> _folderSet = new List<string>();

    private void OnGUI()
    {
        if (!_isLoad)
        {
            LoadXML();
            InitSet();

            _isLoad = true;
        }

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("SDK id");
        EditorGUILayout.LabelField("文件夹名");
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < _idSet.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            _idSet[i] = EditorGUILayout.TextField(_idSet[i]);
            _folderSet[i] = EditorGUILayout.TextField(_folderSet[i]);
            EditorGUILayout.EndHorizontal();
        }


        if (GUILayout.Button("+", GUILayout.Width(50f)))
        {
            AppendNode();
        }

        if (GUILayout.Button("Save", GUILayout.Width(150f)))
        {
            SaveXML();
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 读取xml文件
    /// </summary>
    void LoadXML()
    {
        _path = Application.dataPath + AppendSDKEditor.XMLpath;

        if (!File.Exists(_path))
        {
            XmlDeclaration dex = _xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlDoc.AppendChild(dex);

            _root = _xmlDoc.CreateElement(AppendSDKEditor.RootName);
            XmlElement sdkNode = _xmlDoc.CreateElement(AppendSDKEditor.NodeKey);
            sdkNode.SetAttribute(AppendSDKEditor.NodeAttr, "");
            _root.AppendChild(sdkNode);
            _xmlDoc.AppendChild(_root);
            _xmlDoc.Save(_path);
        }
        else
        {
            _xmlDoc.Load(_path);
            _root = _xmlDoc.DocumentElement;

            _xmlNodeList = _root.GetElementsByTagName(AppendSDKEditor.NodeKey);
        }
        
    }

    /// <summary>
    /// 读取xml文件的节点数据
    /// </summary>
    void InitSet()
    {
        if (_xmlNodeList != null && !_isLoad)
        {
            foreach (XmlElement xmlNode in _xmlNodeList)
            {
                string sdkid = xmlNode.GetAttribute(AppendSDKEditor.NodeAttr);
                string folder = xmlNode.InnerText;
                _idSet.Add(sdkid);
                _folderSet.Add(folder);
            }
        }
    }


    void SaveXML()
    {
        int i = 0;
        foreach (XmlElement xmlNode in _xmlNodeList)
        {
            xmlNode.SetAttribute(AppendSDKEditor.NodeAttr , _idSet[i]);
            xmlNode.InnerText = _folderSet[i];

            i++;
        }

        _xmlDoc.Save(_path);
    }

    /// <summary>
    /// 更新单个节点的数据
    /// </summary>
    /// <param name="selectName"></param>
    /// <param name="index"></param>
    void UpdateXmlNode( string selectName , int index )
    {
        XmlNode xmlNode = _root.SelectSingleNode(selectName);

        ((XmlElement)xmlNode).SetAttribute(AppendSDKEditor.NodeAttr, _idSet[index]);
        xmlNode.InnerText = _folderSet[index];

        _xmlDoc.Save(_path);
    }

    /// <summary>
    /// 新增节点
    /// </summary>
    void AppendNode()
    {

        XmlElement newNode = _xmlDoc.CreateElement(AppendSDKEditor.NodeKey);
        newNode.SetAttribute(AppendSDKEditor.NodeAttr, "");
        _root.AppendChild(newNode);
        _xmlDoc.Save(_path);

        _xmlNodeList = _root.GetElementsByTagName(AppendSDKEditor.NodeKey);

        _idSet.Add("");
        _folderSet.Add("");

        Debug.Log("更新后的数量：" + _xmlNodeList.Count);
    }
}
