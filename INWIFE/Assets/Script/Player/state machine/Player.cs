using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerState CurrentState; // Initial state

    public PlayerData PlayerData;
    public Movement Movement;
    public CollisionSenses CollisionSenses;
    public PlayerInput PlayerInput;
    public PlayerGun PlayerGun;

    public PlayerAimState AimState { get; private set; }
    public PlayerAimResetState AimResetState { get; private set; }
    public PlayerAirState AirState { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerSlopeState SlopeState { get; private set; }


    public void InitializeState(PlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        Debug.Log("ChangeState to " + newState.ToString());
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    private void Awake()
    {
        AimResetState = new PlayerAimResetState(this, PlayerData);
        AimState = new PlayerAimState(this, PlayerData);
        AirState = new PlayerAirState(this, PlayerData);
        IdleState = new PlayerIdleState(this, PlayerData);
        JumpState = new PlayerJumpState(this, PlayerData);
        LandState = new PlayerLandState(this, PlayerData);
        MoveState = new PlayerMoveState(this, PlayerData);
        SlopeState = new PlayerSlopeState(this, PlayerData);
    }
    private void Start()
    {
        Movement = GetComponent<Movement>();
        CollisionSenses = GetComponent<CollisionSenses>();
        PlayerInput = GetComponent<PlayerInput>();
        PlayerGun = GetComponent<PlayerGun>();

        InitializeState(IdleState);
    }

    private void Update()
    {
        Movement.LogicUpdate();
        CollisionSenses.LogicUpdate();
        PlayerInput.LogicUpdate();

        
        CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        CurrentState.PhysicsUpdate();
    }
}
