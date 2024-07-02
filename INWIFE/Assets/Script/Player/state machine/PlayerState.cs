using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected Player player;
    protected PlayerData data;
    protected bool isExitingState;

    public PlayerState(Player player, PlayerData data)
    {
        this.player = player;
        this.data = data;
    }

    public virtual void Enter()
    {

    }

    public virtual void Exit()
    {

    }

    public virtual void LogicUpdate()
    {

    }

    public virtual void PhysicsUpdate()
    {

    }

    public virtual void DoChecks()
    {

    }

}
