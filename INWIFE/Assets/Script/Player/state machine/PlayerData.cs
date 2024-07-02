using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] //Create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
public class PlayerData : ScriptableObject
{
    public LayerMask layerGround;

    //Component
    public Rigidbody2D rb;


    [Header("Move")]
    public float maxSpeedNormal = 14f; // Maximum move speed
    public float maxSpeedBoot = 16f; // Maximum with boot speed
    public float acceleration = 5f; // Acceleration rate
    [Range(0.01f, 1f)] public float accelerationInAir = 0.65f; // Acceleration rate in air
    public float deceleration = 2f; // Deceleration rate
    [Range(0.01f, 1f)] [SerializeField] private float decelerationInAir = 0.65f; // Deceleration rate in air
    public float accelerationMaxSpeed = 5f; // Acceleration rate
    public float decelerationMaxSpeed = 5f; // Acceleration rate



    [Header("Jump")]
    public float jumpHeight = 4f; // Maximum jump height
    public float maxFallSpeed = 20f; //Maximum fall speed when falling.
    public float jumpForce; // Maximum jump height
    public float timeToJumpApex = 0.5f; // Time to reach the peak of the jump
    public float defaultGravity = 1f; //default gravity to the player's gravityScale when not jumping or falling or else.
    public float jumpGravity;
    public float jumpGravityScale;
    public float fallingMultiplier = 1f;
    public float jumpCutOffMultiplier = 3f;


    [Header("JumpHang")]
    [Range(0f, 1)] public float jumpHangGravityMult; //Reduces gravity while close to the apex (desired max height) of the jump
    public float jumpHangTimeThreshold; //Speeds (close to 0) where the player will experience extra "jump hang". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)
    [Space(1.5f)]
    public float jumpHangAccelerationMult;
    public float jumpHangMaxSpeedMult;

    [Header("Assist")]
    public float coyoteTime;
    public float jumpBuffer;
    public float shootBuffer;
    public float resetAimingBuffer;


    [Header("Aim")]
    [Range(0.01f, 1f)] public float AimingTimeScale; // Rotation speed
    //[SerializeField] private float rotationSpeed = 100f; // Rotation speed
    public float RotationAimSpeed = 360; // Rotation speed
    public float ResetRotationAimSpeedMult = 2; // Rotation speed


    [Header("Shoot")]
    public GameObject bulletPrefab;
    public LayerMask enemyLayer;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public int bulletMaxAmount = 4;
    public int bulletAmount;

    public int aimAssistRadius;
    public int aimAssistAngle;

    private void OnValidate()
    {
        jumpGravity = -(2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
        jumpGravityScale = jumpGravity / Physics2D.gravity.y;
        jumpForce = Mathf.Abs(jumpGravity) * timeToJumpApex;
    }
}
