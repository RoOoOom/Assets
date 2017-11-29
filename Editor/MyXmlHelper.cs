using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using UnityEditor;

public class NodeProperty
{
    string _nodeName;
    string _nodeAttribute;
    string _attrValue;
}

public class MyXmlHelper : EditorWindow {

    Type _type;
    List<NodeProperty> _nodeList;

    private void Awake()
    {
        _nodeList = new List<NodeProperty>();    
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("节点名");
        EditorGUILayout.LabelField("属性名");
        EditorGUILayout.LabelField("属性值");
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < _nodeList.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Button("+",GUILayout.Width(50f));

        EditorGUILayout.EndVertical();

        GUILayout.Button("Save", GUILayout.Width(100f));

    }

    void SaveXmlFormat()
    {

    }

    void AddNode()
    {

    }
}
