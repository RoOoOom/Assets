using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InfoTileTool : MonoBehaviour {

    #region 数据区
    public bool m_showTileArea = false ;  //是否显示方格区
    public bool m_showWireCube = false;  //是否绘制空心的方格

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

    private float m_curY = 0; //地面高度

    private List<Square> m_squareList = new List<Square>();
    private Dictionary<GameObject, int> m_dicTileID = new Dictionary<GameObject, int>();
    #endregion

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region 初始化及善后方法
    public void InitData()
    {
        if (m_proxy != null)
        {
            m_curY = m_proxy.transform.position.y;
        }

        if (m_tileGroup != null)
        {
            DestroyImmediate(m_tileGroup);
        }

        m_tileGroup = new GameObject("TileGroup");

        m_squareList.Clear();
    }
    public void ClearApart()
    {
        m_dicTileID.Clear();
        m_squareList.Clear();
    }

    public void ClearAllData()
    {
        m_navMeshVertices = null;
        m_navMeshIndcies = null;
        m_dicTileID.Clear();
        m_squareList.Clear();
    }

    #endregion

    #region 配合编辑界面的方法

    public void CreateTiles()
    {
        if (m_navMeshIndcies == null || m_navMeshIndcies == null )
        {
            Debug.LogError("导航网格数据为空");
            return;
        }

        Debug.Log("开始计算合适的方格");

        InitData();
        int squareID = 0;
        for (int curRow = 0; curRow < m_RowCount; ++curRow)
        {
            for (int curCol = 0; curCol < m_ColCount; ++curCol)
            {
                Vector2 topleft = new Vector2(curCol * m_tilePixel, (curRow + 1) * m_tilePixel);
                Square square = new Square(topleft, m_tilePixel);

                if (SquareIsInNavMesh(square))
                {
                    square.Set3DPosition(m_curY);
                    GameObject tmpTile = Instantiate(m_tilePrefab, m_tileGroup.transform);
                    tmpTile.transform.localScale = new Vector3(m_tilePixel, m_tilePixel, 1f);
                    tmpTile.transform.position = square.Position3D;
                    tmpTile.name = "MapTile-" + square.Row + "-" + square.Col;
                    m_squareList.Add(square);
                    m_dicTileID.Add(tmpTile, squareID++);
                }
                else
                {
                    square = null;
                }
            }
        }

        Debug.Log("结束计算 , 合适的方格数:" + m_squareList.Count);

    }

    //判断方格的中心是否在导航网格内
    private bool SquareIsInNavMesh( Square square )
    {
        bool isInside = false;
        for ( int i = 0; i< m_navMeshIndcies.Length ;i +=3 )
        {
            if ( PointIsInTriangle(m_navMeshVertices[m_navMeshIndcies[i]] , m_navMeshVertices[m_navMeshIndcies[i+1]] , m_navMeshVertices[m_navMeshIndcies[i+2]] , square.Center ) )
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
    private double Dot3Point( Vector2 p1 , Vector2 p2 , Vector2 p3 )
    {
        return ((p1.x - p2.x) * (p3.y - p2.y) - (p1.y - p2.y) * (p3.x - p2.x));
    }


    public void SetSquareType( GameObject square , int newType )
    {
        int id ;
        if (m_dicTileID.TryGetValue(square, out id))
        {
            Debug.Log("重新设定方格类型");
            m_squareList[id].SetType(newType);
        }
    }
    #endregion

    #region 绘制方法

    void OnDrawGizmos()
    {
        DrawLimitLine();
        DrawTileArea();
        DrawSelectTiles();
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
            Gizmos.color = Color.red;
            int limit = m_ColCount > m_RowCount ? m_ColCount : m_RowCount;
            Vector3 horiStart = new Vector3(0, m_curY, 0);
            Vector3 horiEnd = horiStart + new Vector3(m_tilePixel * m_ColCount, 0, 0);
            Vector3 vertStart = horiStart;
            Vector3 vertEnd = vertStart + new Vector3(0, 0, m_tilePixel * m_RowCount);

            for (int i = 0; i <= limit; i++ )
            {
                Gizmos.DrawLine(horiStart, horiEnd);
                Gizmos.DrawLine(vertStart, vertEnd);
                horiStart.z += m_tilePixel;
                horiEnd.z += m_tilePixel;
                vertStart.x += m_tilePixel;
                vertEnd.x += m_tilePixel;
            }
        }
    }

    //绘制筛选方格
    void DrawSelectTiles()
    {
        if ( m_squareList == null || m_squareList.Count <= 0)  return;

        Gizmos.color = Color.green;
        foreach ( Square item in m_squareList )
        {
            Gizmos.color = GetSquareColorByType(item.GetType());
            Vector3 scl = Vector3.one * m_tilePixel;
            scl.y = 0;
            if(m_showWireCube)
               Gizmos.DrawWireCube( item.Position3D , scl);
            else
               Gizmos.DrawCube(item.Position3D, scl);
        }
    }


    //根据类型获取方格颜色
    Color GetSquareColorByType( int type )
    {
        Color color = Color.green;
        switch ( type )
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
    #endregion
}
