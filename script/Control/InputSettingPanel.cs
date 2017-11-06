using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputSettingPanel : MonoBehaviour {

    public GameObject _content;
    public GameObject _toggle;
    public GameObject _group;
    private GameObject[] _toggleGroup; 
    private void Start()
    {
        int count = InputConfig.Instance.TryGetSetLength();
        _toggleGroup = new GameObject[count];

        for (int i = 0; i < count; ++i)
        {
            GameObject clone = Instantiate(_toggle , _group.transform);

            string key = InputConfig.Instance._keySet[i];
            string val = InputConfig.Instance._valueSet[i];

            clone.transform.Find("CmdName").GetComponent<Text>().text = key;
            clone.transform.Find("keyCode").GetComponent<Text>().text = val;

            clone.transform.localPosition -= (new Vector3(0f, 30f*i, 0f));
            clone.SetActive(true);

            _toggleGroup[i] = clone;
        }
    }

    /*
    private void Update()
    {
        if (Input.anyKeyDown)
        {s
            Event e = Event.current;
            if (e.isKey)
            {
                KeyCode currentKey = e.keyCode;
                Debug.Log("curretn key is :" + currentKey.ToString());
            }
        }
    }
    */
    private void OnGUI()
    {
        if (!_content.activeSelf) return;

        if (Input.anyKeyDown)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                KeyCode currentKey = e.keyCode;

                for (int i = 0; i < _toggleGroup.Length; ++i)
                {
                    if (_toggleGroup[i].GetComponent<Toggle>().isOn)
                    {
                        string newCode = currentKey.ToString();
                        _toggleGroup[i].transform.Find("keyCode").GetComponent<Text>().text = newCode;
                        _toggleGroup[i].GetComponent<Toggle>().isOn = false;

                        InputConfig.Instance.RefreshNewValue(i, newCode);
                    }
                }
                Debug.Log("curretn key is :" + currentKey.ToString());
            }
        }
    }

    public void SwitchContentStatus()
    {
        _content.SetActive(!_content.activeSelf);
    }
}
