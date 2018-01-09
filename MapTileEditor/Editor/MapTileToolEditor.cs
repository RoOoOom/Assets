using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapTileTool))]
public class MapTileToolEditor : Editor {
    private static bool m_startWork = false ;
    private MapTileTool m_mapTileTool;
    private int m_layer = 0;

    #region 限制选中场景中的其他对象
    [InitializeOnLoadMethod]
    static void Init()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
       Event e = Event.current;

       int controlID = GUIUtility.GetControlID(FocusType.Passive);

       if (e.type == EventType.Layout)
       {
           HandleUtility.AddDefaultControl(controlID);
       }
    }
    #endregion

    /// <summary>
    /// 限制鼠标点击选中scene视图中的对象
    /// </summary>
    /// <param name="sceneView"></param>
    void OnSceneGUI( )
    {
        
        Event e = Event.current;
        /*
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlID);
        }
        */
        if (!m_startWork) return;

        if (e.button == 0 && e.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray ,out hit , Mathf.Infinity , m_layer))
            {
                Debug.Log(hit.transform.gameObject.name);
                int r, c;
                m_mapTileTool.PositionToRowCol(hit.point,out r,out c);
                Debug.Log("row:" + r + "  col:" + c);
                m_mapTileTool.AddNewPointPosition(hit.point);
            }
        }
    }


    private void Awake()
    {
        m_mapTileTool = (MapTileTool)target;

        m_layer = 1 << LayerMask.NameToLayer("Ground");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = !m_startWork;

        if (GUILayout.Button("开始编辑", GUILayout.Height(20)))
        {   
            m_startWork = true;
        }

        GUI.enabled = m_startWork;

        if (GUILayout.Button("结束编辑", GUILayout.Height(20)))
        {
            m_mapTileTool.CalculateComfotalbeTile();
            m_startWork = false;
        }

        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;

        m_mapTileTool.m_orignTransform =(Transform)EditorGUILayout.ObjectField("原点",m_mapTileTool.m_orignTransform,typeof(Transform) , true);
        m_mapTileTool.TileGroup = (GameObject)EditorGUILayout.ObjectField("方格组" , m_mapTileTool.TileGroup , typeof(GameObject) , true);
        //m_mapTileTool.m_tile = (GameObject)EditorGUILayout.ObjectField("tile预制件",m_mapTileTool.m_tile , typeof(GameObject),true);

        m_mapTileTool.m_scale = EditorGUILayout.Slider( "方格大小" ,m_mapTileTool.m_scale , 0.5f , 5f );
        m_mapTileTool.m_range = EditorGUILayout.IntSlider("区域范围", m_mapTileTool.m_range, 1, 99);

        GUI.enabled = !m_startWork;

        if (GUILayout.Button("绘制地图方格" , GUILayout.Height(20)))
        {
            m_mapTileTool.DrawAllTileWithCenter();
        }

        if (GUILayout.Button("清除方格", GUILayout.Height(20)))
        {
            m_mapTileTool.ClearAllTile();
        }

        if (GUILayout.Button("清理点", GUILayout.Height(20)))
        {
            m_mapTileTool.ClearAllPoint();
        }
    }

  
}
