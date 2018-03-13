using UnityEngine;
using UnityEngine.EventSystems;
using LuaInterface;
using UnityEngine.UI;
using DG.Tweening;

public class SwitchTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 m_lastPos;
    private Camera m_uiCamera = null;
    private float m_radiusPow = 0f;
    private float m_sensitivity = 15;

    public Image up;
    public Image down;
    public LuaFunction switchTarget;

    void SetImageAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    float Square(float v)
    {
        return v * v;
    }

    void Start()
    {
        m_radiusPow = Square(transform.rectTransform().sizeDelta.x / 2f);

        if (null == m_uiCamera)
        {
            for (int i = 0; i < Camera.allCamerasCount; i++)
            {
                if ("UICamera".Equals(Camera.allCameras[i].tag))
                {
                    m_uiCamera = Camera.allCameras[i];
                    break;
                }
            }
        }
    }

    public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_lastPos = eventData.position;
    }

    public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    {
        int trigger = 0;
        Vector3 vec = eventData.position - m_lastPos;
        if (vec.y > m_sensitivity)
        {
            Show(true);
            trigger = 1;
        }
        else if (vec.y < -m_sensitivity)
        {
            Show(false);
            trigger = 2;
        }

       /* Vector3 wPos = m_uiCamera.ScreenToWorldPoint(eventData.position);
        Vector3 lPos = transform.InverseTransformPoint(wPos);
        if (Square(lPos.x) + Square(lPos.y) > m_radiusPow)
        {
            Vector2 vec = eventData.position - m_lastPos;
            if (vec.y > 0)
            {
                Show(true);
                trigger = 1;
            }
            else
            {
                Show(false);
                trigger = 2;
            }
        }*/
        if (null != switchTarget)
        {
            //LuaManager.instance.CallFunctionNoGC(switchTarget, false, trigger);
         //   switchTarget.Call(trigger);
            switchTarget.BeginPCall();
            switchTarget.Push(trigger);
            switchTarget.PCall();
            switchTarget.EndPCall();
        }
    }

    public void Show(bool isUp)
    {
        up.enabled = isUp;
        down.enabled = !isUp;
    }

    void OnDestroy()
    {
        if (null != switchTarget)
        {
            switchTarget.Dispose();
            switchTarget = null;
        }
    }
}
