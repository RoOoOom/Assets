using UnityEngine;
using System.Collections;
using DG.Tweening;
using LuaInterface;

public class TweenTools : MonoBehaviour {

    public Transform textTransform = null;
    private CanvasGroup textCanvas = null;

    private LuaFunction m_moveEndAction = null;

	// Use this for initialization
	void Start () {
        
	}

    public void Play()
    {
        textCanvas = textTransform.GetComponent<CanvasGroup>();
        textCanvas.DOFade(1, 0.6f);
        CancelInvoke("Hide");
        Invoke("Hide", 1.5f);
    }

    private void Hide()
    {
        textCanvas.DOFade(0, 0.6f);
    }

    public void SetMoveEndAction(LuaFunction func)
    {
        m_moveEndAction = func;
    }

    public void Move(float endPos, float duration, int index)
    {
        Tweener tweener = transform.DOLocalMoveX(endPos, duration);
        tweener.SetEase(Ease.Linear).OnComplete(() => { P_MoveEnd(index); });
    }

    private void P_MoveEnd(int info)
    {
        if (null != m_moveEndAction)
        {
         //   LuaManager.instance.CallFunctionNoGC(m_moveEndAction, false, info);
            //m_moveEndAction.Call(info);
            m_moveEndAction.BeginPCall();
            m_moveEndAction.Push(info);
            m_moveEndAction.PCall();
            m_moveEndAction.EndPCall();
        }
    }
}
