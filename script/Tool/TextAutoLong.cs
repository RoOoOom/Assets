using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextAutoLong : MonoBehaviour {
    public const int MaxRow = 100;
    public const int RowPerPage = 10;

    private int m_curMaxRow = 0;

    // Update is called once per frame
    public void FixTextHeight()
    {
        m_curMaxRow++;

        if (m_curMaxRow <= RowPerPage) return;
        if (m_curMaxRow > MaxRow)
        {
            m_curMaxRow = MaxRow;
            return;
        }

        this.GetComponent<RectTransform>().sizeDelta = new Vector2(900f, m_curMaxRow * 40);
    }
}
