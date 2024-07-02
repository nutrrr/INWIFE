using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandState : PlayerState
{
    public PlayerLandState(Player player, PlayerData data) : base(player, data)
    {

    }
    public override void Enter()
    {
        base.Enter();

        if(player.Movement.RB.rotation != 0)
        {
            player.Movement.RB.rotation = 0;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.PlayerInput.MoveXInput == 0)
        {
            player.ChangeState(player.IdleState);
            return;
        }
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
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

}
