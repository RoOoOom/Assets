using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommandObj;

public class PlayerObj : MonoBehaviour {
    public CharacterController _characterController;

    private void Start()
    {
        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }
    }

    public void RecieverCommand( CommandBase cmd )
    {
        cmd.Excute( this );
    }

    public void Jump()
    {
        Debug.Log("Jump!!");
    }

    public void Attack()
    {
        Debug.Log("Attack!!");
    }

    public void MoveLeft()
    {
        _characterController.Move(new Vector3(-2f, 0f, 0f));
    }

    public void MoveRight()
    {
        _characterController.Move(new Vector3(2f, 0f, 0f));
    }

    public void MoveForward()
    {
        _characterController.Move(new Vector3(0f, 0f, 2f));
    }

    public void MoveBack()
    {
        _characterController.Move(new Vector3(0f, 0f, -2f));
    }
}
