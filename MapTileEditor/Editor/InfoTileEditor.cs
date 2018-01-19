using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(InfoTileTool))]
public class InfoTileEditor : Editor {
    public const float StyleHeight = 30f;

    private static int m_selectIndex = 0;
    private static int m_selectArea = 0;
    private int m_layer;
    private float[] m_lengthArray = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
    private string[] m_lengtStrArray = { "100x100", "200x200", "300x300", "400x400", "500x500" };
    private string[] m_areaName = {"重置", "障碍区", "安全区", "传说区", "怪物区", "出生区", "复活区", "采集区", "跳跃区"};

    InfoTileTool m_infoTileTool;

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

    void OnEnable()
    {
        m_infoTileTool = (InfoTileTool)target;
        m_layer = 1 << LayerMask.NameToLayer("Ground");
    }

    void OnSceneGUI()
    {
        Event e = Event.current;

        if (e.button == 0 && e.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_layer))
            {
                Debug.Log("点到方格");
                m_infoTileTool.SetSquareType( hit.transform.gameObject , m_selectArea );
            }
        }
    }

    public override void OnInspectorGUI()
    {
        BasicDataArea();
        DrawButtonArea();
        DrawFeaturesArea();
        SaveLoadArea();
    }

    void Destroy()
    {
        m_infoTileTool = null;
    }

    #region 摆设Inspeactor界面
    //基本数据设置区
    void BasicDataArea()
    {
        m_infoTileTool.m_proxy = (GameObject)EditorGUILayout.ObjectField("确定地面高度的对象" , m_infoTileTool.m_proxy , typeof(GameObject) , true);
        m_infoTileTool.m_tilePrefab = (GameObject)EditorGUILayout.ObjectField("展示效果的方格预制件", m_infoTileTool.m_tilePrefab, typeof(GameObject), true);

        m_selectIndex = EditorGUILayout.Popup("方格的大小(pixel)", m_selectIndex, m_lengtStrArray);
        m_infoTileTool.m_tilePixel = m_lengthArray[m_selectIndex];

        EditorGUILayout.BeginHorizontal();
        m_infoTileTool.m_RowCount = EditorGUILayout.IntField("行数",m_infoTileTool.m_RowCount);
        m_infoTileTool.m_ColCount = EditorGUILayout.IntField("列数",m_infoTileTool.m_ColCount);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    //绘制相关区
    void DrawButtonArea()
    {
        EditorGUILayout.BeginHorizontal();
        m_infoTileTool.m_showTileArea = EditorGUILayout.Toggle( "绘制方格区域" ,m_infoTileTool.m_showTileArea );
        m_infoTileTool.m_showWireCube = EditorGUILayout.Toggle("绘制线条方格", m_infoTileTool.m_showWireCube);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create Tiles", GUILayout.Height(StyleHeight)))
        {
            m_infoTileTool.CreateTiles();
        }

        EditorGUILayout.Space();
    }

    //编辑功能区
    void DrawFeaturesArea()
    {
        EditorGUILayout.Space();
        /*
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("重置",GUILayout.Height(StyleHeight)))
        {

        }
        if (GUILayout.Button("阻碍区", GUILayout.Height(StyleHeight)))
        {

        }

        if (GUILayout.Button("安全区", GUILayout.Height(StyleHeight)))
        {

        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("传送区", GUILayout.Height(StyleHeight)))
        {

        }
        if (GUILayout.Button("怪物区", GUILayout.Height(StyleHeight)))
        {

        }

        if (GUILayout.Button("采集区", GUILayout.Height(StyleHeight)))
        {

        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("复活区", GUILayout.Height(StyleHeight)))
        {

        }
        if (GUILayout.Button("跳跃区", GUILayout.Height(StyleHeight)))
        {

        }
        EditorGUILayout.EndHorizontal();
        */

        m_selectArea = GUILayout.SelectionGrid( m_selectArea , m_areaName , 3 );

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    //保存载入区 还有清理
    void SaveLoadArea()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Export NavMeshData", GUILayout.Height(StyleHeight)))
        {
            ExportNavMesh();  
            //ExportNavMeshTest();
        }
        if (GUILayout.Button("Load NavMeshData", GUILayout.Height(StyleHeight)))
        {
            LoadNavMeshData(out m_infoTileTool.m_navMeshVertices ,out m_infoTileTool.m_navMeshIndcies );
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save MapTileData" , GUILayout.Height(StyleHeight)))
        {
           
        }

        if (GUILayout.Button("Load MapTileData", GUILayout.Height(StyleHeight)))
        {
            string path = EditorUtility.OpenFilePanel("选择地图块数据", Application.dataPath , "td");
            Debug.Log("确认路径:" + path);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Apart", GUILayout.Height(StyleHeight)))
        {
            m_infoTileTool.ClearApart();
        }

        if (GUILayout.Button("Clear All", GUILayout.Height(StyleHeight)))
        {
            m_infoTileTool.ClearAllData();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
    #endregion
    
    #region 导出，保存，载入数据相关的方法
    //测试用
    void ExportNavMeshTest()
    {
        NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();
        
        Mesh mesh = new Mesh();
        mesh.name = "_NavMesh";
        mesh.vertices = triangulatedNavMesh.vertices;
        mesh.triangles = triangulatedNavMesh.indices;

        GameObject test = new GameObject("TestMesh");
        MeshFilter mf = test.AddComponent<MeshFilter>();
        test.AddComponent<MeshRenderer>();
        mf.mesh = mesh;

        

        for (int i = 0; i < mesh.vertices.Length; ++i)
        {
            GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sp.transform.position = test.transform.TransformPoint(mesh.vertices[i]);
            sp.transform.parent = test.transform;
        }

    }

    //导出NavMesh的数据
    void ExportNavMesh()
    {
        NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();

        Mesh mesh = new Mesh();
        mesh.name = "_NavMesh";
        mesh.vertices = triangulatedNavMesh.vertices;
        mesh.triangles = triangulatedNavMesh.indices;

        string baseName = "navmesh_test";
        string fileName = Application.dataPath + "/navmesh/" + baseName + ".obj";
        string txtName = Application.dataPath + "/navmesh/" + baseName + ".txt";
        NavMeshToMeshObj(mesh, fileName);
        NavMeshToBinaryFile(mesh ,txtName);
        AssetDatabase.Refresh();
        /*
        string assetName = fileName.Replace(Application.dataPath, "Assets");
        GameObject navMesh = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(assetName));
        navMesh.name = baseName;
        MeshObjToBinaryFile(navMesh);
        */
        Debug.Log("导出完成：" + baseName);
        AssetDatabase.Refresh();
    }

    //把导航网格转成普通的网格对象，保存到指定路径
    void NavMeshToMeshObj(Mesh mesh , string fileName)
    {
        
            StringBuilder sb = new StringBuilder();
            sb.Append("g ").Append(mesh.name).Append("\n");

            foreach ( Vector3 vertex in mesh.vertices )
            {
                sb.AppendLine(string.Format("v {0} {1} {2}",vertex.x , vertex.y , vertex.z));
            }
            sb.Append("\n");
             /*   
            foreach ( Vector3 normal in mesh.normals )
            {
                sb.AppendLine(string.Format("vn {0} {1} {2}", normal.x, normal.y, normal.z));
            }
            sb.Append("\n");

            foreach ( Vector3 uv in mesh.uv )
            {
                sb.AppendLine(string.Format("vt {0} {1}", uv.x, uv.y));
            }

            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                sb.Append("\n");
                int[] triangles = mesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j+=3)
                {
                // sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", triangles[j] + 1, triangles[j + 1] + 1, triangles[j + 2] + 1));
                    sb.AppendLine(string.Format("f {0} {1} {2}", triangles[j] + 1, triangles[j + 1] + 1, triangles[j + 2] + 1));
                }
            }
            */
            for (int j = 0; j < mesh.triangles.Length; j+=3)
            {
                sb.AppendLine(string.Format("f {0} {1} {2}", mesh.triangles[j] + 1, mesh.triangles[j + 1] + 1, mesh.triangles[j + 2] + 1));
            }
            

        using (StreamWriter sw = new StreamWriter(fileName))
        {
            sw.Write(sb.ToString());
        }
            

    }

    //把网格对象的数据转成二进制文件保存
    void NavMeshToBinaryFile(Mesh mesh ,string fileName)
    {
        StringBuilder sb = new StringBuilder();

        foreach (Vector3 vertex in mesh.vertices)
        {
            sb.AppendLine(string.Format("{0},{1},{2}", vertex.x, vertex.y, vertex.z));
        }

        using (StreamWriter sw = new StreamWriter(fileName))
        {
            sw.Write(sb.ToString());
        }
    }

    bool LoadNavMeshData( out Vector2[] triangles , out int[] indices )
    {
        Debug.Log("开始载入NavMesh数据");

        string path = EditorUtility.OpenFilePanel("选择导航网格数据", Application.dataPath, "obj");

        string assetName = path.Replace(Application.dataPath, "Assets");
        GameObject navMesh = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(assetName));
        Vector3 eulr = navMesh.transform.eulerAngles;
        eulr.z = -180;
        navMesh.transform.eulerAngles = eulr;

        MeshFilter meshFilter = navMesh.GetComponentInChildren<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError("对象不存在网格组件");
            indices = new int[0];
            triangles = new Vector2[0];
            return false;
        }

        Vector3[] localVertices = meshFilter.sharedMesh.vertices;
        indices = new int[meshFilter.sharedMesh.triangles.Length];
        for (int j = 0; j < meshFilter.sharedMesh.triangles.Length; j++)
        {
            indices[j] = meshFilter.sharedMesh.triangles[j];
        }
 

        //把mesh的本地坐标转成世界坐标,忽略y轴，把3D转2D
        triangles = new Vector2[localVertices.Length];
        for (int i = 0; i < localVertices.Length; ++i)
        {
            //Vector3 pos = navMesh.transform.TransformPoint(localVertices[i]);
            Vector3 pos = localVertices[i];

            GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 tranPos = navMesh.transform.TransformPoint(pos);
            sp.transform.position = tranPos;
            sp.transform.parent = navMesh.transform;

            triangles[i] = new Vector2(tranPos.x , tranPos.z);
        }
        Debug.Log("成功载入，顶点数为:" + triangles.Length);
        return true;
    }

    #endregion
}
