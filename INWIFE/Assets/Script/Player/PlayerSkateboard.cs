using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkateboard : MonoBehaviour
{
    public enum SkateboardState { Air, Ground, Trick } // Define possible states
    [SerializeField] private LayerMask layerGround;

    //Component
    private Rigidbody2D rb;

    [Header("Slope")]
    [SerializeField] private float maxSlopeAngle = 60f;
    [SerializeField] private float slopeCheckDistance = 0.05f;
    private Vector2 slopeNormalPerp;
    private float slopeDownAngle;
    private float slopeSideAngle;
    private float lastSlopeAngle;



    //State
    private SkateboardState currentState = SkateboardState.Ground; // Initial state
    private bool isFacingRight;
    private bool isGrounded;
    private bool isOnSlope;
    private bool canWalkOnSlope;

    private bool isJumping;
    private bool isFloating;
    private bool isFalling;
    private bool isJumpCut;


    [Header("Move")]
    [SerializeField] private float maxSpeedNormal = 10f; // Maximum move speed
    [SerializeField] private float maxSpeedBoot = 13f; // Maximum with boot speed
    [SerializeField] private float acceleration = 5f; // Acceleration rate
    [Range(0.01f, 1f)] [SerializeField] private float accelerationInAir = 0.65f; // Acceleration rate in air
    [SerializeField] private float deceleration = 2f; // Deceleration rate
    [Range(0.01f, 1f)] [SerializeField] private float decelerationInAir = 0.65f; // Deceleration rate in air
    

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 4f; // Maximum jump height
    [SerializeField] private float maxFallSpeed = 20f; //Maximum fall speed when falling.
    private float jumpForce; // Maximum jump height
    [SerializeField] private float timeToJumpApex = 0.5f; // Time to reach the peak of the jump
    [SerializeField] private float defaultGravity = 1f; //default gravity to the player's gravityScale when not jumping or falling or else.
    private float jumpGravity;
    private float jumpGravityScale;
    [SerializeField] private float fallingMultiplier = 1f;
    [SerializeField] private float jumpCutOffMultiplier = 3f;


    [Header("JumpHang")]
    [Range(0f, 1)] public float jumpHangGravityMult; //Reduces gravity while close to the apex (desired max height) of the jump
    public float jumpHangTimeThreshold; //Speeds (close to 0) where the player will experience extra "jump hang". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)
    [Space(1.5f)]
    public float jumpHangAccelerationMult;
    public float jumpHangMaxSpeedMult;

    [Header("Assist")]
    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBuffer;
    private float jumpBufferCounter;


    [Header("Trick")]
    [Range(0.01f, 1f)] [SerializeField] private float TrickTimeScale; // Rotation speed
    [SerializeField] private float rotationSpeed = 100f; // Rotation speed
    [SerializeField] private float rotationTrickSpeed = 100f; // Rotation speed
    [SerializeField] private float bootForce;


    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;


    [Header("Input")]
    private float moveInput;
    private bool jumpInput;
    private bool jumpInputRelease;
    private bool shootInput;
    private bool shootInputRelease;
    private bool trickInput;
    private bool trickInputRelease;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isFacingRight = true;
        jumpGravity = -(2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
        jumpGravityScale = jumpGravity / Physics2D.gravity.y;
        jumpForce = Mathf.Abs(jumpGravity) * timeToJumpApex;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal"); // Get horizontal input (left/right)
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        jumpInputRelease = Input.GetKeyUp(KeyCode.Space);
        shootInput = Input.GetKeyDown(KeyCode.Mouse0);
        shootInputRelease = Input.GetKeyUp(KeyCode.Mouse0);
        trickInput = Input.GetKeyDown(KeyCode.Mouse1);
        trickInputRelease = Input.GetKeyUp(KeyCode.Mouse1);

        if (jumpInput)
        {
            jumpBufferCounter = jumpBuffer;
        }

        if ((moveInput > 0 && rb.velocity.x < 0f) || (moveInput < 0 && rb.velocity.x > 0f))
        {
            isFacingRight = !isFacingRight;
        }

        if (!isFloating)
        {
            CheckGrounded();
            SlopeCheck();
        }


        // Gravity
        // Handle state transitions based on input and ground contact
        // Jump
        if (jumpBufferCounter > 0 && (isGrounded || coyoteTimeCounter > 0)) 
        {
            coyoteTimeCounter = 0;
            rb.gravityScale = jumpGravityScale;
            isJumping = true;
            isFloating = true;
            isGrounded = false;
            Jump();
            currentState = SkateboardState.Air;
        }
        if (currentState == SkateboardState.Air)
        {
            // Floating
            if (rb.velocity.y > 0)
            {
                // JumpCut
                if (jumpInputRelease)
                {
                    isJumpCut = true;
                    rb.gravityScale = jumpGravityScale * jumpCutOffMultiplier;
                }
            }
            // Falling
            else
            {
                isFloating = false;
                isFalling = true;
                rb.gravityScale = jumpGravityScale * fallingMultiplier;
            }
            // Jump as Apex
            if (Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold && !isJumpCut)
            {
                rb.gravityScale = jumpGravityScale * jumpHangGravityMult;
            }
        }


        if (isGrounded)
        {
            rb.gravityScale = defaultGravity;
            if (currentState == SkateboardState.Air)
            {
                Land();
            }
            else if (currentState == SkateboardState.Trick) // Transition from trick to ground
            {
                EndTrick();
                Land();

            }
            currentState = SkateboardState.Ground;
        }
        else{
            rb.gravityScale = jumpGravityScale;
        }

        //Shot
        if (currentState == SkateboardState.Trick && shootInputRelease)
        {
            Shoot();
        }

        //Trick
        if (currentState == SkateboardState.Air && trickInput)
        {
            currentState = SkateboardState.Trick;
        }
        else if(currentState == SkateboardState.Trick && trickInputRelease)
        {
            currentState = SkateboardState.Air;
        }


        jumpBufferCounter -= Time.deltaTime;
        coyoteTimeCounter -= Time.deltaTime;
    }

    void FixedUpdate()
    {

        // Apply physics based on current state
        switch (currentState)
        {
            case SkateboardState.Air:
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                rb.freezeRotation = true;
                break;
            case SkateboardState.Ground:
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                rb.freezeRotation = true;
                break;
            case SkateboardState.Trick:
                //Time.timeScale = TrickTimeScale;
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                rb.freezeRotation = false;
                break;
        }
        //CalculateGravity();
        switch (currentState)
        {
            case SkateboardState.Trick:
                DoSomethingTrick();
                break;
            default:
                Move();
                break;
        }
    }

    // handle movement when Skating(normal movement), Sloping(movement on slope like go up down on slope and slide down when not not moving), Grinding()
    void Move()
    {

        // Handle movement based on current state
        float targetSpeed = moveInput * maxSpeedNormal;
        float accelRate;
        Vector2 direction = Vector2.right;

        // on Grounded
        if (coyoteTimeCounter > 0)
        {
            if (isOnSlope)
            {
                direction = -slopeNormalPerp;
                if (moveInput == 0)
                {
                    // slope down to right
                    if (slopeNormalPerp.y > 0)
                    {
                        targetSpeed =  maxSpeedBoot ;
                    }
                    // slope down to left
                    else if (slopeNormalPerp.y < 0)
                    {
                        targetSpeed = -maxSpeedBoot;
                    }
                }
            }
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        }
        // on Air
        else
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration * accelerationInAir : deceleration * decelerationInAir;
            if (Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold)
            {
                accelRate *= jumpHangAccelerationMult;
                targetSpeed *= jumpHangMaxSpeedMult;
            }
        }

        //Calculate difference between current velocity and desired velocity
        float speedDif = targetSpeed - rb.velocity.x;

        //Calculate force along x-axis to apply to thr player
        float movement = speedDif * accelRate;

        //Convert this to a vector and apply to rigidbody
        Debug.Log("targetSpeed = " + targetSpeed);
        Debug.Log("speedDif = " + speedDif);
        rb.AddForce(movement * direction, ForceMode2D.Force);


    }

    void Jump()
    {
        jumpBufferCounter = 0;

        float force = jumpForce;
        if (rb.velocity.y < 0)
            force -= rb.velocity.y;
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

    }

    void Land()
    {
        if (rb.velocity.x != 0)
        {
            // When hitting the ground at the right angle, will gain additional speed.
            if (Mathf.Abs(rb.rotation) < 60f)
            {
                Vector2 direction = rb.velocity.x > 0 ? Vector2.right : Vector2.left;

                rb.AddForce(bootForce * Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale * direction, ForceMode2D.Force);
                //rb.velocity = new Vector2(maxSpeed * direction, rb.velocity.y);
                Debug.Log(direction);
            }
        }
        rb.rotation = 0f;

    }

    void DoSomethingTrick()
    {
        transform.Rotate(0f, 0f, moveInput * rotationTrickSpeed);
    }
    void EndTrick()
    {
        
    }

    void Shoot()
    {
        // Instantiate a new bullet at the fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Get the Rigidbody2D component of the bullet
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // Apply force to the bullet in the calculated direction;
        Vector2 direction = new Vector2(Mathf.Cos((this.rb.rotation - 90) * Mathf.Deg2Rad), Mathf.Sin((this.rb.rotation - 90) * Mathf.Deg2Rad));
        rb.AddForce((direction) * bulletSpeed, ForceMode2D.Impulse);
    }

    void CheckGrounded()
    {
        CapsuleCollider2D collider2d = GetComponent<CapsuleCollider2D>();
        // Set origin point below player for Raycast
        Vector2 originRight = new Vector2(rb.position.x + (collider2d.size.x / 2f), rb.position.y - (collider2d.size.y / 2f));
        Vector2 originLeft = new Vector2(rb.position.x - (collider2d.size.x / 2f), rb.position.y - (collider2d.size.y / 2f));
        // Draw Ray from origin point to check ground
        RaycastHit2D hitR = Physics2D.Raycast(originRight, Vector2.down, 0.05f, layerGround);
        RaycastHit2D hitL = Physics2D.Raycast(originLeft, Vector2.down, 0.05f, layerGround);

        // Ray hit the ground
        if (hitR.collider != null || hitL.collider != null)
        {
            coyoteTimeCounter = coyoteTime;
            isJumping = false;
            isFloating = false;
            isFalling = false;
            isJumpCut = false;
            isGrounded = true;
            return;
        }

        // Ray not hit the ground
        isGrounded = false;
        return;
    }
    private void SlopeCheck()
    {
        CapsuleCollider2D collider2d = GetComponent<CapsuleCollider2D>();
        Vector2 origin = new Vector2(rb.position.x, rb.position.y - (collider2d.size.y / 2f));

        SlopeCheckHorizontal(origin);
        SlopeCheckVertical(origin);
    }
    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, layerGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, layerGround);

        if (slopeHitFront)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }

    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, layerGround);

        if (hit)
        {

            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }

            lastSlopeAngle = slopeDownAngle;

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.green);

        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }

        if (isOnSlope && canWalkOnSlope && moveInput == 0.0f)
        {
            //rb.sharedMaterial = fullFriction;
        }
        else
        {
            //rb.sharedMaterial = noFriction;
        }
    }
}