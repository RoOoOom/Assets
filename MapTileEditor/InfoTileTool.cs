using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class InfoTileTool : MonoBehaviour {
    public enum FileType
    {
        Server,
        Client
    }

    public const int PixelScale = 100;

    #region 数据区
    public bool m_showTileArea = false;  //是否显示方格区
    public bool m_showWireCube = false;  //是否绘制空心的方格
    public bool m_isExportProcess = false; //是否为导出操作

    public int m_selTileIndex = 0;

    public float m_tilePixel; //方格大小
    public float m_mapWidth;
    public float m_mapHeight;

    public Vector2[] m_navMeshVertices; //导航网格的2D顶点数据
    public int[] m_navMeshIndcies; //导航网格绘制三角形的顶点索引

    public GameObject m_proxy; //用于确定地面高度的对象
    public GameObject m_tilePrefab;//展示效果的方格预制件
    public GameObject m_tileGroup; //所有tile的父对象

    public int m_RowCount; //行数
    public int m_ColCount; //列数

    public float m_curY = 0; //地面高度
    private float m_lastY = 0;

    private Vector3 m_selectStart;

    private List<Square> m_squareList = new List<Square>();  //收录可行走的地图块
    private Dictionary<GameObject, int> m_dicTileID = new Dictionary<GameObject, int>();  //对象-方格类型的映射表
    private Dictionary<int, List<Square>> m_dicTileList = new Dictionary<int, List<Square>>(); //方格类型-方格组的映射表

    public int m_selBarrierArea = 0; //当前选择障碍区的子区号
    public int m_selBarrierPoint = 0;
    public int m_selSafeArea = 0;    //当前选择的安全区的子区号
    public int m_selSafePoint = 0;
    public int m_selPortolArea = 0;     //当前选择的传送区的子区号
    public int m_selPortolPoint = 0;
    public int m_selBornArea = 0;       //当前选择的出生区的子区号
    public int m_selBornPoint = 0;
    public int m_selRebornArea = 0;     //当前选择的复活区的子区号
    public int m_selRebornPoint = 0;
    public int m_selMonsterArea = 0;       //当前选择的怪物区的子区号
    public int m_selMonsterPoint = 0;
    public int m_selCollectArea = 0;        //当前选择的采集区的子区号
    public int m_selCollectPoint = 0;
    public int m_selJumpArea = 0;       //当前选择的跳跃区的子区号
    public int m_selJumpPoint = 0;

    private GameObject m_barrierAreaParent;
    private GameObject m_safeAreaParent;
    private GameObject m_portolAreaParent;
    private GameObject m_bornAreaParent;
    private GameObject m_rebornAreaParent;
    private GameObject m_monsterAreaParent;
    private GameObject m_collectAreaParent;
    private GameObject m_jumpAreaParent;

    private List<EditSelectSquare> m_barrierAreaList = new List<EditSelectSquare>();
    private List<EditSelectSquare> m_safeAreaList = new List<EditSelectSquare>();
    private List<EditSelectSquare> m_portolAreaList = new List<EditSelectSquare>();
    private List<EditSelectSquare> m_bornAreaList = new List<EditSelectSquare>();
    private List<EditSelectSquare> m_rebornAreaList = new List<EditSelectSquare>();
    private List<EditSelectSquare> m_monsterAreaList = new List<EditSelectSquare>();
    private List<EditSelectSquare> m_collectAreaList = new List<EditSelectSquare>();
    private List<EditSelectSquare> m_jumpAreaList = new List<EditSelectSquare>();

    #endregion

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
    }

    #region 保存和载入已编辑好的地图数据
    public void SaveMapTileData(string path)
    {
        SetEverySquareType();

        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine(m_tilePixel.ToString());
        strBuilder.AppendLine(m_RowCount.ToString());
        strBuilder.AppendLine(m_ColCount.ToString());
        strBuilder.AppendLine(m_curY.ToString());
        strBuilder.AppendLine(m_squareList.Count.ToString());

        for (int i = 0; i < m_squareList.Count; ++i)
        {
            SimpleSquare strc;
            strc.id = m_squareList[i]._id;
            strc.row = m_squareList[i].Row;
            strc.col = m_squareList[i].Col;
            strc.type = m_squareList[i].GetAreaType();
            strc.topLeftPoint = m_squareList[i].TopLeftPoint;

            string json = JsonUtility.ToJson(strc);
            strBuilder.AppendLine(json);
        }

        
        AppendListToString(strBuilder, m_safeAreaList , Square.SAFE_TILE);
        AppendListToString(strBuilder, m_portolAreaList , Square.PORTOL_TILE);
        AppendListToString(strBuilder, m_monsterAreaList , Square.MONSTER_TILE);
        AppendListToString(strBuilder, m_bornAreaList, Square.BORN_TILE);
        AppendListToString(strBuilder, m_rebornAreaList, Square.REBORN_TILE);
        AppendListToString(strBuilder, m_collectAreaList, Square.COLLECT_TILE);
        AppendListToString(strBuilder, m_jumpAreaList, Square.JUMP_TILE);
        AppendListToString(strBuilder, m_barrierAreaList, Square.BARRIER_TILE);

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(strBuilder.ToString());
        }
    }

    //设置每一个方格的属性，障碍区一定要最后设置
    private void SetEverySquareType()
    {
        for (int i = 0; i < m_squareList.Count; i++)
        {
            SetSquareTypeWithList(m_squareList[i], m_safeAreaList, Square.SAFE_TILE);
            SetSquareTypeWithList(m_squareList[i], m_portolAreaList, Square.PORTOL_TILE);
            SetSquareTypeWithList(m_squareList[i], m_monsterAreaList, Square.MONSTER_TILE);
            SetSquareTypeWithList(m_squareList[i], m_bornAreaList, Square.BORN_TILE);
            SetSquareTypeWithList(m_squareList[i], m_rebornAreaList, Square.REBORN_TILE);
            SetSquareTypeWithList(m_squareList[i], m_collectAreaList, Square.COLLECT_TILE);
            SetSquareTypeWithList(m_squareList[i], m_jumpAreaList, Square.JUMP_TILE);
            SetSquareTypeWithList(m_squareList[i], m_barrierAreaList, Square.BARRIER_TILE);
        }
    }

    private void SetSquareTypeWithList(Square singleSqa, List<EditSelectSquare> editList, int type)
    {
        foreach (EditSelectSquare ess in editList)
        {
            if (ess.AllPoints.Count > 1)
            {
                int minRow, minCol, maxRow, maxCol;
                CalculateRowCol(ess.StartPoint.transform.position, out minRow, out minCol);
                CalculateRowCol(ess.EndPoint.transform.position, out maxRow, out maxCol);

                if (IsLegalRange(minRow, minCol) && IsLegalRange(maxRow, maxCol))
                {
                    if (minRow > maxRow) { minRow = maxRow + minRow; maxRow = minRow - maxRow; minRow = minRow - maxRow; }
                    if (minCol > maxCol) { minCol = maxCol + minCol; maxCol = minCol - maxCol; minCol = minCol - maxCol; }

                    if (minRow <= singleSqa.Row && maxRow >= singleSqa.Row && minCol <= singleSqa.Col && maxCol >= singleSqa.Col)
                    {
                        singleSqa.SetAreaType(type);
                    }
                }
            }
        }
    }

    private void AppendListToString(StringBuilder builder, List<EditSelectSquare> editSquareList , int sqaType)
    {
        foreach (EditSelectSquare ess in editSquareList)
        {
            if (ess.AllPoints.Count > 1)
            {
                AreaSquare areaSqa;
                areaSqa.type = sqaType;
                areaSqa.startPoint = ess.StartPoint.transform.position;
                areaSqa.endPoint = ess.EndPoint.transform.position;
                string json = JsonUtility.ToJson(areaSqa);
                builder.AppendLine(json);
            }
        }
    }

    //弃用
    private void SaveSimpleSquare(string path)
    {
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine(m_tilePixel.ToString());
        strBuilder.AppendLine(m_RowCount.ToString());
        strBuilder.AppendLine(m_ColCount.ToString());
        strBuilder.AppendLine(m_curY.ToString());
        strBuilder.AppendLine(m_squareList.Count.ToString());

        for (int i = 0; i < m_squareList.Count; ++i)
        {
            SimpleSquare strc;
            strc.id = m_squareList[i]._id;
            strc.row = m_squareList[i].Row;
            strc.col = m_squareList[i].Col;
            strc.type = m_squareList[i].GetAreaType();
            strc.topLeftPoint = m_squareList[i].TopLeftPoint;

            string json = JsonUtility.ToJson(strc);
            strBuilder.AppendLine(json);
        }

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(strBuilder.ToString());
        }
    }

    public void LoadMapTileData(string path)
    {
        InitData();

        ClearAreaSquare();

        InstancedAllAreaParent();
        
        using (StreamReader sr = new StreamReader(path))
        {
            m_tilePixel = float.Parse(sr.ReadLine());
            m_RowCount = int.Parse(sr.ReadLine());
            m_ColCount = int.Parse(sr.ReadLine());
            m_curY = float.Parse(sr.ReadLine());
            int cout = int.Parse(sr.ReadLine());
            int i = 0;
            while (i < cout)
            {
                SimpleSquare strc = JsonUtility.FromJson<SimpleSquare>(sr.ReadLine());
                Square sqa = new Square(strc.topLeftPoint, m_tilePixel);
                sqa.SetAreaType(strc.type);
                sqa._id = strc.id;
                sqa.Set3DPosition(m_curY);
                m_squareList.Add(sqa);
                i++;
            }

            while (!sr.EndOfStream)
            {
                AreaSquare areaSqa = JsonUtility.FromJson<AreaSquare>(sr.ReadLine());
                EditSelectSquare ess = new EditSelectSquare();
                switch (areaSqa.type)
                {
                    case Square.BARRIER_TILE:
                        ess.AddPoint(CreatePoint(areaSqa.startPoint,ref m_barrierAreaParent));
                        ess.AddPoint(CreatePoint(areaSqa.endPoint, ref m_barrierAreaParent));
                        m_barrierAreaList.Add(ess);
                        break;
                    case Square.SAFE_TILE:
                        ess.AddPoint(CreatePoint(areaSqa.startPoint, ref m_safeAreaParent));
                        ess.AddPoint(CreatePoint(areaSqa.endPoint, ref m_safeAreaParent));
                        m_safeAreaList.Add(ess);
                        break;
                    case Square.PORTOL_TILE:
                        ess.AddPoint(CreatePoint(areaSqa.startPoint, ref m_portolAreaParent));
                        ess.AddPoint(CreatePoint(areaSqa.endPoint, ref m_portolAreaParent));
                        m_portolAreaList.Add(ess);
                        break;
                    case Square.MONSTER_TILE:
                        ess.AddPoint(CreatePoint(areaSqa.startPoint, ref m_monsterAreaParent));
                        ess.AddPoint(CreatePoint(areaSqa.endPoint, ref m_monsterAreaParent));
                        m_monsterAreaList.Add(ess);
                        break;
                    case Square.BORN_TILE:
                        ess.AddPoint(CreatePoint(areaSqa.startPoint, ref m_bornAreaParent));
                        ess.AddPoint(CreatePoint(areaSqa.endPoint, ref m_bornAreaParent));
                        m_bornAreaList.Add(ess);
                        break;
                    case Square.REBORN_TILE:
                        ess.AddPoint(CreatePoint(areaSqa.startPoint, ref m_rebornAreaParent));
                        ess.AddPoint(CreatePoint(areaSqa.endPoint, ref m_rebornAreaParent));
                        m_rebornAreaList.Add(ess);
                        break;
                    case Square.COLLECT_TILE:
                        ess.AddPoint(CreatePoint(areaSqa.startPoint, ref m_collectAreaParent));
                        ess.AddPoint(CreatePoint(areaSqa.endPoint, ref m_collectAreaParent));
                        m_collectAreaList.Add(ess);
                        break;
                    case Square.JUMP_TILE:
                        ess.AddPoint(CreatePoint(areaSqa.startPoint, ref m_jumpAreaParent));
                        ess.AddPoint(CreatePoint(areaSqa.endPoint, ref m_jumpAreaParent));
                        m_jumpAreaList.Add(ess);
                        break;
                }
            }
        }

        if (!m_isExportProcess)
        {
            CreateTileColliderArea();
        }

    }
    //弃用的
    private GameObject CreatePoint(Vector3 pos,ref GameObject parent , string parentName)
    {
        if (parent == null)
        {
            parent = new GameObject(parentName);
            parent.transform.parent = this.transform;
        }

        GameObject go = new GameObject("point(" + pos + ")");
        go.transform.position = pos;
        go.transform.parent = parent.transform;
        return go;
    }

    private GameObject CreatePoint(Vector3 pos, ref GameObject parent)
    {
        GameObject go = new GameObject("point(" + pos + ")");
        go.transform.position = pos;
        go.transform.parent = parent.transform;
        return go;
    }
    //实例化碰撞体组
    private void CreateTileColliderArea()
    {
        int squareID = 0;
        m_dicTileID.Clear();
        foreach (Square item in m_squareList)
        {
            GameObject tmpTile = Instantiate(m_tilePrefab, m_tileGroup.transform);
            tmpTile.transform.localScale = new Vector3(m_tilePixel, m_tilePixel, 1f);
            tmpTile.transform.position = item.Position3D;
            tmpTile.name = "MapTile-" + item.Row + "-" + item.Col;
            m_dicTileID.Add(tmpTile, squareID++);
        }
    }
    #endregion

    #region 初始化及善后方法
    public void InitData()
    {
        if (m_proxy != null)
        {
            m_curY = m_proxy.transform.position.y;
            m_lastY = m_curY;
        }

        if (m_tileGroup != null)
        {
            DestroyImmediate(m_tileGroup);
        }

        m_tileGroup = new GameObject("TileGroup");

        m_squareList.Clear();
    }

    public void InstancedAllAreaParent()
    {
        if (m_barrierAreaParent == null)
        {
            m_barrierAreaParent = new GameObject("障碍区");
            m_barrierAreaParent.transform.parent = transform;
        }

        if (m_safeAreaParent == null)
        {
            m_safeAreaParent = new GameObject("安全区");
            m_safeAreaParent.transform.parent = transform;
        }

        if (m_portolAreaParent == null)
        {
            m_portolAreaParent = new GameObject("传送区");
            m_portolAreaParent.transform.parent = transform;
        }

        if (m_bornAreaParent == null)
        {
            m_bornAreaParent = new GameObject("出生区");
            m_bornAreaParent.transform.parent = transform;
        }

        if (m_rebornAreaParent == null)
        {
            m_rebornAreaParent = new GameObject("复活区");
            m_rebornAreaParent.transform.parent = transform;
        }

        if (m_monsterAreaParent == null)
        {
            m_monsterAreaParent = new GameObject("怪物区");
            m_monsterAreaParent.transform.parent = transform;
        }

        if (m_collectAreaParent == null)
        {
            m_collectAreaParent = new GameObject("采集区");
            m_collectAreaParent.transform.parent = transform;
        }

        if (m_jumpAreaParent == null)
        {
            m_jumpAreaParent = new GameObject("跳跃区");
            m_jumpAreaParent.transform.parent = transform;
        }
    }

    public void ClearApart()
    {
        m_dicTileID.Clear();
        m_squareList.Clear();
        m_dicTileList.Clear();

        ClearAreaSquare();
    }

    public void ClearAreaSquare()
    {
        if (m_barrierAreaParent != null)
        {
           DestroyImmediate(m_barrierAreaParent);
        }

        if (m_safeAreaParent != null)
        {
            DestroyImmediate(m_safeAreaParent);
        }

        if (m_portolAreaParent != null)
        {
            DestroyImmediate(m_portolAreaParent);
        }

        if (m_bornAreaParent != null)
        {
            DestroyImmediate(m_bornAreaParent);
        }

        if (m_rebornAreaParent != null)
        {
            DestroyImmediate(m_rebornAreaParent);
        }

        if (m_monsterAreaParent != null)
        {
            DestroyImmediate(m_monsterAreaParent);
        }

        if (m_collectAreaParent != null)
        {
            DestroyImmediate(m_collectAreaParent);
        }

        if (m_jumpAreaParent != null)
        {
            DestroyImmediate(m_jumpAreaParent);
        }

        m_selBarrierArea = 0; //当前选择障碍区的子区号
        m_selBarrierPoint = 0;
        m_selSafeArea = 0;    //当前选择的安全区的子区号
        m_selSafePoint = 0;
        m_selPortolArea = 0;     //当前选择的传送区的子区号
        m_selPortolPoint = 0;
        m_selBornArea = 0;       //当前选择的出生区的子区号
        m_selBornPoint = 0;
        m_selRebornArea = 0;     //当前选择的复活区的子区号
        m_selRebornPoint = 0;
        m_selMonsterArea = 0;       //当前选择的怪物区的子区号
        m_selMonsterPoint = 0;
        m_selCollectArea = 0;        //当前选择的采集区的子区号
        m_selCollectPoint = 0;
        m_selJumpArea = 0;       //当前选择的跳跃区的子区号
        m_selJumpPoint = 0;

        m_barrierAreaList.Clear();
        m_safeAreaList.Clear();
        m_portolAreaList.Clear();
        m_bornAreaList.Clear();
        m_rebornAreaList.Clear();
        m_monsterAreaList.Clear();
        m_collectAreaList.Clear();
        m_jumpAreaList.Clear();
    }

    public void ClearAllData()
    {
        m_navMeshVertices = null;
        m_navMeshIndcies = null;

        ClearApart();
        ClearAreaSquare();

        if (m_tileGroup)
        {
            DestroyImmediate(m_tileGroup);
            m_tileGroup = null;
        }
    }

    #endregion

    #region 配合编辑界面的方法

    public void CreateTiles()
    {
        if (m_navMeshIndcies == null || m_navMeshIndcies == null)
        {
            Debug.LogError("导航网格数据为空");
            return;
        }

        Debug.Log("开始计算合适的方格");

        InitData();

        for (int curRow = 0; curRow < m_RowCount; ++curRow)
        {
            for (int curCol = 0; curCol < m_ColCount; ++curCol)
            {
                Vector2 topleft = new Vector2(curCol * m_tilePixel, (curRow + 1) * m_tilePixel);
                Square square = new Square(topleft, m_tilePixel);

                if (SquareIsInNavMesh(square))
                {
                    square.Set3DPosition(m_curY);
                    m_squareList.Add(square);
                    /*
                    GameObject tmpTile = Instantiate(m_tilePrefab, m_tileGroup.transform);
                    tmpTile.transform.localScale = new Vector3(m_tilePixel, m_tilePixel, 1f);
                    tmpTile.transform.position = square.Position3D;
                    tmpTile.name = "MapTile-" + square.Row + "-" + square.Col;
                    m_dicTileID.Add(tmpTile, squareID++);
                    */
                }
                else
                {
                    square = null;
                }
            }
        }

        CreateTileColliderArea();

        Debug.Log("结束计算 , 合适的方格数:" + m_squareList.Count);

    }

    //判断方格的中心是否在导航网格内
    private bool SquareIsInNavMesh(Square square)
    {
        bool isInside = false;
        for (int i = 0; i < m_navMeshIndcies.Length; i += 3)
        {
            if (PointIsInTriangle(m_navMeshVertices[m_navMeshIndcies[i]], m_navMeshVertices[m_navMeshIndcies[i + 1]], m_navMeshVertices[m_navMeshIndcies[i + 2]], square.Center))
            {
                isInside = true;
                break;
            }
        }

        return isInside;
    }

    //判断点是否在三角形内
    private bool PointIsInTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        double ca, ba, cb;
        ca = Dot3Point(C, A, P);
        ba = Dot3Point(B, A, P);
        if ((ca > 0 && ba > 0) || (ca < 0 && ba < 0)) //若同为逆时针或同为顺时针则不在三角形内
        {
            return false;
        }

        if (ca == 0 && ba == 0) return true; //在A点上的情况

        cb = Dot3Point(C, B, P);
        if (cb == 0) return true; //在BC边上，也可能B点或C点
        if (ba == 0) //和AB共线的情况
        {
            if ((ca > 0 && cb > 0) || (ca < 0 && cb < 0)) //不再ABC夹角内
            {
                return false;
            }
            else
                return true; //在AB边上
        }

        if ((ba > 0 && cb > 0) || (ba < 0 && cb < 0)) //在三角形内
        {
            return true;
        }
        else
            return false;
    }

    //三点点积
    private double Dot3Point(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return ((p1.x - p2.x) * (p3.y - p2.y) - (p1.y - p2.y) * (p3.x - p2.x));
    }

    //设置地图块类型
    public void SetSquareType(GameObject square, int newType)
    {
        int id;
        if (m_dicTileID.TryGetValue(square, out id))
        {
            m_squareList[id].SetAreaType(newType);
        }
    }

    //计算根据2D坐标计算行列
    private void CalculateRowCol(Vector3 pos, out int row, out int col)
    {
        row = (int)(pos.z / m_tilePixel);
        col = (int)(pos.x / m_tilePixel);
    }

    //判断行列数是否在合理范围内
    private bool IsLegalRange(int row, int col)
    {
        return row >= 0 && col >= 0 && row < m_RowCount && col < m_ColCount;
    }

    public void CheckYChange()
    {
        if (m_curY != m_lastY)
        {
            ResetYValue();
            m_lastY = m_curY;
        }
    }

    public void ResetYValue()
    {
        if (m_squareList != null)
        {
            for (int i = 0; i < m_squareList.Count; i++)
            {
                m_squareList[i].Set3DPosition(m_curY);
            }
        }
    }

    //设置框选的起点
    public void SetSelectStartPoint(Vector3 point)
    {
        m_selectStart = point;
    }

    //设置框选的终点并计算出框选的方格并设置
    public void BoxSelection(Vector3 endPoint, int tileType)
    {
        int startRow, startCol;
        CalculateRowCol(m_selectStart, out startRow, out startCol);

        if (!IsLegalRange(startRow, startCol))
        {
            Debug.Log("框选范围不合法");
            return;
        }

        int endRow, endCol;
        CalculateRowCol(endPoint, out endRow, out endCol);

        if (!IsLegalRange(endRow, endCol))
        {
            Debug.Log("框选范围不合法");
            return;
        }

        int minRow = startRow < endRow ? startRow : endRow;
        int maxRow = startRow > endRow ? startRow : endRow;
        int minCol = startCol < endCol ? startCol : endCol;
        int maxCol = startCol > endCol ? startCol : endCol;

        foreach (Square item in m_squareList)
        {
            if (item.Row >= minRow && item.Row <= maxRow && item.Col >= minCol && item.Col <= maxCol)
            {
                item.SetAreaType(tileType);
            }
        }
    }

    #endregion

    #region 编辑区域相关

    public int TryGetAreaCount(int index)
    {
        int count = 0;
        switch (index)
        {
            case 0 :count = m_barrierAreaList.Count;break;
            case 2: count = m_safeAreaList.Count; break;
            case 3: count = m_portolAreaList.Count; break;
            case 4: count = m_monsterAreaList.Count; break;
            case 5: count = m_bornAreaList.Count; break;
            case 6: count = m_rebornAreaList.Count; break;
            case 7: count = m_collectAreaList.Count; break;
            case 8: count = m_jumpAreaList.Count; break;
        }

        return count;
    }

    public EditSelectSquare TryGetSubArea( int index )
    {
        switch (index)
        {
            case 0:
                if (m_barrierAreaList.Count > 0)
                {
                    return m_barrierAreaList[m_selBarrierArea];
                }
                else return null;
            case 2:
                if (m_safeAreaList.Count>0)
                {
                    return m_safeAreaList[m_selSafeArea];
                }
                else return null;
            case 3:
                if (m_portolAreaList.Count > 0)
                {
                    return m_portolAreaList[m_selPortolArea];
                }
                else return null;
            case 4:
                if (m_monsterAreaList.Count > 0)
                {
                    return m_monsterAreaList[m_selMonsterArea];
                }
                else return null;
            case 5:
                if (m_bornAreaList.Count > 0)
                {
                    return m_bornAreaList[m_selBornArea];
                }
                else return null;
            case 6:
                if (m_rebornAreaList.Count > 0)
                {
                    return m_rebornAreaList[m_selRebornArea];
                }
                else return null;
            case 7:
                if (m_collectAreaList.Count > 0)
                {
                    return m_collectAreaList[m_selCollectArea];
                }
                else return null;
            case 8:
                if (m_jumpAreaList.Count > 0)
                {
                    return m_jumpAreaList[m_selJumpArea];
                }
                else return null;
        }

        return null;
    }

    public void CreateSubArea( int index )
    {
        /*
        switch (index)
        {
            case 2: break;
            case 3: break;
            case 4: break;
            case 5: break;
            case 6: break;
            case 7: break;
            case 8: break;
        }
        */

        switch (index)
        {
            case 0: CreateBarrierSubArea(); break;
            case 2: CreateSafeSubArea(); break;
            case 3: CreatePortolSubArea(); break;
            case 4: CreateMonsterSubArea(); break;
            case 5: CreateBornSubArea(); break;
            case 6: CreateRebornSubArea(); break;
            case 7: CreateCollectSubArea(); break;
            case 8: CreateJumpSubArea(); break;
        }
    }

    public void DeleteSubArea(int index)
    {
        switch (index)
        {
            case 0: DeleteBarrierSubArea(); break;
            case 2: DeleteSafeSubArea(); break;
            case 3: DeletePortolSubArea(); break;
            case 4: DeleteMonsterSubArea(); break;
            case 5: DeleteBornSubArea(); break;
            case 6: DeleteRebornSubArea(); break;
            case 7: DeleteCollectSubArea(); break;
            case 8: DeleteJumpSubArea(); break;
        }
    }

    public void DeleteSubAreaPoint( int index )
    {
        switch (index)
        {
            case 0: DeleteBarrierSubAreaPoint(); break;
            case 2: DeleteSafeSubAreaPoint(); break;
            case 3: DeletePortolSubAreaPoint(); break;
            case 4: DeleteMonsterSubAreaPoint(); break;
            case 5: DeleteBornSubAreaPoint(); break;
            case 6: DeleteRebornSubAreaPoint(); break;
            case 7: DeleteCollectSubAreaPoint(); break;
            case 8: DeleteJumpSubAreaPoint(); break;
        }
    }


    //创建各子区的方法
    public void CreateBarrierSubArea()
    {
        if (m_barrierAreaParent == null)
        {
            m_barrierAreaParent = new GameObject("障碍区");
            m_barrierAreaParent.transform.parent = transform;
        }

        EditSelectSquare selectSqa = new EditSelectSquare();
        selectSqa.parent = m_barrierAreaParent;
        m_barrierAreaList.Add(selectSqa);
        m_selBarrierArea = m_barrierAreaList.Count - 1;
    }

    public void CreateSafeSubArea()
    {
        if (m_safeAreaParent == null)
        {
            m_safeAreaParent = new GameObject("安全区");
            m_safeAreaParent.transform.parent = transform;
        }

        EditSelectSquare selectSqa = new EditSelectSquare();
        selectSqa.parent = m_safeAreaParent;
        m_safeAreaList.Add(selectSqa);
        m_selSafeArea = m_safeAreaList.Count-1;
    }

    public void CreatePortolSubArea()
    {
        if (m_portolAreaParent == null)
        {
            m_portolAreaParent = new GameObject("传送区");
            m_portolAreaParent.transform.parent = transform;
        }

        EditSelectSquare selectSqa = new EditSelectSquare();
        selectSqa.parent = m_portolAreaParent;
        m_portolAreaList.Add(selectSqa);
        m_selPortolArea = m_portolAreaList.Count-1;
    }

    public void CreateMonsterSubArea()
    {
        if (m_monsterAreaParent == null)
        {
            m_monsterAreaParent = new GameObject("怪物区");
            m_monsterAreaParent.transform.parent = transform;
        }

        EditSelectSquare selectSqa = new EditSelectSquare();
        selectSqa.parent = m_monsterAreaParent;
        m_monsterAreaList.Add(selectSqa);
        m_selMonsterArea = m_monsterAreaList.Count-1;
    }

    public void CreateBornSubArea()
    {
        if (m_bornAreaParent == null)
        {
            m_bornAreaParent = new GameObject("出生区");
            m_bornAreaParent.transform.parent = transform;
        }

        EditSelectSquare selectSqa = new EditSelectSquare();
        selectSqa.parent = m_bornAreaParent;
        m_bornAreaList.Add(selectSqa);
        m_selBornArea = m_bornAreaList.Count-1;
    }

    public void CreateRebornSubArea()
    {
        if (m_rebornAreaParent == null)
        {
            m_rebornAreaParent = new GameObject("复活区");
            m_rebornAreaParent.transform.parent = transform;
        }

        EditSelectSquare selectSqa = new EditSelectSquare();
        selectSqa.parent = m_rebornAreaParent;
        m_rebornAreaList.Add(selectSqa);
        m_selRebornArea = m_rebornAreaList.Count - 1;
        
    }

    public void CreateCollectSubArea()
    {
        if (m_collectAreaParent == null)
        {
            m_collectAreaParent = new GameObject("采集区");
            m_collectAreaParent.transform.parent = transform;
        }

        EditSelectSquare selectSqa = new EditSelectSquare();
        selectSqa.parent = m_collectAreaParent;
        m_collectAreaList.Add(selectSqa);
        m_selCollectArea = m_collectAreaList.Count - 1;
    }

    public void CreateJumpSubArea()
    {
        if (m_jumpAreaParent == null)
        {
            m_jumpAreaParent = new GameObject("跳跃区");
            m_jumpAreaParent.transform.parent = transform;
        }

        EditSelectSquare selectSqa = new EditSelectSquare();
        selectSqa.parent = m_jumpAreaParent;
        m_jumpAreaList.Add(selectSqa);
        m_selJumpArea = m_jumpAreaList.Count - 1;
    }

    //删除各子区的方法
    public void DeleteBarrierSubArea()
    {
        if (m_barrierAreaList.Count <= 0) return;

        m_barrierAreaList[m_selBarrierArea].Destroy();
        m_barrierAreaList.RemoveAt(m_selBarrierArea);

        m_selBarrierArea--;
        if (m_selBarrierArea < 0)
            m_selBarrierArea = 0;
    }
       
    public void DeleteSafeSubArea( )
    {
        if (m_safeAreaList.Count <= 0) return;

        m_safeAreaList[m_selSafeArea].Destroy();
        m_safeAreaList.RemoveAt(m_selSafeArea);

        m_selSafeArea--;
        if (m_selSafeArea < 0)
            m_selSafeArea = 0;
    }

    public void DeletePortolSubArea()
    {
        if (m_portolAreaList.Count <= 0) return;

        m_portolAreaList[m_selPortolArea].Destroy();
        m_portolAreaList.RemoveAt(m_selPortolArea);

        m_selPortolArea--;
        if (m_selPortolArea < 0)
            m_selPortolArea = 0;
    }

    public void DeleteMonsterSubArea()
    {
        if (m_monsterAreaList.Count <= 0) return;

        m_monsterAreaList[m_selMonsterArea].Destroy();
        m_monsterAreaList.RemoveAt(m_selMonsterArea);

        m_selMonsterArea--;
        if (m_selMonsterArea < 0)
            m_selMonsterArea = 0;
    }

    public void DeleteBornSubArea()
    {
        if (m_bornAreaList.Count <= 0) return;

        m_bornAreaList[m_selBornArea].Destroy();
        m_bornAreaList.RemoveAt(m_selBornArea);

        m_selBornArea--;
        if (m_selBornArea < 0)
            m_selBornArea = 0;
    }

    public void DeleteRebornSubArea()
    {
        if (m_rebornAreaList.Count <= 0) return;

        m_rebornAreaList[m_selRebornArea].Destroy();
        m_rebornAreaList.RemoveAt(m_selRebornArea);

        m_selRebornArea--;
        if (m_selRebornArea < 0)
            m_selRebornArea = 0;

    }

    public void DeleteCollectSubArea()
    {
        if (m_collectAreaList.Count <= 0) return;

        m_collectAreaList[m_selCollectArea].Destroy();
        m_collectAreaList.RemoveAt(m_selCollectArea);

        m_selCollectArea--;
        if (m_selCollectArea < 0)
            m_selCollectArea = 0;
    }

    public void DeleteJumpSubArea()
    {
        if (m_jumpAreaList.Count <= 0) return;

        m_jumpAreaList[m_selJumpArea].Destroy();
        m_jumpAreaList.RemoveAt(m_selJumpArea);

        m_selJumpArea--;
        if (m_selJumpArea < 0)
            m_selJumpArea = 0;
    }

    //删除子区中的点
    public void DeleteBarrierSubAreaPoint()
    {
        if (m_barrierAreaList.Count <= 0) return;

        m_barrierAreaList[m_selBarrierArea].RemovePoint(m_selBarrierPoint);
    }

    public void DeleteSafeSubAreaPoint()
    {
        if (m_safeAreaList.Count <= 0) return;

        m_safeAreaList[m_selSafeArea].RemovePoint(m_selSafePoint);
    }

    public void DeletePortolSubAreaPoint()
    {
        if (m_portolAreaList.Count <= 0) return;

        m_portolAreaList[m_selPortolArea].RemovePoint(m_selPortolPoint);
    }

    public void DeleteMonsterSubAreaPoint()
    {
        if (m_monsterAreaList.Count <= 0) return;

        m_monsterAreaList[m_selMonsterArea].RemovePoint(m_selMonsterPoint);
    }

    public void DeleteBornSubAreaPoint()
    {
        if (m_bornAreaList.Count <= 0) return;

        m_bornAreaList[m_selBornArea].RemovePoint(m_selBornPoint);
    }

    public void DeleteRebornSubAreaPoint()
    {
        if (m_rebornAreaList.Count <= 0) return;

        m_rebornAreaList[m_selRebornArea].RemovePoint(m_selRebornPoint);
    }

    public void DeleteCollectSubAreaPoint()
    {
        if (m_collectAreaList.Count <= 0) return;

        m_collectAreaList[m_selCollectArea].RemovePoint(m_selCollectPoint);
    }

    public void DeleteJumpSubAreaPoint()
    {
        if (m_jumpAreaList.Count <= 0) return;

        m_jumpAreaList[m_selJumpArea].RemovePoint(m_selJumpPoint);
    }

    #endregion

    #region 绘制方法

    void OnDrawGizmos()
    {
        DrawLimitLine();
        DrawTileArea();
        //DrawSelectTiles();
        DrawAllAreaSquare();
    }

    //绘制边界线
    void DrawLimitLine()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(Vector3.zero, Vector3.forward * 100f);
        Gizmos.DrawLine(Vector3.zero, Vector3.right * 100f);
    }

    //绘制方格线
    void DrawTileArea()
    {
        if (m_showTileArea)
        {
            Gizmos.color = Color.white;
           //int limit = m_ColCount > m_RowCount ? m_ColCount : m_RowCount;
            Vector3 horiStart = new Vector3(0, m_curY, 0);
            Vector3 horiEnd = horiStart + new Vector3(m_tilePixel * m_ColCount, 0, 0);
            Vector3 vertStart = horiStart;
            Vector3 vertEnd = vertStart + new Vector3(0, 0, m_tilePixel * m_RowCount);
            /*
            for (int i = 0; i <= limit; i++)
            {
                Gizmos.DrawLine(horiStart, horiEnd);
                Gizmos.DrawLine(vertStart, vertEnd);
                horiStart.z += m_tilePixel;
                horiEnd.z += m_tilePixel;
                vertStart.x += m_tilePixel;
                vertEnd.x += m_tilePixel;
            }
            */
            for (int curRow = 0; curRow <= m_RowCount; curRow++)
            {
                Gizmos.DrawLine(horiStart, horiEnd);
                horiStart.z += m_tilePixel;
                horiEnd.z += m_tilePixel;
            }

            for (int curCol = 0; curCol <= m_ColCount; curCol++)
            {
                Gizmos.DrawLine(vertStart, vertEnd);
                vertStart.x += m_tilePixel;
                vertEnd.x += m_tilePixel;
            }
        }
    }

    //绘制筛选方格
    void DrawSelectTiles()
    {
        if (m_squareList == null || m_squareList.Count <= 0) return;

        foreach (Square item in m_squareList)
        {
            //Gizmos.color = GetSquareColorByType(item.GetAreaType());
            Vector3 scl = Vector3.one * m_tilePixel;
            scl.y = 0;
            if (m_showWireCube)
            {
                DrawWireCube(item.Position3D, scl , item.GetAreaType());
            }
            else
            {
                DrawSoliderCube(item.Position3D, scl ,item.GetAreaType());
            }
        }
    }


    //根据类型绘制实体方格
    Color getColor(Vector3 pos, Vector3 scl, int type)
    {
        Color color = Color.green;
        switch (type)
        {
            case Square.NORMAL_TILE: color = Color.green; break;
            case Square.BARRIER_TILE: color = Color.red; break;
            case Square.SAFE_TILE: color = Color.gray; break;
            case Square.PORTOL_TILE: color = Color.magenta; break;
            case Square.MONSTER_TILE: color = Color.yellow; break;
            case Square.BORN_TILE: color = Color.blue; break;
            case Square.REBORN_TILE: color = Color.cyan; break;
            case Square.COLLECT_TILE: color = Color.white; break;
            case Square.JUMP_TILE: color = Color.black; break;
        }
        
        return color;
    }

    //根据类型绘制实体方格
    void DrawSoliderCube(Vector3 pos, Vector3 scl, int type)
    {
        Color finalColor = Color.white;
        if (type == 0)
        {
            finalColor = Color.red;
        }

        if ((type & Square.BORN_TILE) > 0)
        {
            finalColor *= Color.blue;
        }

        if ((type & Square.COLLECT_TILE) > 0)
        {
            finalColor *= Color.white;
        }

        if ((type & Square.JUMP_TILE) > 0)
        {
            finalColor *= Color.black;
        }

        if ((type & Square.MONSTER_TILE) > 0)
        {
            finalColor *= Color.yellow;
        }

        if ((type & Square.NORMAL_TILE) > 0)
        {
            finalColor *= Color.green;
        }

        if ((type & Square.PORTOL_TILE) > 0)
        {
            finalColor *= Color.magenta;
        }

        if ((type & Square.REBORN_TILE) > 0)
        {
            finalColor *= Color.cyan;
        }

        if ((type & Square.SAFE_TILE) > 0)
        {
            finalColor *= Color.gray;
        }

        Gizmos.color = finalColor;
        Gizmos.DrawCube(pos, scl);
    }

    //根据类型绘制线框方格
    void DrawWireCube(Vector3 pos, Vector3 scl, int type)
    {
        Color finalColor = Color.white;
        if (type == 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(pos, scl);
        }

        if ((type & Square.BORN_TILE) > 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(pos, scl);
        }

        if ((type & Square.COLLECT_TILE) > 0)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(pos, scl);
        }

        if ((type & Square.JUMP_TILE) > 0)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(pos, scl);
        }

        if ((type & Square.MONSTER_TILE) > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(pos, scl);
        }

        if ((type & Square.NORMAL_TILE) > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(pos, scl);
        }

        if ((type & Square.PORTOL_TILE) > 0)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(pos, scl);
        }

        if ((type & Square.REBORN_TILE) > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(pos, scl);
        }

        if ((type & Square.SAFE_TILE) > 0)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(pos, scl);
        }
    }

    private void DrawAllAreaSquare()
    {
        if (m_barrierAreaList.Count > 0)
        {
            foreach (EditSelectSquare item in m_barrierAreaList)
            {
                DrawSquare(item, Color.red);
            }
        }

        if (m_safeAreaList.Count > 0)
        {
            foreach (EditSelectSquare item in m_safeAreaList)
            {
                DrawSquare(item , Color.gray);
            }
        }

        if (m_portolAreaList.Count > 0)
        {
            foreach ( EditSelectSquare item in m_portolAreaList )
            {
                DrawSquare(item, Color.magenta);
            }
        }

        if (m_monsterAreaList.Count > 0)
        {
            foreach (EditSelectSquare item in m_monsterAreaList)
            {
                DrawSquare(item, Color.yellow);
            }
        }

        if (m_bornAreaList.Count > 0)
        {
            foreach (EditSelectSquare item in m_bornAreaList)
            {
                DrawSquare(item, Color.blue);
            }
        }

        if (m_rebornAreaList.Count > 0)
        {
            foreach (EditSelectSquare item in m_rebornAreaList)
            {
                DrawSquare(item, Color.cyan);
            }
        }

        if (m_collectAreaList.Count > 0)
        {
            foreach (EditSelectSquare item in m_collectAreaList)
            {
                DrawSquare(item, Color.green);
            }
        }

        if (m_jumpAreaList.Count > 0)
        {
            foreach (EditSelectSquare item in m_jumpAreaList)
            {
                DrawSquare(item, Color.black);
            }
        }
    }


    private void DrawSquare(EditSelectSquare square, Color color)
    {
        if (square == null)
            return;
        Gizmos.color = color;

        for (int i = 0; i < square.AllPoints.Count; i++)
        {
            if (i != square.AllPoints.Count - 1)
            {
                if (square.AllPoints[i + 1] == null)
                {
                    Debug.LogError("there a null in the point gameobj lst.");
                    return;
                }
                Gizmos.DrawLine(square.AllPoints[i].transform.position, square.AllPoints[i + 1].transform.position);
            }
            else
            {
                Gizmos.DrawLine(square.AllPoints[i].transform.position, square.AllPoints[0].transform.position);
            }
        }
    }
    #endregion

    #region 数据导出
    //地图块分类
    private void MapTileClassification()
    {
        m_dicTileList.Clear();

        List<Square> normalArea = new List<Square>();
        List<Square> SafeArea = new List<Square>();
        List<Square> PortolArea = new List<Square>();
        List<Square> MonsterArea = new List<Square>();
        List<Square> BornArea = new List<Square>();
        List<Square> RebornArea = new List<Square>();
        List<Square> CollectArea = new List<Square>();
        List<Square> JumpArea = new List<Square>();

        foreach (Square item in m_squareList)
        {
            /*
            switch (item.GetAreaType())
            {
                case Square.NORMAL_TILE:
                    normalArea.Add(item);
                    break;
                case Square.BORN_TILE:
                    BornArea.Add(item);
                    break;
                case Square.COLLECT_TILE:
                    CollectArea.Add(item);
                    break;
                case Square.JUMP_TILE:
                    JumpArea.Add(item);
                    break;
                case Square.MONSTER_TILE:
                    MonsterArea.Add(item);
                    break;
                case Square.PORTOL_TILE:
                    PortolArea.Add(item);
                    break;
                case Square.REBORN_TILE:
                    RebornArea.Add(item);
                    break;
                case Square.SAFE_TILE:
                    SafeArea.Add(item);
                    break;
            }*/

            int areaType = item.GetAreaType();

            if ( (areaType & Square.NORMAL_TILE) > 0)
            {
                normalArea.Add(item);
            }

            if ((areaType & Square.BORN_TILE) > 0)
            {
                BornArea.Add(item);
            }

            if ((areaType & Square.COLLECT_TILE) > 0)
            {
                CollectArea.Add(item);
            }

            if ((areaType & Square.JUMP_TILE) > 0)
            {
                JumpArea.Add(item);
            }

            if ((areaType & Square.MONSTER_TILE) > 0)
            {
                MonsterArea.Add(item);
            }

            if ((areaType & Square.PORTOL_TILE) > 0)
            {
                PortolArea.Add(item);
            }

            if ((areaType & Square.REBORN_TILE) > 0)
            {
                RebornArea.Add(item);
            }

            if ((areaType & Square.SAFE_TILE) > 0)
            {
                SafeArea.Add(item);
            }
        }

        m_dicTileList.Add(Square.NORMAL_TILE, normalArea);
        m_dicTileList.Add(Square.SAFE_TILE, SafeArea);
        m_dicTileList.Add(Square.REBORN_TILE, RebornArea);
        m_dicTileList.Add(Square.PORTOL_TILE, PortolArea);
        m_dicTileList.Add(Square.MONSTER_TILE, MonsterArea);
        m_dicTileList.Add(Square.JUMP_TILE, JumpArea);
        m_dicTileList.Add(Square.COLLECT_TILE, CollectArea);
        m_dicTileList.Add(Square.BORN_TILE, BornArea);
    }

    public void ExportMapInfoFile(string filePath, string name , FileType fileType)
    {
        MapTileClassification();

        if (fileType == FileType.Server)
        {
            WriteToErlangFile(filePath, name.GetHashCode());
        }
        if (fileType == FileType.Client)
        {
            WriteToLuaFile(filePath, name.GetHashCode());
        }
    }


    //导出服务器用文件
    private void WriteToErlangFile(string filePath, int mapId)
    {
        string includeName = WriteToErlangIncludeFile(filePath);
        string name = "data_mask_" + mapId;


        StringBuilder builder = new StringBuilder();

        builder.Append("-module(" + name + ").");
        builder.Append("\n");
        builder.Append("-include(\"" + includeName + "\").");
        builder.Append("\n");
        builder.Append("-export([get_rec/0, get_normal_area/0, get_safe_area/0, get_portal_area/0, get_born_area/0, get_reborn_area/0, get_monster_area/0, get_collect_area/0, get_jump_area/0]).");
        builder.Append("\n");

        builder.Append("\n");
        builder.Append("get_rec()->");
        builder.Append("\n");
        builder.Append("\t#map{");
        builder.Append("\n");
        builder.Append("\t\t");
        builder.Append("row=" + m_RowCount + ",\n");
        builder.Append("\t\t");
        builder.Append("col=" + m_ColCount + ",\n");
        builder.Append("\t\t");
        builder.Append("tile_side_length=" + (int)(m_tilePixel * 100));
        builder.Append("\n");
        builder.Append("\t}.");
        /*
        builder.Append("get_small_rec()->");
        builder.Append("\n");
        builder.Append("\t{" + info.smallWidth + "," + info.smallHeight + "}.");
        builder.Append("\n");
        */

        builder.Append("\n");
        WriteErlangArea(builder, TileType.Normal);
        builder.Append("\n");
        WriteErlangArea(builder, TileType.Safe);
        builder.Append("\n");
        WriteErlangArea(builder, TileType.Portol);
        builder.Append("\n");
        WriteErlangArea(builder, TileType.Born);
        builder.Append("\n");
        WriteErlangArea(builder, TileType.Reborn);
        builder.Append("\n");
        WriteErlangArea(builder, TileType.Monster);
        builder.Append("\n");
        WriteErlangArea(builder, TileType.Collect);
        builder.Append("\n");
        WriteErlangArea(builder, TileType.Jump);
        builder.Append("\n");

        string dirName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirName))
            Directory.CreateDirectory(dirName);
        WriteToFile(dirName + "/" + name + ".erl", builder.ToString());
    }

    private void WriteErlangArea(StringBuilder builder, TileType AreaType)
    {
        if (m_dicTileList.ContainsKey((int)AreaType))
        {
            List<Square> squareList;
            if (m_dicTileList.TryGetValue((int)AreaType, out squareList))
            {
                string name = AreaType.ToString().ToLower();
                builder.Append("get_" + name + "_area" + "()->");
                builder.Append("\n");
                builder.Append("\t[\n");
                for (int i = 0; i < squareList.Count; i++)
                {
                    Vector2 pos = squareList[i].TopLeftPoint * PixelScale;
                    Vector2 endPos = squareList[i].BottomRightPoint * PixelScale;
                    builder.Append("\t\t{");
                    builder.Append("{" + Mathf.CeilToInt(pos.x) + ", " + Mathf.CeilToInt(pos.y) + "}, ");
                    builder.Append("{" + Mathf.CeilToInt(endPos.x) + ", " + Mathf.CeilToInt(endPos.y) + "}");
                    //builder.Append("{" + Mathf.CeilToInt((pos.x + endPos.x) / 2) + ", " + Mathf.CeilToInt((pos.y + endPos.y) / 2) + "}");
                    builder.Append("}");

                    if (i != squareList.Count - 1)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                }
                builder.Append("\n\t].");
            }
        }
    }

    //创建引用文件
    private string WriteToErlangIncludeFile(string filePath)
    {
        UTF8Encoding utf8 = new UTF8Encoding();

        string name = "map_mask.hrl";

        StringBuilder builder = new StringBuilder();
        builder.Append("-record(map, {row, col, tile_side_length}).");
        builder.Append("\n");
        builder.Append("-record(nav_triangle, {nav_id, tris, neibor, distance, center, group_id, collider}).");
        builder.Append("\n");
        builder.Append("-record(area, {area_id, border, tris, center}).");
        builder.Append("\n");
        builder.Append("-record(nav_area, {border, tris}).");
        builder.Append("\n");
        builder.Append("-record(monster_area, {area_id, border, tris, points}).");

        string dirName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirName))
            Directory.CreateDirectory(dirName);
        WriteToFile(dirName + "/" + name, builder.ToString());
        return name; ;
    }

    //导出客户端用的lua文件
    private void WriteToLuaFile(string filePath, int mapId)
    {
        UTF8Encoding utf8 = new UTF8Encoding();


        StringBuilder builder = new StringBuilder();

        //Config.MapResourceInfoTbl tpl = UnityEditor.AssetDatabase.LoadAssetAtPath<Config.MapResourceInfoTbl>("Assets/Resources/tpl/Config.MapResourceInfoTbl.asset");
        //Config.MapResourceInfo info = tpl.data.Find(it => it.maskId == mapId);
        string name = "DataMask" + mapId;
        builder.Append(name + " = {");
        builder.Append("\n");
        builder.Append("\trec = { width = " + m_ColCount + ", height = " + m_RowCount + "},");
        builder.Append("\n");

        /*
        builder.Append("\tsmall_rec = { width = " + info.smallWidth + ", height = " + info.smallHeight + "},");
        builder.Append("\n");
        */

        builder.Append("\n");
        WriteStringSquare(builder, TileType.Normal);
        //WriteStringArea(builder);
        builder.Append(",");
        builder.Append("\n");
        WriteStringSquare(builder, TileType.Safe);
        //WriteStringSafeArea(builder);
        builder.Append(",");
        builder.Append("\n");
        WriteStringSquare(builder, TileType.Portol);
        //WriteStringSquare(builder, SquareArea.Transfer);
        builder.Append(",");
        builder.Append("\n");
        WriteStringSquare(builder, TileType.Born);
        //WriteStringSquare(builder, SquareArea.Birth);
        builder.Append(",");
        builder.Append("\n");
        WriteStringSquare(builder, TileType.Reborn);
        //WriteStringSquare(builder, SquareArea.Relive);
        builder.Append(",");
        builder.Append("\n");
        WriteStringSquare(builder, TileType.Monster);
        //WriteStringMonster("monster", builder, NavEditAreaManager.sInstance.GetMonsterArea());
        builder.Append(",");
        builder.Append("\n");
        WriteStringSquare(builder, TileType.Collect);
        //WriteStringMonster("collection", builder, NavEditAreaManager.sInstance.GetCollectionArea());
        builder.Append(",");
        builder.Append("\n");
        WriteStringSquare(builder, TileType.Jump);
        //WriteStringMultiArea(builder, "jump_area", NavEditAreaManager.sInstance.GetJumpAreaPolygon());
        builder.Append("\n");
        builder.Append("}");

        string dirName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirName))
            Directory.CreateDirectory(dirName);
        WriteToFile(dirName + "/" + name + ".lua", builder.ToString());
    }

    //写入矩形块
    private void WriteStringSquare(StringBuilder builder, TileType area)
    {
        if (m_dicTileList.ContainsKey((int)area))
        {
            List<Square> squareList;
            if (m_dicTileList.TryGetValue((int)area, out squareList))
            {
                string name = area.ToString().ToLower();
                builder.Append("\t" + name + " = {");
                builder.Append("\n");
                for (int i = 0; i < squareList.Count; i++)
               {
                    builder.Append("\t\t");
                    Vector2 pos = squareList[i].TopLeftPoint;
                    Vector2 endPos = squareList[i].BottomRightPoint;
                    builder.Append("[" + (i+1) + "] = {");
                    builder.Append("start_point = { x = " + pos.x + ", y = " + pos.y + " }, ");
                    builder.Append("end_point = { x = " + endPos.x + ", y = " + endPos.y + " }");
                    //builder.Append("center = { x = " + ((pos.x + endPos.x) / 2) + ", y = " + ((pos.y + endPos.y) / 2) + "}");
                    builder.Append("}");
                    if (i != squareList.Count - 1)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                }
               builder.Append("\n");
               builder.Append("\t}");
            }
        }
    }

    //写入lua通用文件
    public void WriteClientCommonFile(DirectoryInfo info, string filePath)
    {

        StringBuilder builder = new StringBuilder();
        builder.Append("DataMask = DataMask or {}");
        builder.Append("\n");
        FileInfo[] files = info.GetFiles();
        List<int> list = new List<int>();
        int i = 0;
        foreach (var item in files)
        {
            //string name = item.Name;
            //string mapId = name.Split('.')[0];
            //list.Add(int.Parse(mapId));
            if (item.Extension == ".td")
            {
                list.Add(i++);
            }
        }

        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetRec", "rec");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetSmallRec", "small_rec");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetArea", "normal");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetSafeArea", "safe");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetPortol", "portol");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetBorn", "born");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetReborn", "reborn");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetMonster", "monster");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetCollection", "collect");
        builder.Append("\n");
        ClientCommonFileFunction(builder, list, "GetJumpArea", "jump");
        builder.Append("\n");
        //ClientCommonFileFunction(builder, list, "GetShadowArea", "shadow_area");
        //builder.Append("\n");

        string dirName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirName))
            Directory.CreateDirectory(dirName);
        WriteToFile(dirName + "/" + "DataMask" + ".lua", builder.ToString());
    }

    private void ClientCommonFileFunction(StringBuilder builder, List<int> list, string funName, string fieldName)
    {
        builder.Append("function DataMask." + funName + "(maskId)");
        builder.Append("\n");
        for (int i = 0; i < list.Count; i++)
        {
            builder.Append("\t");
             if (i == 0)
            {
               builder.Append("if maskId == " + list[i] + " then ");
            }
            else
            {
                builder.Append("elseif maskId == " + list[i] + " then ");
            }
            builder.Append("\n");
            builder.Append("\t\t");
            builder.Append("return DataMask" + list[i] + "." + fieldName);
            builder.Append("\n");
        }
        builder.Append("\t");
        builder.Append("else");
        builder.Append("\n");
        builder.Append("\t\t");
        builder.Append("LogError(\"not find \" .. maskId .. \" " + fieldName + " data\", true)");
        builder.Append("\n");
        builder.Append("\t\t");
        builder.Append("return nil");
        builder.Append("\n");
        builder.Append("\t");
        builder.Append("end");

        builder.Append("\n");
        builder.Append("end");
        builder.Append("\n");
    }

    //创建本地文件
    private void WriteToFile(string path, string content)
    {
        FileStream fs = File.Create(path);
        byte[] bytes = UTF8Encoding.Default.GetBytes(content);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
    }

    #endregion
}
