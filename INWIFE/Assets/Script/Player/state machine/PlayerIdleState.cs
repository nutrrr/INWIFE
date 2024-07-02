using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player, PlayerData data) : base(player, data)
    {

    }

    public override void Enter()
    {
        base.Enter();

        player.Movement.RB.gravityScale = data.defaultGravity;
        player.Movement.Move(0);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.PlayerInput.MoveXInput != 0)
        {
            player.ChangeState(player.MoveState);
            return;
        }

        if (player.CollisionSenses.isOnSlope)
        {
            player.ChangeState(player.SlopeState);
            return;
        }

        if (player.PlayerInput.HasBeenPressJump())
        {
            player.ChangeState(player.JumpState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        player.Movement.Move(0);
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }
}

