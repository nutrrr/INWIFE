using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(Player player, PlayerData data) : base(player, data)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        player.Movement.RB.gravityScale = data.jumpGravityScale * data.fallingMultiplier;

        player.Movement.Move(player.PlayerInput.MoveXInput * data.accelerationInAir);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
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

        player.Movement.Move(player.PlayerInput.MoveXInput * data.accelerationInAir);

    }

    public override void DoChecks()
    {
        base.DoChecks();
    }
}
