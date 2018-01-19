using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Normal = 0,
    Barrier,
    Safe,
    Portol,
    Monster,
    Born,
    Reborn,
    Collect,
    Jump
}

public class Square {
    public const int NORMAL_TILE = 0;
    public const int BARRIER_TILE = 1;
    public const int SAFE_TILE = 2;
    public const int PORTOL_TILE = 3;
    public const int MONSTER_TILE = 4;
    public const int BORN_TILE = 5;
    public const int REBORN_TILE = 6;
    public const int COLLECT_TILE = 7;
    public const int JUMP_TILE = 8;

    public bool Comfortable { get{return _ok; }set { _ok = value; } }
    public int Row { get { return _row; } }
    public int Col { get { return _col; } }
    public float Width { get { return _width; } }
    public float Height { get { return _height; } }
    public Vector2 TopLeftPoint { get { return _topLeftPoint; } }
    public Vector2 BottomRightPoint { get{ return _bottomRightPoint; } }
    public Vector3 Position3D { get{ return _3dPosition; }set { _3dPosition = value; } }
    public Vector2 Center { get{ return _center; } }

    public int _id = -1;

    private bool _ok = false;
    private int _row;
    private int _col;
    private int _type = 0;
    private float _width;
    private float _height;
    private Vector2 _topLeftPoint;
    private Vector2 _bottomRightPoint;
    private Vector3 _3dPosition;
    private Vector2 _center;

    private Square()
    {
    
    }

    /// <summary>
    /// 以左上顶点和长宽构造一个矩形
    /// </summary>
    /// <param name="topLeft"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Square( Vector2 topLeft , float width ,float height)
    {
        _topLeftPoint = topLeft;
        _width = width;
        _height = height;
        _bottomRightPoint = new Vector2(_topLeftPoint.x + width, _topLeftPoint.y - _height);
        _center = (_topLeftPoint + _bottomRightPoint) / 2;

        _row = (int)Mathf.Floor(_center.y / height);
        _col = (int)Mathf.Floor(_center.x / width);
    }

    /// <summary>
    /// 以左上顶点和边长构造一个正方形
    /// </summary>
    /// <param name="topLeft"></param>
    /// <param name="length"></param>
    public Square(Vector2 topLeft , float length)
    {
        _topLeftPoint = topLeft;
        _width = _height = length;
        _bottomRightPoint = new Vector2(_topLeftPoint.x + length, _topLeftPoint.y - length);
        _center = (_topLeftPoint + _bottomRightPoint) / 2;

        _row = (int)Mathf.Floor(_center.y / length);
        _col = (int)Mathf.Floor(_center.x / length);
    }

    /// <summary>
    /// 以3维位置点和长宽构造一个矩形
    /// </summary>
    /// <param name="position"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Square(Vector3 position, float width, float height)
    {
        _3dPosition = position;
        _center = new Vector2(position.x, position.z);
        _width = width;
        _height = height;
        _topLeftPoint = new Vector2(_center.x - _width / 2, _center.y + _height / 2);
        _bottomRightPoint = new Vector2(_center.x + _width / 2, _center.y - _height / 2);

        _row = (int)Mathf.Floor(_center.y / height);
        _col = (int)Mathf.Floor(_center.x / width);
    }

    /// <summary>
    /// 以3维位置点和边长构造一个正方形
    /// </summary>
    /// <param name="position"></param>
    /// <param name="length"></param>
    public Square(Vector3 position, float length)
    {
        _3dPosition = position;
        _center = new Vector2(position.x, position.z);
        _width = _height = length;
        _topLeftPoint = new Vector2(_center.x - _width / 2, _center.y + _height / 2);
        _bottomRightPoint = new Vector2(_center.x + _width / 2, _center.y - _height / 2);

        _row = (int)Mathf.Floor(_center.y / length);
        _col = (int)Mathf.Floor(_center.x / length);
    }

    public void Set3DPosition( float y )
    {
        _3dPosition = new Vector3( _center.x , y ,_center.y );
    }

    public Vector2[] Get4Points()
    {
        Vector2[] arr = new Vector2[4];
        arr[0] = _topLeftPoint;
        arr[1] = new Vector2(_topLeftPoint.x + _width, _topLeftPoint.y);
        arr[2] = _bottomRightPoint;
        arr[3] = new Vector2(_topLeftPoint.x, _topLeftPoint.y - _height);

        return arr;
    }

    public void SetType(int newType)
    {
        _type = newType;
    }

    public int GetType()
    {
        return _type;
    }
}
