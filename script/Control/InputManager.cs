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

    CommandBase[] _CmdList = new CommandBase[MAX_KEYCOUNT];
    KeyCode[] _keycodeList = new KeyCode[MAX_KEYCOUNT];

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
        if (Input.GetKeyDown(_keycodeList[(int)CommandType.MoveForward]))
        {
            _CmdList[(int)CommandType.MoveForward].Excute();
        }
        else if (Input.GetKeyDown(_keycodeList[(int)CommandType.MoveBack]))
        {
            _CmdList[(int)CommandType.MoveBack].Excute();
        }
        else if (Input.GetKeyDown(_keycodeList[(int)CommandType.MoveLeft]))
        {
            _CmdList[(int)CommandType.MoveLeft].Excute();
        }
        else if (Input.GetKeyDown(_keycodeList[(int)CommandType.MoveRight]))
        {
            _CmdList[(int)CommandType.MoveRight].Excute();
        }


        if ( Input.GetKeyDown(_keycodeList[(int)CommandType.Attack]) )
        {
            _CmdList[(int)CommandType.Attack].Excute();
        }
        else if(Input.GetKeyDown(_keycodeList[(int)CommandType.Jump]))
        {
            _CmdList[(int)CommandType.Jump].Excute();
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

            for ( int i = 0;i<len;++i )
            {
                int index = (int)inputConfig._commandSet[i];
                _keycodeList[index] = inputConfig._valueSet[i];
            }
        }
        
    }
}
