using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandObj {
    public class CommandBase {
        public virtual void Excute( PlayerObj player )
        {

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