using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//用于对所有场景物体进行深度排序的管理器
public class YPositionSortManager : MonoBehaviour {
    public static float SORT_TIME = float.MaxValue;

    private static YPositionSortManager m_instance = null;
    public static YPositionSortManager instance
    {
        get
        {
            if (null == m_instance &&  null != GameWorld.instance)
            {
                m_instance = GameWorld.instance.gameObject.AddComponent<YPositionSortManager>();
            }
            return m_instance;
        }
    }

    public float loopInterval = 0.2f;
    private float time = 0;
    private float sortTime = SORT_TIME;
    public List<YPositionSort> items = new List<YPositionSort>();

    void Awake()
    {
        m_instance = this;
    }

    public void SetTime(float time)
    {
        SORT_TIME = time;
        sortTime = time;
    }

    float y1, y2;
	// Update is called once per frame
	void Update ()
    {
        time += Time.deltaTime;
        if (time >= loopInterval)
        {
            time = 0;
            for (int i = 0; i < items.Count; i++)
            {
                items[i].sortDepth();
            }
        }

        sortTime -= Time.deltaTime;
        if (sortTime <= 0)
        {
            sortTime = SORT_TIME;
            // 暂时这么处理，还没想到更好的解决方式
            items.Sort(delegate(YPositionSort a, YPositionSort b)
            {
                y1 = a.transform.localPosition.y;
                y2 = b.transform.localPosition.y;
                if (y1 > y2)
                {
                    return -1;
                }
                else if (y1 == y2)
                {
                    return 0;
                }
                return 1;
            });

            for (int i = 0; i < items.Count; i++)
            {
                items[i].transform.SetSiblingIndex(i);
            }
        }
	}
}
