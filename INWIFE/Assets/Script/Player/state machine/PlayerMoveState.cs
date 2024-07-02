using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerState
{
    // Input
    private float moveX;


    // State
    private bool isGrounded;
    private bool isJumping;
    private bool isAiming;
    private bool isReset;


    public PlayerMoveState(Player player, PlayerData data) : base(player, data)
    {

    }

    public override void Enter()
    {
        base.Enter();

        player.Movement.Move(player.PlayerInput.MoveXInput);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        //ChangeState
        if (player.PlayerInput.MoveXInput == 0)
        {
            player.ChangeState(player.IdleState);
            return;
        }

        if (player.CollisionSenses.isOnSlope)
        {
            player.ChangeState(player.SlopeState);
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

        // Set gravity
        player.Movement.RB.gravityScale = data.defaultGravity;

        player.Movement.Move(player.PlayerInput.MoveXInput);
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }


    
}
