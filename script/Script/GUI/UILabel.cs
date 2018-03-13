using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UILabel : MonoBehaviour
{
    public enum LabelType
    {
        A,
        B,
        C,
        D,
        E,
        C01,
        C02,
        C03,
        C04,
        C05,
        C06,
        C07,
        Other,
    }

    public LabelType type = LabelType.Other;
    [HideInInspector]
    public Text text 
    {
        get
        {
            if (null == m_text) m_text = GetComponent<Text>();
            return m_text;
        }
        set
        {
            m_text = value;
        }
    }
    private Text m_text = null;
}
