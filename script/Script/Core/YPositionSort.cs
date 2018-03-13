using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//用于对当前场景物体进行深度排序
public class YPositionSort : MonoBehaviour {
    private Vector3 cachedPos = Vector3.zero; 

    void OnEnable()
    {
        if(YPositionSortManager.instance != null && !YPositionSortManager.instance.items.Contains(this))
        {
            YPositionSortManager.instance.items.Add(this);
        }
    }

    void OnDisable()
    {
        if (YPositionSortManager.instance != null && YPositionSortManager.instance.items.Contains(this))
        {
            YPositionSortManager.instance.items.Remove(this);
        }
    }

	// Use this for initialization
	void Start () {
        cachedPos = transform.localPosition;
	}

    public void sortDepth()
    {
        cachedPos = transform.localPosition;
        if (cachedPos.y > 0)
        {
            cachedPos.z = cachedPos.y / 10000f;
        }
        else
        {
            cachedPos.z = cachedPos.y / 10000f;
        }
        transform.localPosition = cachedPos;
    }
}
