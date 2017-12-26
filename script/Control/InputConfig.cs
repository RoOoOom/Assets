using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum CommandType {
    None,
    Attack,
    Jump,
    MoveForward,
    MoveBack,
    MoveLeft,
    MoveRight,
    Turnarround,
    Eat
}

[System.Serializable]
public class InputConfigBase
{
    public string[] _keySet = new string[0];
    public CommandType[] _commandSet = new CommandType[0];
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
    public CommandType[] _commandSet = new CommandType[0];
    public string[] _keySet = new string[0];
    //public string[] _valueSet = new string[0];
    public KeyCode[] _valueSet = new KeyCode[0];

    public int TryGetSetLength()
    {
        // return _keySet.Length;
        return _commandSet.Length;
    }

    public void ClearKeySet()
    {
     
    }

    /// <summary>
    /// 添加指令和对应的按键
    /// </summary>
    /// <param name="commandType">指令类型</param>
    /// <param name="value">按键</param>
    public void AddKeyValue( CommandType commandType , KeyCode value )
    {
        CommandType[] newCmdSet = new CommandType[_commandSet.Length + 1];
        KeyCode[] newValueSet = new KeyCode[_valueSet.Length + 1];

        for (int i = 0;i < _commandSet.Length ;++i )
        {
            newCmdSet[i] = _commandSet[i];
            newValueSet[i] = _valueSet[i];
        }

        newCmdSet[_commandSet.Length] = commandType;
        newValueSet[_valueSet.Length] = value;

        _commandSet = newCmdSet;
        _valueSet = newValueSet;
    }

    /// <summary>
    /// 添加指令名和按键
    /// </summary>
    /// <param name="key">指令名</param>
    /// <param name="value">按键</param>
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
    
    /// <summary>
    /// 移除指令
    /// </summary>
    /// <param name="index">指令对应的索引</param>
    public void RemoveKey(int index)
    {
        string[] newKeySet = new string[_keySet.Length - 1];
        KeyCode[] newValueSet = new KeyCode[_keySet.Length - 1];

        int n = 0;
        for (int i = 0; i<_keySet.Length; ++i)
        {
            if (i == index) continue;

            newKeySet[n] = _keySet[i];
            newValueSet[n] = _valueSet[i];
            n++;
        }

        _keySet = newKeySet;
        _valueSet = newValueSet;
    }

    /// <summary>
    /// 移除指令
    /// </summary>
    /// <param name="index">指令对应的索引</param>
    public void RemoveKeyByIndex(int index)
    {
        CommandType[] newCmdSet = new CommandType[_commandSet.Length - 1];
        KeyCode[] newValueSet = new KeyCode[_valueSet.Length - 1];

        int n = 0;
        for (int i = 0; i < _commandSet.Length; ++i)
        {
            if (i == index) continue;

            newCmdSet[n] = _commandSet[i];
            newValueSet[n] = _valueSet[i];
            n++;
        }
    
        _commandSet = newCmdSet;
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
