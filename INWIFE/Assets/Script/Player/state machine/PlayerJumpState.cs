using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private bool isJumpCut = false;

    public PlayerJumpState(Player player, PlayerData data) : base(player, data)
    {
    }
    public override void Enter()
    {
        base.Enter();

        player.Movement.RB.gravityScale = data.jumpGravityScale;

        isJumpCut = false;

        player.Movement.Jump();
        player.PlayerInput.ResetJumpBufferCounter();
        player.CollisionSenses.JumpHasDone();

        player.Movement.Move(player.PlayerInput.MoveXInput * data.accelerationInAir);
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
            
        if(player.Movement.RB.velocity.y < 0)
        {
            player.ChangeState(player.AirState);
            return;
        }
        if (player.PlayerInput.HasBeenPressAim())
        {
            player.ChangeState(player.AimState);
            return;
        }
        if (player.CollisionSenses.IsGrounded())
        {
            player.ChangeState(player.LandState);
            return;
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();


        if(player.Movement.RB.velocity.y < data.jumpHangTimeThreshold)
        {
            player.Movement.Move(player.PlayerInput.MoveXInput * data.accelerationInAir * data.jumpHangAccelerationMult);
            player.Movement.RB.gravityScale = data.jumpGravityScale * data.jumpHangGravityMult;
        }
        else
        {
            player.Movement.Move(player.PlayerInput.MoveXInput * data.accelerationInAir);
        }
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }
}

