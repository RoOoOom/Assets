using System;
using UnityEngine;
using UnityEngine.UI;

public class UpdateNoticePanel : MonoBehaviour
{
    private Text m_txtContent;

    private Button m_btnEnter;

    private Action m_ok;

    void Awake()
    {
        m_txtContent = Util.GetChildGameObject(gameObject, "txtContent", true).GetComponent<Text>();
        m_btnEnter = Util.GetChildGameObject(gameObject, "btnEnter", true).GetComponent<Button>();
    }

    void Start()
    {
        m_btnEnter.onClick.AddListener(() => { if (null != m_ok) m_ok.Invoke(); });
    }


    public void Init(string notice, Action ok)
    {
        m_txtContent.text = notice;
        m_ok = ok;
    }

    public static void Show(string tips, Action ok)
    {
        GameObject goTips = GameObject.Find("Canvas/UpdateNoticePanel");
        if (goTips == null)
        {
            goTips = GameObject.Instantiate(Resources.Load<GameObject>("UI/Update/UpdateNoticePanel")) as GameObject;
            Util.BindParent(GameObject.Find("Canvas").transform, goTips.transform);
        }
        UpdateNoticePanel panel = Util.GetOrAddComponent<UpdateNoticePanel>(goTips);
        panel.Init(tips, ok);
    }
}
