using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimResetState : PlayerState
{
    public bool isDone = false;

    public PlayerAimResetState(Player player, PlayerData data) : base(player, data)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();

        isDone = false;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (isDone)
        {
            player.ChangeState(player.AimState);
            return;
        }

        isDone = true;
        player.Movement.RB.rotation = 0;
        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
