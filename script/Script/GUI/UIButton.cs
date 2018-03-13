using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButton : MonoBehaviour
{
    public enum ButtonType
    {
        A1,
        A2,
        B1,
        B2,
        C1,
        C2,
        D1,
        D2,
        Other,
        Skill
    }

    public ButtonType type = ButtonType.Other;
    public Button button
    {
        get
        {
            if (null == m_button) m_button = GetComponent<Button>();
            return m_button;
        }
        set 
        {
            m_button = value;
        }
    }
    private Button m_button;
    public Image image
    {
        get
        {
            if (null == m_image) m_image = GetComponent<Image>();
            return m_image;
        }
        set
        {
            m_image = value;
        }
    }
    private Image m_image = null;
    public Text text
    {
        get
        {
            if (null == m_text) m_text = GetComponentInChildren<Text>();
            return m_text;
        }
        set
        {
            m_text = value;
        }
    }
    private Text m_text = null;

    void Start()
    {
        if (null == gameObject.GetComponent<UIButtonScale>())
        {
            UIButtonScale buttonScale = gameObject.AddComponent<UIButtonScale>();
            buttonScale.tweenTarget = gameObject;
        }
    }
}
