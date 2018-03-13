using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject tweenTarget;
    public float pressScale = 0.95f;
    public float duration = 0.15f;

    private Vector3 _originalScale = Vector2.one;

    void Start()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (null != tweenTarget)
        {
            tweenTarget.transform.DOScale(pressScale * _originalScale.x, duration);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (null != tweenTarget)
        {
            tweenTarget.transform.DOScale(_originalScale.x, duration);
        }
    }
}
