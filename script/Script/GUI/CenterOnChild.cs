using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using LuaInterface;

//该脚本需要挂载在ScrollRect组件下
public class CenterOnChild : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public enum MovingType
    {
        Horizontal,
        Vertical,
    }

    public ScrollRect scrollRect = null;
    public RectTransform contentTransform = null;
    public int sensitivity = 50;
    public float space = 0f;
    public int pageContent = 0;     // 每页的item数量
    public MovingType type = MovingType.Horizontal;

    private LuaFunction m_moveStartAction = null;
    private LuaFunction m_moveEndAction = null;
    private float m_startDragPos;
    private int m_maxCenterIndex;
    private int m_centerIndex = 1;
    private int m_updateCounter = 0;
    private Vector3 m_originalPos = Vector3.zero;
    private Tweener m_tweener = null;

    void Start()
    {

        m_originalPos = contentTransform.localPosition;
    }

    void OnDestroy()
    {
        if (null != m_moveStartAction)
        {
            m_moveStartAction.Dispose();
            m_moveStartAction = null;
        }

        if (null != m_moveEndAction)
        {
            m_moveEndAction.Dispose();
            m_moveEndAction = null;
        }
    }

    void Update()
    {
        if (m_updateCounter == 0)
        {
            m_updateCounter = 1;
            return;
        }
        else if (m_updateCounter == 1)
        {
            m_updateCounter = 2;
            //使用GridLayoutGroup时，第二帧后才能拿到长宽信息
            if (type == MovingType.Horizontal)
            {
                this.Init(contentTransform.sizeDelta.x);
            }
            else
            {
                this.Init(contentTransform.sizeDelta.y);
            }
        }
    }

    /// <summary>
    /// 初始化最大行（列）数
    /// </summary>
    /// <param name="size"></param>
    public void Init(float size)
    {
        m_centerIndex = 1;
        //m_maxCenterIndex = Mathf.CeilToInt(size / space);
        int realCount = Mathf.CeilToInt(size / space);
        if (pageContent == 0)
        {
            m_maxCenterIndex = realCount;
        }
        else
        {
            m_maxCenterIndex = realCount-pageContent+1;
        }
    }

    public void SetMoveStartAction(LuaFunction func)
    {
        m_moveStartAction = func;
    }

    public void SetMoveEndAction(LuaFunction func)
    {
        m_moveEndAction = func;
    }

    private float M_GetPos(PointerEventData eventData)
    {
        if (type == MovingType.Horizontal)
        {
            return eventData.position.x;
        }
        else
        {
            return eventData.position.y;
        }
    }

    /// <summary>
    /// !!!: 该接口仅为IBeginDragHandler服务，其他地方不允许调用
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        m_startDragPos = M_GetPos(eventData);

        if (null != m_tweener)
        {
            m_tweener.Pause();
            m_tweener = null;
        }
    }

    /// <summary>
    /// !!!: 该接口仅为IEndDragHandler服务，其他地方不允许调用
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        scrollRect.StopMovement();
        float curPos = M_GetPos(eventData);
        if (type == MovingType.Horizontal)
        {
            if (m_centerIndex > 1 && curPos > m_startDragPos + sensitivity)
            {
                m_centerIndex--;
            }
            else if (m_centerIndex < m_maxCenterIndex && curPos < m_startDragPos - sensitivity)
            {
                m_centerIndex++;
            }
        }
        else
        {
            if (m_centerIndex < m_maxCenterIndex && curPos > m_startDragPos + sensitivity)
            {
                m_centerIndex++;
            }
            else if (m_centerIndex > 1 && curPos < m_startDragPos - sensitivity)
            {
                m_centerIndex--;
            }
        }

        MoveToIndex(m_centerIndex);
    }

    /// <summary>
    /// 移动到制定下表位置
    /// </summary>
    /// <param name="index"></param>
    public void MoveToIndex(int index)
    {
        if (m_centerIndex > 0 && m_centerIndex <= m_maxCenterIndex)
        {
            m_centerIndex = index;
            P_MoveStart();
            float centerPos = space * (m_centerIndex - 1);

            if (type == MovingType.Horizontal)
            {
                m_tweener = contentTransform.DOLocalMoveX(m_originalPos.x - centerPos, 0.5f, true).SetEase(Ease.OutQuint).OnComplete(P_MoveEnd);
            }
            else
            {
                m_tweener = contentTransform.DOLocalMoveY(m_originalPos.y + centerPos, 0.5f, true).SetEase(Ease.OutQuint).OnComplete(P_MoveEnd);
            }
        }
    }

    void P_MoveStart()
    {
        if (null != m_moveStartAction)
        {
            //LuaManager.instance.CallFunctionNoGC(m_moveStartAction, false, m_centerIndex);
            //m_moveStartAction.Call(m_centerIndex);
            m_moveStartAction.BeginPCall();
            m_moveStartAction.Push(m_centerIndex);
            m_moveStartAction.PCall();
            m_moveStartAction.EndPCall();
        }
    }

    void P_MoveEnd()
    {
        if (null != m_moveEndAction)
        {
            //LuaManager.instance.CallFunctionNoGC(m_moveEndAction, false, m_centerIndex);
           // m_moveEndAction.Call(m_centerIndex);
            m_moveEndAction.BeginPCall();
            m_moveEndAction.Push(m_centerIndex);
            m_moveEndAction.PCall();
            m_moveEndAction.EndPCall();
        }
    }
}