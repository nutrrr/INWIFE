using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private PlayerData data;

    public float MoveXInput { get; private set; } //AD
    public float MoveYInput { get; private set; } //WS

    private bool jumpInput;
    public bool JumpInputRelease { get; private set; }

    private bool aimInput;
    public bool AimInputRelease { get; private set; }

    public bool ResetAimingInput { get; private set; }

    public bool ShootInput { get; private set; }
    public bool ShootInputRelease { get; private set; }

    public bool trickInput;
    public bool TrickInputRelease { get; private set; }

    public float JumpBufferCounter { get; private set; }
    public float AimBufferCounter { get; private set; }

    private void Awake()
    {
        data = GetComponent<Player>().PlayerData;
    }

    public void LogicUpdate()
    {
        MoveXInput = Input.GetAxisRaw("Horizontal");
        MoveYInput = Input.GetAxisRaw("Vertical");

        jumpInput = Input.GetKeyDown(KeyCode.Space);
        JumpInputRelease = Input.GetKeyUp(KeyCode.Space);

        aimInput = Input.GetKeyDown(KeyCode.J);
        AimInputRelease = Input.GetKeyUp(KeyCode.J);

        ResetAimingInput = Input.GetKeyDown(KeyCode.L);

        ShootInput = Input.GetKeyDown(KeyCode.S);
        ShootInputRelease = Input.GetKeyUp(KeyCode.S);
        
        trickInput = Input.GetKeyDown(KeyCode.K);
        TrickInputRelease = Input.GetKeyUp(KeyCode.K);

        if (jumpInput)
        {
            JumpBufferCounter = data.jumpBuffer;
        }

        if (aimInput)
        {
            AimBufferCounter = data.shootBuffer;
        }


        JumpBufferCounter -= Time.deltaTime;
        AimBufferCounter -= Time.deltaTime;
    }

    public bool HasBeenPressJump()
    {
        return JumpBufferCounter > 0;
    }
    
    public bool HasBeenPressAim()
    {
        return AimBufferCounter > 0;
    }

    public void ResetJumpBufferCounter()
    {
        JumpBufferCounter = 0;
    }
    public void ResetAimBufferCounter()
    {
        AimBufferCounter = 0;
    }
}
