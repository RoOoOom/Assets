using UnityEngine;
using UnityEngine.EventSystems;
using LuaInterface;
using UnityEngine.UI;
using DG.Tweening;

public class SwitchSolulTarget : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
{

    private Vector2 m_lastPos;
    private Camera m_uiCamera = null;
    private float m_radiusPow = 179;
    public LuaFunction switchsoluTarget;

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
        float angle = 0;
        Vector3 wPos = m_uiCamera.ScreenToWorldPoint(eventData.position);
        Vector3 lPos = transform.InverseTransformPoint(wPos);
        if (Vector2.Distance(lPos, Vector2.zero) <= m_radiusPow) {
            angle = Mathf.Atan2((lPos.y), (lPos.x)) * 180 / Mathf.PI;
        }
        if (null != switchsoluTarget)
        {
            //LuaManager.instance.CallFunctionNoGC(switchsoluTarget, false, angle);
            //switchsoluTarget.Call(angle);
            switchsoluTarget.BeginPCall();
            switchsoluTarget.Push(angle);
            switchsoluTarget.PCall();
            switchsoluTarget.EndPCall();
        }
            
    }

    public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    {
        
    }
    //控制目标选中底图显示
    //public void Show(int index)
    //{
      //  for (int i = 0; i < 8; i++){
        //    listimage[i].enabled = false;
        //}
       // listimage[index].enabled = true;
   // }

    void OnDestroy()
    {
        if (null != switchsoluTarget)
        {
            switchsoluTarget.Dispose();
            switchsoluTarget = null;
        }
    }
}
