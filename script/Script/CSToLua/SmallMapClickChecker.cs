using UnityEngine;
using UnityEngine.EventSystems;
using LuaInterface;

/// <summary>
/// 小地图点击检测和拖拽脚本
/// 1. 该脚本需要挂载于小地图组件下，父层为Mask裁切组件
/// 2. 小地图左下端为锚点
/// </summary>
public class SmallMapClickChecker : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 m_mouseLastPos = Vector3.zero;
    private Vector3 m_meLastPos = Vector3.zero;
    private Vector3 m_originalPos = Vector3.zero;

    private Vector2 m_maskSize = Vector2.zero;
    private Vector2 m_meSize = Vector2.zero;

    private RectTransform m_rect;
    private RectTransform m_parentRect;

    private bool m_canDragHorizontal = false;
    private bool m_canDragVertial = false;

    private bool m_beenDragged = false;

    public LuaFunction click;
    void Awake()
    {
        m_rect = transform as RectTransform;
        m_originalPos = m_rect.localPosition;
        m_parentRect = m_rect.parent as RectTransform;
    }

    void OnDestroy()
    {
        if (null != click)
        {
            click.Dispose();
            click = null;
        }
    }

    /// <summary>
    /// 初始化小地图的长宽，以及是否可以拖拽判断
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void InitWidthAndHeight(int width, int height)
    {
        m_meSize = new Vector2(width, height);
        m_rect.sizeDelta = m_meSize;
        m_maskSize = m_parentRect.sizeDelta;
        m_canDragHorizontal = width > m_maskSize.x;
        m_canDragVertial = height > m_maskSize.y;

        //地图太小，居中显示
        Vector3 originalPos = Vector3.zero;
        if (!m_canDragHorizontal)
        {
            originalPos.x = m_originalPos.x + (m_maskSize.x - width) / 2;
        }

        if (!m_canDragVertial)
        {
            originalPos.y = m_originalPos.y + (m_maskSize.y - height) / 2;
        }

        m_rect.localPosition = originalPos;
        m_meLastPos = originalPos;
    }

    /// <summary>
    /// 初始化地图位置，用于打开界面时，默认显示玩家在小地图视野内等
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void InitPos(int x, int y)
    {
        Move(new Vector3(-x, -y, 0f));
    }

    /// <summary>
    /// !!!: lua端不可使用该接口
    /// </summary>
    public void OnPointerDown(PointerEventData data)
    {
        m_beenDragged = false;
        m_meLastPos = m_rect.localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parentRect, data.position, data.pressEventCamera, out m_mouseLastPos);
    }

    /// <summary>
    /// !!!: lua端不可使用该接口
    /// </summary>
    public void OnPointerUp(PointerEventData data)
    {
        if (!m_beenDragged)
        {
            if (null != click)
            {
                Vector3 pos = (Vector3)m_mouseLastPos - m_rect.localPosition;
                //LuaManager.instance.CallFunctionNoGC(click, false, pos.x, pos.y);
                click.BeginPCall();
                click.Push(pos.x);
                click.Push(pos.y);
                click.PCall();
                click.EndPCall();
                //click.Call(pos.x, pos.y);
            }

            //Debug.LogError("bbbbb: " + ((Vector3)m_mouseLastPos - m_rect.localPosition));
        }
    }

    /// <summary>
    /// !!!: lua端不可使用该接口
    /// </summary>
    public void OnDrag(PointerEventData data)
    {
        m_beenDragged = true;
        if (m_rect == null || m_parentRect == null || (!m_canDragVertial && !m_canDragHorizontal)) return;

        Vector2 mouseCurPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parentRect, data.position, data.pressEventCamera, out mouseCurPos))
        {
            Vector3 moveVec = mouseCurPos - m_mouseLastPos;
            Move(moveVec);
        }
    }

    void Move(Vector3 moveVec)
    {
        Vector3 toPos = m_meLastPos + moveVec;

        //横向边界判断
        if (!m_canDragHorizontal)
        {
            moveVec.x = 0;
        }
        else if (toPos.x > -m_maskSize.x / 2)
        {
            moveVec.x = -m_maskSize.x / 2 - m_meLastPos.x;
        }
        else if (toPos.x < (m_maskSize.x / 2 - m_meSize.x))
        {
            moveVec.x = m_maskSize.x / 2 - m_meSize.x - m_meLastPos.x;
        }

        //纵向边界判断
        if (!m_canDragVertial)
        {
            moveVec.y = 0;
        }
        else if (toPos.y > -m_maskSize.y / 2)
        {
            moveVec.y = -m_maskSize.y / 2 - m_meLastPos.y;
        }
        else if (toPos.y < (m_maskSize.y / 2 - m_meSize.y))
        {
            moveVec.y = m_maskSize.y / 2 - m_meSize.y - m_meLastPos.y;
        }

        m_rect.localPosition = m_meLastPos + moveVec;
    }
}

