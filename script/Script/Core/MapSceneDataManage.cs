using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Util;
using LuaInterface;

public class MapSceneDataManage  {


    private static MapSceneDataManage instance;

    public int curMapId;
    public int width;
    public int height;
    public int rowNum;
    public int colNum;

    public Vector3 minXAndY;
    public Vector3 maxXAndY;

    //public List<byte> mask = new List<byte>();
    public Dictionary<int, LuaTable> mask = new Dictionary<int, LuaTable>();
    public Dictionary<int, LuaTable> cover = new Dictionary<int, LuaTable>();
    public List<byte> trunkRoad = new List<byte>();
    public HashSet<int> safeArea = new HashSet<int>(); // 表示safe的grid index

    public bool onlyCheckMap = false;
    public bool isChangingScene = false;

    Transform mapRoot = null;
    LuaTable maskConfig = null;

    public static MapSceneDataManage Instance()
    {
        if (instance == null)
        {
            instance = new MapSceneDataManage();
        }
        return instance;
    }

    LuaTable GetData(int maskId, string key)
    {
        if (maskConfig == null)
        {
            maskConfig = LuaManager.instance.Lua.GetTable("DataMapMask");
        }

        LuaTable data = (LuaTable)maskConfig[maskId];
        LuaTable maskTable = (LuaTable)data[key];
        return maskTable;
    }

    LuaTable GetMask(int maskId)
    {
        return GetData(maskId, "mask");
    }

    LuaTable GetCover(int maskId)
    {
        return GetData(maskId, "cover");
    }

    public void SetMask(int sceneId, int maskId)
    {
        this.mask[sceneId] = GetMask(maskId);

        this.cover[sceneId] = GetCover(maskId);
    }

    public bool IsMask(int sceneId, int index)
    {
        return System.Convert.ToByte(this.mask[sceneId][index + 1]) != 0;
    }


    public void setTrunkRoad(LuaTable trunkRoad)
    {
        this.trunkRoad.Clear();
        object[] trunkRoadObj = trunkRoad.ToArray();
        for (int i = 0; i < trunkRoadObj.Length; i++)
        {
            this.trunkRoad.Add(System.Convert.ToByte(trunkRoadObj[i]));
        }
    }

    public void setSafeArea(LuaTable safeArea)
    {
        this.safeArea.Clear();
        for (int i = 0; i < safeArea.Length; i++)
        {
            this.safeArea.Add(System.Convert.ToInt32(safeArea[i]));
        }
    }

    public void setMapData(int curMapId, int width, int height, int rowNum, int colNum, Vector2 minXAndY, Vector2 maxXAndY)
    {
        this.curMapId = curMapId;
        this.width = width;
        this.height = height;
        this.rowNum = rowNum;
        this.colNum = colNum;
        this.minXAndY = minXAndY;
        this.maxXAndY = maxXAndY;
    }

    public void createMap(int rowNum, int colNum, float size)
    {
        GridGraph gg = AstarPath.active.astarData.gridGraph;
        gg.width = colNum;
        gg.depth = rowNum;
        gg.nodeSize = size; 
        gg.maxClimb = 0; 
        //gg.center = new Vector3(colNum * size / 2, -rowNum * size / 2, 0);
        gg.center = new Vector3(colNum * size * 1.5f / 2f, -rowNum * size / 2f, 0);
        // Updates internal size from the above values
        gg.UpdateSizeFromWidthDepth();
        // Scans all graphs, do not call gg.Scan(), that is an internal method
        AstarPath.active.Scan();
        GameObject go = GameObject.Find("/MapCanvas/MapBG");
        if (null != go)
        {
            mapRoot = go.transform;
        }
        else
        {
            Debug.LogError("can not find mapRoot : /MapCanvas/MapBG");
        }
    }

    public Vector3 GetNodePosition(int index)
    {
        
        GridGraph gg = AstarPath.active.astarData.gridGraph;
        if (index >= gg.nodes.Length)
        {
            return Vector3.zero;
        }
        return (Vector3)gg.nodes[index].position;
    }

    public int[] GetCellByPos(float x, float y, float z)
    {
        int cx = 0;
        int cy = 0;
        GetCell(new Vector3(x, y, z), ref cx, ref cy);
        return new int[2] { cx, cy };
        //GridNode node = (GridNode)AstarPath.active.GetNearest(new Vector3(x, y, z)).node;
        //return new int[2] { (int)node.Cell.x, (int)node.Cell.y };
    }

    public byte[] getByte()
    {
        return new byte[]{(byte)1,(byte)2,(byte)3,(byte)4};
    }
 
    void GetCell(Vector3 worldPos, ref int x, ref int y)
    {
        if (null == mapRoot)
        {
            x = 0;
            y = 0;
            return;
        }

        Vector3 pos = mapRoot.InverseTransformPoint(worldPos);
        if (pos.x >= 0)
        {
            if (pos.x < this.width)
            {
                x = Mathf.FloorToInt(pos.x / this.width * this.colNum);
            }
            else
            {
                x = this.colNum;
            }
        }

        if (pos.y <= 0)
        {
            if (pos.y > -this.height)
            {
                y = this.rowNum - Mathf.FloorToInt(-pos.y / this.height * this.rowNum) - 1;
            }
            else
            {
                y = 0;
            }
        }
    }

    int GetIndex(Vector3 worldPos)
    {
        int x = 0;
        int y = 0;
        GetCell(worldPos, ref x, ref y);
        return y * this.colNum + x;
    }

    public bool IsCover(Vector3 worldPos)
    {
        if (maskConfig == null)
            return false;

        LuaTable covers;
        if (this.cover.TryGetValue(this.curMapId, out covers))
        {
            int length = covers.Length;
            if (length < 1)
                return false;

            int index = GetIndex(worldPos);
            if (index < length)
            {
                return System.Convert.ToByte(covers[index + 1]) != 0;
            }
        }
        
        return false;
    }
}
