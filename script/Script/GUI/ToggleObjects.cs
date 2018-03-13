using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ToggleObjects : MonoBehaviour
{
    public GameObject[] activeObjects;
    public GameObject[] inactiveObjects;
    private Toggle m_toggle;

    void Start()
    {
        m_toggle = gameObject.GetComponent<Toggle>();
        ValueChange(m_toggle.isOn);
        if (null != m_toggle) m_toggle.onValueChanged.AddListener(ValueChange);
    }

    void ValueChange(bool value)
    {
        ArrayUtils.ForEach(ref activeObjects, (it) => {
            it.gameObject.SetActive(value);
        });

        ArrayUtils.ForEach(ref inactiveObjects, (it) => {
            it.gameObject.SetActive(!value);
        });
    }

    void OnDestroy()
    {
        if (null != m_toggle) m_toggle.onValueChanged.RemoveListener(ValueChange);
    }
}
