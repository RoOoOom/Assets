using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class InputSettingPanel : MonoBehaviour {

    public GameObject _content;
    public GameObject _toggle;
    public GameObject _group;
    private GameObject[] _toggleGroup;

    InputConfigBase _inputConfig;

    private void Start()
    {
        string path = Application.dataPath + "/Resources/InputConfig.json";
        if (File.Exists(path))
        {
            StreamReader sr = new StreamReader(path);
            string json = sr.ReadToEnd();
            _inputConfig = JsonUtility.FromJson<InputConfigBase>(json);
            sr.Close();
        }

        int count = _inputConfig._keySet.Length;
        _toggleGroup = new GameObject[count];

        for (int i = 0; i < count; ++i)
        {
            GameObject clone = Instantiate(_toggle , _group.transform);

            string key = _inputConfig._keySet[i];
            KeyCode val = _inputConfig._valueSet[i];

            clone.transform.Find("CmdName").GetComponent<Text>().text = key;
            clone.transform.Find("keyCode").GetComponent<Text>().text = val.ToString();

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
                        KeyCode newCode = currentKey;
                        _toggleGroup[i].transform.Find("keyCode").GetComponent<Text>().text = newCode.ToString();
                        _toggleGroup[i].GetComponent<Toggle>().isOn = false;

                        _inputConfig._valueSet[i] = newCode;

                        SaveConfig();
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

    void SaveConfig()
    {
        string json = JsonUtility.ToJson(_inputConfig);
        string path = Application.dataPath + "/Resources/InputConfig.json";
        StreamWriter file = new StreamWriter(path);
        file.Write(json);
        file.Flush();
        file.Close();   
    }
}
