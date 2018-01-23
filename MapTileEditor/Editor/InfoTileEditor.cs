using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(InfoTileTool))]
public class InfoTileEditor : Editor {
    public const int BarrierArea = 0;
    public const int NormalArea = 1;
    public const int SafeArea = 2;
    public const int PortolArea = 3;
    public const int MonsterArea = 4;
    public const int BornArea = 5;
    public const int RebornArea = 6;
    public const int CollectArea = 7;
    public const int JumpArea = 8;

    public const float StyleHeight = 30f;

    private static GameObject m_navMeshObj;
    private static bool m_startEditorWork = false;
    private static int m_selectIndex = 0;
    private static int m_selectAreaIndex = 0;  //正在编辑的区域
    private int m_selectArea = 0;//区域的具体类型值
    private int m_layer;
    private float[] m_lengthArray = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
    private string[] m_lengtStrArray = { "100x100", "200x200", "300x300", "400x400", "500x500" };
    private string[] m_areaName = { "障碍区", "重置", "安全区", "传送区", "怪物区", "出生区", "复活区", "采集区", "跳跃区" };
    private int[] m_areaNumber = { 0, 1, 2, 4, 8, 16, 32, 64, 128 };

    private Vector2 m_AreaViewPos = Vector2.zero;
    private Vector2 m_AreaPointViewPos = Vector2.zero;

    InfoTileTool m_infoTileTool;
    [MenuItem("Tools/打开地图块编辑工具")]
    public static void OpenlMapInfoTool()
    {
        GameObject go = new GameObject("MapInfoEditor");
        go.AddComponent<InfoTileTool>();
    }
    
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
    

    // 焦点转移到编辑窗口
    private void FocusEditPanel()
    {
        if (SceneView.sceneViews.Count > 0)
        {
            SceneView myView = (SceneView)SceneView.sceneViews[0];
            myView.Focus();
        }
    }

    void OnEnable()
    {
        m_infoTileTool = (InfoTileTool)target;
        m_layer = 1 << LayerMask.NameToLayer("Ground");
    }

    #region 编辑区域相关的方法
    void OnSceneGUI()
    {
        Event e = Event.current;

        if (m_startEditorWork)
        {
            /*
            if (e.button == 1 && e.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_layer))
                {
                    m_infoTileTool.SetSquareType(hit.transform.gameObject, m_selectArea);
                }
            }*/
            /*
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_layer))
                {
                    m_infoTileTool.SetSelectStartPoint(hit.point);
                }
            }
            else if (e.button == 0 && e.type == EventType.MouseUp)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_layer))
                {
                    m_infoTileTool.BoxSelection(hit.point, m_selectArea);
                }
            }*/
            /*
            if (m_selectAreaIndex == BarrierArea)
            {

            }
            if (m_selectAreaIndex == BornArea)
            {

            }
            if (m_selectAreaIndex == CollectArea)
            {

            }

            if (m_selectAreaIndex == JumpArea)
            {

            }

            if (m_selectAreaIndex == MonsterArea)
            {

            }

            if (m_selectAreaIndex == NormalArea)
            {

            }

            if (m_selectAreaIndex == PortolArea)
            {

            }

            if (m_selectAreaIndex == RebornArea)
            {

            }

            if (m_selectAreaIndex == SafeArea)
            {

            }*/
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_layer))
                {
                    EditSelectSquare square = m_infoTileTool.TryGetSubArea(m_selectAreaIndex);
                    if(square!=null)
                        square.AddPoint(CreatePoint(hit.point, square.parent.transform));
                }
            }
        }
        
    }

    private void EditSafeArea()
    {
        Event e = Event.current;

        if (e.button == 0 && e.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_layer))
            {
                EditSelectSquare square = m_infoTileTool.TryGetSubArea(m_selectAreaIndex);
                square.AddPoint(CreatePoint(hit.point, square.parent.transform));
            }
        }
    }

    private GameObject CreatePoint( Vector3 pos , Transform parent)
    {
        GameObject go = new GameObject("point(" + pos + ")");
        go.transform.position = pos;
        go.transform.parent = parent.transform;
        return go;
    }
    #endregion

    public override void OnInspectorGUI()
    {
        BasicDataArea();
        DrawButtonArea();
        DrawFeaturesArea();
        ShowAllSquareArea();

        GUI.enabled = true;
        SaveLoadArea();
        ExportArea();
    }

    void OnDisable()
    {
        if (m_startEditorWork)
        {
            if (m_infoTileTool.gameObject != null)
            {
                Selection.activeGameObject = m_infoTileTool.gameObject;

                //Debug.Log("请先关闭编辑状态(点击FinishArea按钮)，才能选择别的对象");
            }
        }
    }

    void Destroy()
    {
        m_infoTileTool = null;
    }

    #region 摆设Inspeactor界面
    //基本数据设置区
    void BasicDataArea()
    {
        //m_infoTileTool.m_proxy = (GameObject)EditorGUILayout.ObjectField("确定地面高度的对象" , m_infoTileTool.m_proxy , typeof(GameObject) , true);
        m_infoTileTool.m_curY = EditorGUILayout.Slider("预估地图块高度", m_infoTileTool.m_curY , -10f, 10f );
        m_infoTileTool.CheckYChange();

        //m_infoTileTool.m_tilePrefab = (GameObject)EditorGUILayout.ObjectField("展示效果的方格预制件", m_infoTileTool.m_tilePrefab, typeof(GameObject), true);

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
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("开始编辑工作" , GUILayout.Height(30)))
            {
                m_startEditorWork = true;
            }

            if (GUILayout.Button("结束编辑工作" ,GUILayout.Height(30)))
            {
                m_startEditorWork = false;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.enabled = m_startEditorWork;
        m_selectAreaIndex = GUILayout.SelectionGrid( m_selectAreaIndex , m_areaName , 3 );
        m_selectArea = m_areaNumber[m_selectAreaIndex];

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
            SaveTilesData();
        }

        if (GUILayout.Button("Load MapTileData", GUILayout.Height(StyleHeight)))
        {
            LoadTilesData();
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
            if (m_navMeshObj)
            {
                DestroyImmediate(m_navMeshObj);
                m_navMeshObj = null;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    void ExportArea()
    {
        if (GUILayout.Button("导出单个地图数据", GUILayout.Height(StyleHeight)))
        {
            string path = EditorUtility.OpenFilePanel("选择地图块数据", null, "td");
            if (path.Equals(""))
            {
                return;
            }

            string savePath = EditorUtility.SaveFilePanel("保存服务器用的数据文件", null, "map_data","");
            if (savePath.Equals(""))
            {
                return;
            }
            m_infoTileTool.m_isExportProcess = true;
            ExportSingleMapInfo(path, savePath);

            string dirName = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            DirectoryInfo info = new DirectoryInfo(dirName);

            m_infoTileTool.WriteClientCommonFile(info, savePath);
            EditorUtility.DisplayDialog("提示", "信息保存成功", "OK");
            m_infoTileTool.m_isExportProcess = false;
        }

        if (GUILayout.Button("导出所有地图数据", GUILayout.Height(StyleHeight)))
        {
            string path = EditorUtility.OpenFolderPanel("选择所有地图数据所在的文件夹",null,null);
            if (path.Equals(""))
            {
                return;
            }

            string savePath = EditorUtility.SaveFolderPanel("选择保存目录", null, null);
            if ( savePath.Equals("") )
            {
                return;
            }
            m_infoTileTool.m_isExportProcess = true;
            ExportAllMapInfo( path , savePath );
            m_infoTileTool.m_isExportProcess = false;
        }
    }
    #endregion

    #region 各个子区及其组成的点
    private void ShowAllSquareArea()
    {

        if (m_selectAreaIndex == BarrierArea)
        {
            ShowSquareArea(m_selectAreaIndex, "障碍区");
            ShowSquarePoint(m_selectAreaIndex, "障碍区");
        }
        if (m_selectAreaIndex == BornArea)
        {
            ShowSquareArea(m_selectAreaIndex, "出生区");
            ShowSquarePoint(m_selectAreaIndex, "出生区");
        }
        if (m_selectAreaIndex == CollectArea)
        {
            ShowSquareArea(m_selectAreaIndex, "采集区");
            ShowSquarePoint(m_selectAreaIndex, "采集区");
        }

        if (m_selectAreaIndex == JumpArea)
        {
            ShowSquareArea(m_selectAreaIndex, "跳跃区");
            ShowSquarePoint(m_selectAreaIndex, "跳跃区");
        }

        if (m_selectAreaIndex == MonsterArea)
        {
            ShowSquareArea(m_selectAreaIndex, "怪物区");
            ShowSquarePoint(m_selectAreaIndex, "怪物区");
        }

        if (m_selectAreaIndex == NormalArea)
        {

        }

        if (m_selectAreaIndex == PortolArea)
        {
            ShowSquareArea(m_selectAreaIndex, "传送区");
            ShowSquarePoint(m_selectAreaIndex, "传送区");
        }

        if (m_selectAreaIndex == RebornArea)
        {
            ShowSquareArea(m_selectAreaIndex, "复活区");
            ShowSquarePoint(m_selectAreaIndex, "复活区");
        }

        if (m_selectAreaIndex == SafeArea)
        {
            ShowSquareArea(m_selectAreaIndex, "安全区");
            ShowSquarePoint(m_selectAreaIndex, "安全区");
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }


    private void ShowSquareArea(int areaIndex , string areaName)
    {
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Label(areaName);
            m_AreaViewPos = EditorGUILayout.BeginScrollView(m_AreaViewPos,GUILayout.Height(100));
            {
                List<string> lst = new List<string>();
                int count = m_infoTileTool.TryGetAreaCount(areaIndex);
                for (int i = 0; i < count; i++)
                {
                    lst.Add(areaName + ' ' + i.ToString());
                }

                switch (areaIndex)
                {
                    case BarrierArea:
                        m_infoTileTool.m_selBarrierArea = GUILayout.SelectionGrid(m_infoTileTool.m_selBarrierArea, lst.ToArray(), 1);
                        break;
                    case SafeArea:
                        m_infoTileTool.m_selSafeArea = GUILayout.SelectionGrid(m_infoTileTool.m_selSafeArea, lst.ToArray(), 1);
                        break;
                    case PortolArea:
                        m_infoTileTool.m_selPortolArea = GUILayout.SelectionGrid(m_infoTileTool.m_selPortolArea, lst.ToArray(), 1);
                        break;
                    case MonsterArea:
                        m_infoTileTool.m_selMonsterArea = GUILayout.SelectionGrid(m_infoTileTool.m_selMonsterArea, lst.ToArray(), 1);
                        break;
                    case BornArea:
                        m_infoTileTool.m_selBornArea = GUILayout.SelectionGrid(m_infoTileTool.m_selBornArea, lst.ToArray(), 1);
                        break;
                    case RebornArea:
                        m_infoTileTool.m_selRebornArea = GUILayout.SelectionGrid(m_infoTileTool.m_selRebornArea, lst.ToArray(), 1);
                        break;
                    case CollectArea:
                        m_infoTileTool.m_selCollectArea = GUILayout.SelectionGrid(m_infoTileTool.m_selCollectArea, lst.ToArray(), 1);
                        break;
                    case JumpArea:
                        m_infoTileTool.m_selJumpArea = GUILayout.SelectionGrid(m_infoTileTool.m_selJumpArea, lst.ToArray(), 1);
                        break;
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("创建" + areaName))
                {
                    m_infoTileTool.CreateSubArea(areaIndex);
                }
                if (GUILayout.Button("删除" + areaName))
                {
                    m_infoTileTool.DeleteSubArea(areaIndex);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void ShowSquarePoint(int areaIndex, string areaName)
    {
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Label(areaName + "点");

            m_AreaPointViewPos = EditorGUILayout.BeginScrollView(m_AreaPointViewPos, GUILayout.Height(60));
            {
                List<string> lst = new List<string>();
                EditSelectSquare selectSquare = m_infoTileTool.TryGetSubArea(areaIndex);

                if (selectSquare != null)
                {
                    if (selectSquare.StartPoint != null)
                    {
                        lst.Add(GetPointName(areaName, selectSquare.StartPoint));
                    }
                    if (selectSquare.EndPoint != null)
                    {
                        lst.Add(GetPointName(areaName, selectSquare.EndPoint));
                    }


                    switch (areaIndex)
                    {
                        case BarrierArea:
                            m_infoTileTool.m_selBarrierPoint = GUILayout.SelectionGrid(m_infoTileTool.m_selBarrierPoint, lst.ToArray(), 1);
                            break;
                        case SafeArea:
                            m_infoTileTool.m_selSafePoint = GUILayout.SelectionGrid(m_infoTileTool.m_selSafePoint, lst.ToArray(), 1);
                            break;
                        case PortolArea:
                            m_infoTileTool.m_selPortolPoint = GUILayout.SelectionGrid(m_infoTileTool.m_selPortolPoint, lst.ToArray(), 1);
                            break;
                        case MonsterArea:
                            m_infoTileTool.m_selMonsterPoint = GUILayout.SelectionGrid(m_infoTileTool.m_selMonsterPoint, lst.ToArray(), 1);
                            break;
                        case BornArea:
                            m_infoTileTool.m_selBornPoint = GUILayout.SelectionGrid(m_infoTileTool.m_selBornPoint, lst.ToArray(), 1);
                            break;
                        case RebornArea:
                            m_infoTileTool.m_selRebornPoint = GUILayout.SelectionGrid(m_infoTileTool.m_selRebornPoint, lst.ToArray(), 1);
                            break;
                        case CollectArea:
                            m_infoTileTool.m_selCollectPoint = GUILayout.SelectionGrid(m_infoTileTool.m_selCollectPoint, lst.ToArray(), 1);
                            break;
                        case JumpArea:
                            m_infoTileTool.m_selJumpPoint = GUILayout.SelectionGrid(m_infoTileTool.m_selJumpPoint, lst.ToArray(), 1);
                            break;
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("删除" + areaName + "点"))
                {
                    m_infoTileTool.DeleteSubAreaPoint(areaIndex);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    public static string GetPointName(string preName, GameObject point)
    {
        return preName + "(" + point.transform.position.x + "," + point.transform.position.z + ")";
    }

    #endregion

    #region 导出，保存，载入NavMesh数据相关的方法
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

        string baseName = "navmesh_" + SceneManager.GetActiveScene().name;
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

    //载入提取的NavMesh数据
    bool LoadNavMeshData( out Vector2[] triangles , out int[] indices )
    {
        Debug.Log("开始载入NavMesh数据");
        indices = new int[0];
        triangles = new Vector2[0];

        string path = EditorUtility.OpenFilePanel("选择导航网格数据", Application.dataPath, "obj");
        if (path == null) return false;

        string assetName = path.Replace(Application.dataPath, "Assets");
        GameObject navMesh = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(assetName));
        m_navMeshObj = navMesh;
        Vector3 eulr = navMesh.transform.eulerAngles;
        eulr.z = -180;
        navMesh.transform.eulerAngles = eulr;

        MeshFilter meshFilter = navMesh.GetComponentInChildren<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError("对象不存在网格组件");
            
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

            //GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 tranPos = navMesh.transform.TransformPoint(pos);

            //sp.transform.position = tranPos;
            //sp.transform.parent = navMesh.transform;

            triangles[i] = new Vector2(tranPos.x , tranPos.z);
        }
        Debug.Log("成功载入，顶点数为:" + triangles.Length);
        return true;
    }

    //保存td格式的地图数据
    void SaveTilesData()
    {
       string path = EditorUtility.SaveFilePanel("保存地图数据",null,"maptile","td");
        if (path.Equals(""))
        {
            return;
        }
        Debug.Log(path);
        m_infoTileTool.SaveMapTileData(path);
        //m_infoTileTool.ExportMapInfoFile(path, InfoTileTool.FileType.Server);
    }

    //载入td格式的地图数据
    void LoadTilesData()
    {
        string path = EditorUtility.OpenFilePanel("选择地图块数据", null,"td");
        if (path.Equals(""))
        {
            return;
        }
        Debug.Log(path);
        m_infoTileTool.LoadMapTileData(path);
    }

    void ExportAllMapInfo( string filePath , string savePath )
    {
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
        DirectoryInfo info = new DirectoryInfo(filePath);

        FileInfo[] filesInfo = info.GetFiles();

        int i = 0;
        int count = filesInfo.Length;
        foreach (var file in filesInfo)
        {
            i++;
            if (file.Extension == ".td")
            {
                Debug.Log(file.FullName);
                string saveFile = savePath + "/" + file.Name;
                EditorUtility.DisplayProgressBar("正在导出数据", saveFile, i / count);
                ExportSingleMapInfo(file.FullName, saveFile);
            }
        }

        m_infoTileTool.WriteClientCommonFile(info, savePath + "/MapMask.lua");
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("提示", "信息保存成功", "OK");
    }

    //导出单个地图的数据
    void ExportSingleMapInfo( string filePath , string savePath )
    {
        
        m_infoTileTool.LoadMapTileData(filePath);        
        m_infoTileTool.ExportMapInfoFile(savePath, Path.GetFileNameWithoutExtension(savePath), InfoTileTool.FileType.Server);
        m_infoTileTool.ExportMapInfoFile(savePath, Path.GetFileNameWithoutExtension(savePath), InfoTileTool.FileType.Client);
        
    }
    #endregion
}
