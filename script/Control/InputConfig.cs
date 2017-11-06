using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public string[] _valueSet = new string[0];

    public int TryGetSetLength()
    {
        return _keySet.Length;
    }

    public void ClearKeySet()
    {

    }

    public void AddKeyValue(string key, string value)
    {
        string[] newKeySet = new string[_keySet.Length + 1];
        string[] newValueSet = new string[_keySet.Length + 1];

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
        string[] newValueSet = new string[_keySet.Length - 1];

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
    public void RefreshNewValue(int index , string newCode)
    {
        _valueSet[index] = newCode;
    }
}
