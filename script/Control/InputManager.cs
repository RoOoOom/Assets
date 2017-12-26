using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommandObj;
using System;
using System.IO;

public class InputManager : MonoBehaviour {
    public const int MAX_KEYCOUNT = 10;

    public PlayerObj _playerObj;

    CmdAttack _cmdAttack = new CmdAttack();
    CmdJump _cmdJump = new CmdJump();
    CmdMoveBack _cmdMoveBack = new CmdMoveBack();
    CmdMoveForward _cmdMoveForward = new CmdMoveForward();
    CmdMoveLeft _cmdMoveLeft = new CmdMoveLeft();
    CmdMoveRight _cmdMoveRight = new CmdMoveRight();
    // Use this for initialization
    //暂时确定指令索引 0-攻击，1-跳跃，2-向前移动，3向后移动,4向左移动，5向右移动

    CommandBase[] _CmdList = new CommandBase[MAX_KEYCOUNT];
    Dictionary<KeyCode , CommandType> _keyList = new Dictionary<KeyCode, CommandType>();
    KeyCode[] _keycodeList;

    void Start () {
        if (_playerObj == null)
        {
            _playerObj = GameObject.Find("Player").GetComponent<PlayerObj>();
        }

        if (_playerObj == null)
        {
            Debug.Log("找不到玩家对象");
        }

        int n = Enum.GetValues(typeof(KeyCode)).Length;

        InitCommandList();
        ReadInputConfig();
	}
	
	// Update is called once per frame
	void Update () {
        KeyCheckA();
        //KeyCheckB();
    }

    void KeyCheckA()
    {
        for (int i = 0; i < _keycodeList.Length; ++i)
        {
            if (Input.GetKeyDown(_keycodeList[i]))
            {
                CommandType ct;
                if (_keyList.TryGetValue(_keycodeList[i], out ct))
                {
                    _CmdList[(int)ct].Excute();
                }

            }
        }
    }

    void KeyCheckB()
    {
        
    }

    private void OnDisable()
    {

    }

    void InitCommandList()
    {
        for (int i = 0; i < MAX_KEYCOUNT; ++i)
            _CmdList[i] = new CommandBase();

        _CmdList[(int)CommandType.Attack].ProcessAction += _playerObj.Attack;
        _CmdList[(int)CommandType.Jump].ProcessAction += _playerObj.Jump;
        _CmdList[(int)CommandType.MoveForward].ProcessAction += _playerObj.MoveForward;
        _CmdList[(int)CommandType.MoveBack].ProcessAction += _playerObj.MoveBack;
        _CmdList[(int)CommandType.MoveLeft].ProcessAction += _playerObj.MoveLeft;
        _CmdList[(int)CommandType.MoveRight].ProcessAction += _playerObj.MoveRight;
    }

    void InitBplan()
    {
        
    }

    void ReadInputConfig()
    {
        string path = Application.dataPath + "/Resources/InputConfig.json";
        if (File.Exists(path))
        {
            StreamReader sr = new StreamReader(path);
            string json = sr.ReadToEnd();
            InputConfigBase inputConfig = JsonUtility.FromJson<InputConfigBase>(json);
            sr.Close();

            int len = inputConfig._commandSet.Length;
            Debug.Log(len);
            Debug.Break();

            for (int i = 0; i < len; ++i)
            {
                _keyList.Add(inputConfig._valueSet[i], inputConfig._commandSet[i]);
            }

            _keycodeList = inputConfig._valueSet;
        }
        
    }
}
