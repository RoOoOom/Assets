using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommandObj;
using System.IO;

public class InputManager : MonoBehaviour {
    public const int MAX_KEYCOUNT = 4;
    public const int Attack_key = 0;
    public const int Jump_key = 1;
    public const int MoveFoward_key = 2;
    public const int MoveBack_key = 3;

    public PlayerObj _playerObj;

    CmdAttack _cmdAttack = new CmdAttack();
    CmdJump _cmdJump = new CmdJump();
    CmdMoveBack _cmdMoveBack = new CmdMoveBack();
    CmdMoveForward _cmdMoveForward = new CmdMoveForward();
    CmdMoveLeft _cmdMoveLeft = new CmdMoveLeft();
    CmdMoveRight _cmdMoveRight = new CmdMoveRight();
    // Use this for initialization
    //暂时确定指令索引 0-攻击，1-跳跃，2-向前移动，3向后移动

    CommandBase[] _CmdList = new CommandBase[MAX_KEYCOUNT];
    KeyCode[] _keyList = new KeyCode[MAX_KEYCOUNT];
	void Start () {
        if (_playerObj == null)
        {
            _playerObj = GameObject.Find("Player").GetComponent<PlayerObj>();
        }

        if (_playerObj == null)
        {
            Debug.Log("找不到玩家对象");
        }

        //InitCommandList();
        ReadInputConfig();
	}
	
	// Update is called once per frame
	void Update () {
        //KeyCheckA();
        KeyCheckB();
    }

    void KeyCheckA()
    {
        if (Input.GetKeyDown(_keyList[Attack_key]))
        {
            _CmdList[Attack_key].Excute();
        }
        else if (Input.GetKeyDown(_keyList[Jump_key]))
        {
            _CmdList[Jump_key].Excute();
        }

        if (Input.GetKeyDown(_keyList[MoveFoward_key]))
        {
            _CmdList[MoveFoward_key].Excute();
        }
        else if (Input.GetKeyDown(_keyList[MoveBack_key]))
        {
            _CmdList[MoveBack_key].Excute();
        }
    }

    void KeyCheckB()
    {
        if (Input.GetKeyDown(_keyList[Attack_key]))
        {
           _cmdAttack.Excute(_playerObj);
        }
        else if (Input.GetKeyDown(_keyList[Jump_key]))
        {
            _cmdJump.Excute(_playerObj);
        }

        if (Input.GetKeyDown(_keyList[MoveFoward_key]))
        {
            _cmdMoveForward.Excute(_playerObj);
        }
        else if (Input.GetKeyDown(_keyList[MoveBack_key]))
        {
            _cmdMoveBack.Excute(_playerObj);
        }
    }

    private void OnDisable()
    {

    }

    void InitCommandList()
    {
        _CmdList[0].ProcessAction += _playerObj.Attack;
        _CmdList[1].ProcessAction += _playerObj.Jump;
        _CmdList[2].ProcessAction += _playerObj.MoveForward;
        _CmdList[3].ProcessAction += _playerObj.MoveBack;
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

            int len = Mathf.Min(MAX_KEYCOUNT, inputConfig._valueSet.Length);

            for (int i = 0; i < len; ++i)
            {
                _keyList[i] = inputConfig._valueSet[i];
            }
        }
        
    }
}
