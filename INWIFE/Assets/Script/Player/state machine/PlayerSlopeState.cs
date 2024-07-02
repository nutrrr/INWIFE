using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlopeState : PlayerState
{
    public PlayerSlopeState(Player player, PlayerData data) : base(player, data)
    {

    }
    public override void Enter()
    {
        base.Enter();

        player.Movement.Move(player.PlayerInput.MoveXInput, player.CollisionSenses.SlopeNormalPerp);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if (!player.CollisionSenses.isOnSlope && player.PlayerInput.MoveXInput == 0)
        {
            player.ChangeState(player.IdleState);
            return;
        }
        if (!player.CollisionSenses.isOnSlope && player.PlayerInput.MoveXInput == 1)
        {
            player.ChangeState(player.MoveState);
            return;
        }
        if (!player.CollisionSenses.IsGrounded())
        {
            player.ChangeState(player.AirState);
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

        player.Movement.Move(player.PlayerInput.MoveXInput, player.CollisionSenses.SlopeNormalPerp);
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

}
