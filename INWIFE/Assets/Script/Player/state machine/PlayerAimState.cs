using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimState : PlayerState
{
    private float lastSpinDirection;
    public PlayerAimState(Player player, PlayerData data) : base(player, data)
    {

    }
    public override void Enter()
    {
        base.Enter();

        Time.timeScale = 0.66f;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;

        player.PlayerInput.ResetAimBufferCounter();
        

        Aiming();
    }

    public override void Exit()
    {
        base.Exit();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.PlayerInput.AimInputRelease)
        {
            player.ChangeState(player.AirState);
            return;
        }
        if (player.PlayerInput.ResetAimingInput)
        {
            player.ChangeState(player.AimResetState);
            return;
        }
        if (player.CollisionSenses.IsGrounded())
        {
            player.ChangeState(player.LandState);
            return;
        }

        if (player.Movement.RB.velocity.y <= 0)
        {
            player.Movement.RB.gravityScale = data.jumpGravityScale * data.fallingMultiplier;
        }


        if (player.PlayerInput.ShootInput)
        {
            player.PlayerGun.Shoot();
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        Aiming();
    }

    public override void DoChecks()
    {
    }
    void Aiming()
    {
        if (lastSpinDirection == 0)
        {
            lastSpinDirection = player.PlayerInput.MoveXInput;

        }
        player.Movement.RB.rotation += player.PlayerInput.MoveXInput * player.PlayerData.RotationAimSpeed * Time.fixedDeltaTime;

       
    }

}

