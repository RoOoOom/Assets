using UnityEngine;
using UnityEngine.UI;

public class FitDisableDrag : MonoBehaviour
{

    ScrollRect scrollRect 
    {
        get
        {
            if (null == m_scrollRect)
            {
                m_scrollRect = GetComponent<ScrollRect>();
            }

            return m_scrollRect;
        }
    }
    private ScrollRect m_scrollRect = null;
    private Vector2 m_viewportSize = Vector2.zero;
    private Vector2 m_contentSize = Vector2.zero;

    private Vector2 m_tvs = Vector2.zero;
    private Vector2 m_tcs = Vector2.zero;

    void Update()
    {
        m_tvs = scrollRect.viewport.rect.size;
        m_tcs = scrollRect.content.rect.size;

        if (m_tvs != m_viewportSize || m_tcs != m_contentSize)
        {
            m_viewportSize = m_tvs;
            m_contentSize = m_tcs;
            SetDirty();
        }
    }


    void SetDirty()
    {
        //Debug.LogError(m_viewportSize + " => " + m_contentSize);
        scrollRect.horizontal = m_viewportSize.x < m_contentSize.x;
        scrollRect.vertical = m_viewportSize.y < m_contentSize.y;
    }
}
