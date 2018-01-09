using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapTileTool : MonoBehaviour {
    public struct Tile
    {
       public int _row;
       public int _col;
    }

    public struct Line
    {
        public Vector2 _start;
        public Vector2 _end;
    }

    public const string DefalutTileName = "MapTile";

    [Range(0.5f, 10f)]
    public float m_scale = 1f;
    public int m_range = 1;
    public Transform m_orignTransform; //绘制区域的原点

    public GameObject TileGroup
    {
        get {
            if (m_tileGroup == null)
            {
                m_tileGroup = GameObject.Find("TileGroup");
                if (m_tileGroup == null)
                {
                    m_tileGroup = new GameObject("TileGroup");
                    m_tileGroup.transform.parent = this.transform;
                }
            }
            return m_tileGroup;
        }
        set {
            m_tileGroup = value;
        }
    }

    public GameObject PointGroup
    {
        get {
            if (m_hitPointGroup == null)
            {
                m_hitPointGroup = GameObject.Find("PointGroup");
                if (m_hitPointGroup == null)
                {
                    m_hitPointGroup = new GameObject("PointGroup");
                    m_hitPointGroup.transform.parent = this.transform;
                }
            }
            return m_hitPointGroup;
        }
        set {
            m_hitPointGroup = value;
        }
    }

    public List<Vector3> m_pointList;
    public List<Line> m_lineList;
    private int maxRow;
    private int maxCol;
    private float m_moveStep = 1f;
    private Vector3 m_startPos;
    private Vector3 m_orignPont;
    private GameObject m_hitPointGroup;
    private GameObject m_tileGroup;
    [SerializeField]
    public GameObject m_tile;
    private GameObject m_proxy;
    private List<GameObject> m_tileArray = new List<GameObject>();

    private void OnEnable()
    {

    }

    // Update is called once per frame
    void Update() {

    }

    void OnDrawGizmos()
    {
        if (m_pointList == null || m_pointList.Count == 0) return;

        Gizmos.color = Color.red;
        bool first = true;
        Vector3 prevPos = Vector3.zero;
        Vector3 firstPos = Vector3.zero;
        foreach (Vector3 pos in m_pointList)
        {
            Gizmos.DrawSphere(pos, 0.5f);
            if (first)
            {
                firstPos = pos;
                prevPos = pos;
                first = false;
            }
            else
            {
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }

        Gizmos.DrawLine(prevPos, firstPos);

    }

    public void AddNewPointPosition(Vector3 pos)
    {
        m_pointList.Add(pos);
    }

    /// <summary>
    /// 以区域中心点为基准绘制tile
    /// </summary>
    public void DrawAllTileWithCenter()
    {
        if (m_tile == null) return;

        InitOrignPoint();

        ClearAllTile();

        m_moveStep = m_scale;

        m_startPos = new Vector3(m_orignPont.x - m_range * m_moveStep, m_orignPont.y, m_orignPont.z + m_range * m_moveStep);

        int count = m_range * 2 + 1;
        maxCol = maxRow = count;
        for (int row = 0; row < count; row++)
        {
            for (int col = 0; col < count; col++)
            {
                GameObject temp = Instantiate(m_tile, m_tileGroup.transform);
                temp.transform.position = new Vector3(m_startPos.x + col * m_moveStep, m_startPos.y, m_startPos.z - row * m_moveStep);
                temp.transform.localScale = new Vector3(m_scale, m_scale, 1f);
                temp.name = DefalutTileName + '-' + row + '-' + col;

                m_tileArray.Add(temp);
            }
        }
        if (m_proxy == null)
        {
            m_proxy = Instantiate(m_tile, m_tileGroup.transform);
            m_proxy.name = "Proxy";
        }

        m_proxy.transform.position = m_orignPont;
        m_proxy.transform.localScale = new Vector3(m_scale * count, m_scale * count, 1f);
        m_proxy.SetActive(true);

    }

    /// <summary>
    /// 以区域中心点为起点绘制tile
    /// </summary>
    public void DrawAllTileWithLeftTop()
    {
        if (m_tile == null) return;
    }

    /// <summary>
    /// 确定区域中心点的位置
    /// </summary>
    public void InitOrignPoint()
    {
        if (m_orignTransform == null)
        {
            m_orignPont = Vector3.zero;
        }
        else
        {
            m_orignPont = m_orignTransform.position;
            m_orignTransform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 销毁已经生成的Tile对象
    /// </summary>
    public void ClearAllTile()
    {
        foreach (GameObject delObj in m_tileArray)
        {
            DestroyImmediate(delObj);
        }

        m_tileArray.Clear();

        if (m_proxy != null)
            m_proxy.SetActive(false);
    }


    /// <summary>
    /// 清空点对象
    /// </summary>
    public void ClearAllPoint()
    {
        if(m_pointList !=null)
            m_pointList.Clear();

        if (m_lineList != null)
            m_lineList.Clear();
    }


    /// <summary>
    /// 计算构成多边形的方格
    /// </summary>
    public void CalculateComfotalbeTile()
    {
        if (m_pointList.Count < 3)
        {
            Debug.LogWarning("顶点太少，不能构成多边形");
            return;
        }

        CollectAllLines();

        Vector3 topPos = new Vector3(0f, 0f, m_startPos.z - m_scale * maxRow);
        Vector3 bottomPos = new Vector3(0f, 0f, m_startPos.z);
        Vector3 leftPos = new Vector3(m_startPos.x + maxCol * m_scale, 0f, 0f);
        Vector3 rightPos = new Vector3(m_startPos.x, 0f, 0f);



        foreach (Vector3 pos in m_pointList)
        {
            if (pos.z > topPos.z)
            {
                topPos = pos;
            }

            if (pos.z < bottomPos.z)
            {
                bottomPos = pos;
            }

            if (pos.x < leftPos.x)
            {
                leftPos = pos;
            }
            if (pos.x > rightPos.x)
            {
                rightPos = pos;
            }
        }
        
        Tile topTile = PositionToRowCol(topPos);
        Tile leftTile = PositionToRowCol(leftPos);
        Tile bottomTile = PositionToRowCol(bottomPos);
        Tile rightTile =PositionToRowCol(rightPos);

        if (topTile._row < 0||leftTile._col < 0 || rightTile._col >= maxCol || bottomTile._row >=maxRow)
        {
            Debug.LogWarning("计算得出的开始行列和结束行列不在合法范围");
            return;
        }

        m_tileArray[topTile._row * maxRow + topTile._col].SetActive(true);
        m_tileArray[leftTile._row * maxRow + leftTile._col].SetActive(true);
        m_tileArray[bottomTile._row * maxRow + bottomTile._col].SetActive(true);
        m_tileArray[rightTile._row * maxRow + rightTile._col].SetActive(true);

        for (int curRow = topTile._row; curRow <= bottomTile._row; curRow++)
        {
            for (int curCol = leftTile._col; curCol <= rightTile._col; curCol++)
            {
                GameObject temp = m_tileArray[curRow * maxRow + curCol];
                Vector2 center = new Vector2(temp.transform.position.x, temp.transform.position.z);
                if (IsInsidePolygn(center))
                    temp.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 收录多边形的所有边
    /// </summary>
    void CollectAllLines()
    {
        if (m_lineList == null) m_lineList = new List<Line>();

        Vector3 prevPoint = m_pointList[0];
        Vector3 curPoint;
        int count = m_pointList.Count;
        for ( int i = 1; i<count ;++i )
        {
            curPoint = m_pointList[i];
            Line line;
            line._start = new Vector2(prevPoint.x, prevPoint.z);
            line._end = new Vector2(curPoint.x, curPoint.z);
        
            m_lineList.Add(line);

            prevPoint = curPoint;
        }

        Line lasted;
        lasted._start = new Vector2(prevPoint.x, prevPoint.z);
        lasted._end = new Vector2(m_pointList[0].x, m_pointList[0].z);
        m_lineList.Add(lasted);
    }


    bool IsInsidePolygn( Vector2 point )
    {
        int crossing = 0;
        int count = m_lineList.Count;
        for ( int i =0; i<count ; ++i)
        {
            float slope = (m_lineList[i]._end.y - m_lineList[i]._start.y) / (m_lineList[i]._end.x - m_lineList[i]._start.x);
            bool cond1 = (m_lineList[i]._start.x <= point.x) && (m_lineList[i]._end.x > point.x);
            bool cond2 = (m_lineList[i]._end.x <= point.x) && (m_lineList[i]._start.x > point.x);
            bool above = (point.y < slope * (point.x - m_lineList[i]._start.x) + m_lineList[i]._start.y);
            if ((cond1 || cond2) && above) crossing++;
        }

        return crossing % 2 != 0;
    }
    /// <summary>
    /// 对点进行排序
    /// </summary>
    /// <returns></returns>
     Vector3[] SortPoinList()
    {
        Vector3[] sortArray = new Vector3[m_pointList.Count];

        return sortArray;
    }
    /// <summary>
    /// 根据3维坐标转成行列数
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void PositionToRowCol( Vector3 pos , out int row , out int col)
    {
        row = 0;
        col = 0;

        Vector3 realStart = new Vector3( m_startPos.x - m_scale/2 , m_startPos.y, m_startPos.z + m_scale/2 );
        float xDistance = Mathf.Abs(pos.x - realStart.x);
        float zDistance = Mathf.Abs(pos.z - realStart.z);

        col = (int)Mathf.Floor(xDistance / m_scale);
        row = (int)Mathf.Floor(zDistance / m_scale);
    }

    public Tile PositionToRowCol(Vector3 pos)
    {
        Tile tile;

        Vector3 realStart = new Vector3(m_startPos.x - m_scale / 2, m_startPos.y, m_startPos.z + m_scale / 2);
        float xDistance = Mathf.Abs(pos.x - realStart.x);
        float zDistance = Mathf.Abs(pos.z - realStart.z);

        tile._col = (int)Mathf.Floor(xDistance / m_scale);
        tile._row = (int)Mathf.Floor(zDistance / m_scale);
        
        return tile;
    }

    /// <summary>
    /// 根据行列数转成3维坐标
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void RowColToPosition( int row ,int col )
    {

    }
}
