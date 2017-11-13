using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class InputConfigBase
{
    public string[] _keySet = new string[0];
    // public string[] _valueSet = new string[0];
    public KeyCode[] _valueSet = new KeyCode[0];
}

public class InputConfig : ScriptableObject {
    private static InputConfig _instance;
    public static InputConfig Instance
    {
        get {
            if (!_instance)
            {
                _instance = Resources.Load<InputConfig>(ConfigName);
            }
            return _instance;
        }
    }

    public const string ConfigName = "InputConfig";
    public string[] _keySet = new string[0];
    //public string[] _valueSet = new string[0];
    public KeyCode[] _valueSet = new KeyCode[0];

    public int TryGetSetLength()
    {
        return _keySet.Length;
    }

    public void ClearKeySet()
    {
     
    }

    public void AddKeyValue(string key, KeyCode value)
    {
        string[] newKeySet = new string[_keySet.Length + 1];
        KeyCode[] newValueSet = new KeyCode[_keySet.Length + 1];

        for (int i = 0; i < _keySet.Length; ++i)
        {
            newKeySet[i] = _keySet[i];
            newValueSet[i] = _valueSet[i];
        }

        newKeySet[_keySet.Length] = key;
        newValueSet[_keySet.Length] = value;

        _keySet = newKeySet;
        _valueSet = newValueSet;
    }

    public void RemoveKey(int index)
    {
        string[] newKeySet = new string[_keySet.Length - 1];
        KeyCode[] newValueSet = new KeyCode[_keySet.Length - 1];

        for (int i = 0; i<_keySet.Length; ++i)
        {
            if (i == index) continue;

            newKeySet[i] = _keySet[i];
            newValueSet[i] = _valueSet[i];
        }

        _keySet = newKeySet;
        _valueSet = newValueSet;
    }
    /// <summary>
    /// 更新指令对应的键位
    /// </summary>
    /// <param name="index"></param>
    /// <param name="newCode"></param>
    public void RefreshNewValue(int index , KeyCode newCode)
    {
        _valueSet[index] = newCode;

        string json = JsonUtility.ToJson(_instance);
        string path = Application.dataPath + "/Resources/" + InputConfig.ConfigName + ".json";
        StreamWriter file = new StreamWriter(path);
        file.Write(json);
        file.Flush();
        file.Close(); 
    }
}
