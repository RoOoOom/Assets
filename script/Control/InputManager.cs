using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommandObj;

public class InputManager : MonoBehaviour {

    public PlayerObj _playerObj;

    CmdAttack _cmdAttack;
    CmdJump _cmdJump;
    CmdMoveBack _cmdMoveBack;
    CmdMoveForward _cmdMoveForward;
    CmdMoveLeft _cmdMoveLeft;
    CmdMoveRight _cmdMoveRight;
	// Use this for initialization

    
	void Start () {
        if (_playerObj == null)
        {
            _playerObj = GameObject.Find("Player").GetComponent<PlayerObj>();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
