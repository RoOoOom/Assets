using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DoTweenAttacher : MonoBehaviour {
    private Transform _cachedTransform;
    public Transform cachedTransform
    {
        get { return _cachedTransform; }
    }

	// Use this for initialization
	void Start () {
        _cachedTransform = transform;
	}

    public void myDOMoveX(float end,float duration)
    {
        Tweener tweener = _cachedTransform.DOLocalMoveX(end, duration);
        tweener.SetUpdate(true);
        tweener.SetEase(Ease.Linear);
    }
}
