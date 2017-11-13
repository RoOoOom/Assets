using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandObj {
    public class CommandBase {
        public delegate void ActionDelegate();
        public event ActionDelegate ProcessAction;


        /// <summary>
        /// 执行指令
        /// </summary>
        /// <param name="player"></param>
        public virtual void Excute( PlayerObj player )
        {

        }

        /// <summary>
        /// 执行指令
        /// </summary>
        public virtual void Excute()
        {
            if (ProcessAction != null)
            {
                ProcessAction();
            }
        }
    }


    public class CmdAttack:CommandBase
    {
        public override void Excute(PlayerObj player)
        {
            player.Attack();
        }
    }

    public class CmdJump : CommandBase
    {
        public override void Excute(PlayerObj player)
        {
            player.Jump();
        }
    }

    public class CmdMoveForward : CommandBase
    {
        public override void Excute(PlayerObj player)
        {
            player.MoveForward();
        }
    }

    public class CmdMoveBack : CommandBase
    {
        public override void Excute(PlayerObj player)
        {
            player.MoveBack();
        }
    }

    public class CmdMoveLeft : CommandBase
    {
        public override void Excute(PlayerObj player)
        {
            player.MoveLeft();
        }
    }

    public class CmdMoveRight : CommandBase
    {
        public override void Excute(PlayerObj player)
        {
            player.MoveRight();
        }
    }
}