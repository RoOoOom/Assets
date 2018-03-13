using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmTipsPanel : MonoBehaviour
{
    private Text m_txtTips;

    private Button m_btnCancel;
    private Button m_btnConfrim;
    private Button m_btnOK;

    private Action m_ok;
    private Action m_cancel;

    void Awake()
    {
        m_txtTips = Util.GetChildGameObject(gameObject, "txtTips", true).GetComponent<Text>();
        m_btnCancel = Util.GetChildGameObject(gameObject, "btnCancel", true).GetComponent<Button>();
        m_btnConfrim = Util.GetChildGameObject(gameObject, "btnConfirm", true).GetComponent<Button>();
        m_btnOK = Util.GetChildGameObject(gameObject, "btnOK", true).GetComponent<Button>();
    }

    void Start()
    {
        m_btnOK.onClick.AddListener(() => { m_ok.Invoke(); });
        m_btnCancel.onClick.AddListener(() => { m_cancel.Invoke(); });
        m_btnConfrim.onClick.AddListener(() => { m_ok.Invoke(); });
    }


    public void Init(string tips, Action ok, Action cancel, bool hasCancel = false)
    {
        m_btnCancel.gameObject.SetActive(hasCancel);
        m_btnConfrim.gameObject.SetActive(hasCancel);
        m_btnOK.gameObject.SetActive(!hasCancel);

        m_txtTips.text = tips;
        m_ok = ok;
        m_cancel = cancel;
    }

    public static void Show(string tips, Action ok, Action cancel = null, bool hasCancel = false)
    {
        GameObject goTips = GameObject.Find("Canvas/ConfirmTipsPanel");
        if (goTips == null)
        {
            goTips = GameObject.Instantiate(Resources.Load<GameObject>("UI/Confirm/ConfirmTipsPanel")) as GameObject;
            Util.BindParent(GameObject.Find("Canvas").transform, goTips.transform);
        }
        ConfirmTipsPanel panel = Util.GetOrAddComponent<ConfirmTipsPanel>(goTips);
        panel.Init(tips, ok, cancel, hasCancel);
}
}
