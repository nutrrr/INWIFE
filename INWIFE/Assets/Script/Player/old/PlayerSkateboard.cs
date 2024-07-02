using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkateboard : MonoBehaviour
{
    public enum SkateboardState { Aiming, Air, Ground, Trick } // Define possible states
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

    private bool isAiming;
    private bool isReset;


    [Header("Move")]
    [SerializeField] private float maxSpeedNormal = 14f; // Maximum move speed
    [SerializeField] private float maxSpeedBoot = 16f; // Maximum with boot speed
    private float CurrentmaxSpeed;
    [SerializeField] private float acceleration = 5f; // Acceleration rate
    [Range(0.01f, 1f)] [SerializeField] private float accelerationInAir = 0.65f; // Acceleration rate in air
    [SerializeField] private float deceleration = 2f; // Deceleration rate
    [Range(0.01f, 1f)] [SerializeField] private float decelerationInAir = 0.65f; // Deceleration rate in air
    [SerializeField] private float accelerationMaxSpeed = 5f; // Acceleration rate
    [SerializeField] private float decelerationMaxSpeed = 5f; // Acceleration rate



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
    [SerializeField] private float shootBuffer;
    private float shootBufferCounter;
    [SerializeField] private float resetAimingBuffer;
    private float resetAimingBufferCounter;


    [Header("Trick")]
    [Range(0.01f, 1f)] [SerializeField] private float AimingTimeScale; // Rotation speed
    private float spinCheckPoint; // 
    private float lastSpinDirection; // 
    //[SerializeField] private float rotationSpeed = 100f; // Rotation speed
    [SerializeField] private float rotationTrickSpeed = 360; // Rotation speed
    [SerializeField] private float resetRotationTrickSpeedMult = 2; // Rotation speed
    [SerializeField] private float bootForce;


    [Header("Shoot")]
    public GameObject bulletPrefab;
    public LayerMask enemyLayer;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public int bulletMaxAmount = 4;
    public int bulletAmount;

    public int aimAssistRadius;
    public int aimAssistAngle;


    [Header("Input")]
    private float moveHorInput; //AD
    private float moveVerInput; //WS
    private bool jumpInput; //
    private bool jumpInputRelease;
    private bool AimingInput;
    private bool AimingInputRelease;
    private bool resetAimingInput;
    private bool trickInput;
    private bool trickInputRelease;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        isFacingRight = true;

        CurrentmaxSpeed = maxSpeedNormal;

        jumpGravity = -(2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
        jumpGravityScale = jumpGravity / Physics2D.gravity.y;
        jumpForce = Mathf.Abs(jumpGravity) * timeToJumpApex;
    }

    void Update()
    {
        moveHorInput = Input.GetAxisRaw("Horizontal"); // Get horizontal input (left/right)
        moveVerInput = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        jumpInputRelease = Input.GetKeyUp(KeyCode.Space);
        AimingInput = Input.GetKeyDown(KeyCode.J);
        AimingInputRelease = Input.GetKeyUp(KeyCode.J);
        resetAimingInput = Input.GetKeyDown(KeyCode.L);

        trickInput = Input.GetKeyDown(KeyCode.K);
        trickInputRelease = Input.GetKeyUp(KeyCode.K);

        if (jumpInput)
        {
            jumpBufferCounter = jumpBuffer;
        }

        if (AimingInput)
        {
            shootBufferCounter = shootBuffer;
        }

        if ((moveHorInput > 0 && rb.velocity.x < 0f) || (moveHorInput < 0 && rb.velocity.x > 0f))
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
        // Jump State and Gravity
        if (!isGrounded)
        {
            // Floating
            if (rb.velocity.y > 0 )
            {
                // JumpCut
                if ((jumpInputRelease || isJumpCut || !isJumping) && currentState != SkateboardState.Aiming)
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
            if (Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold && !isJumpCut && isJumping)
            {
                rb.gravityScale = jumpGravityScale * jumpHangGravityMult;
            }
        }


        if (isGrounded || (coyoteTimeCounter > 0 && !isOnSlope))
        {
            rb.gravityScale = defaultGravity;
            if (currentState == SkateboardState.Air)
            {
                Land();
            }
            else if (currentState == SkateboardState.Aiming) // Transition from trick to ground
            {
                EndTrick();
                Land();

            }
            currentState = SkateboardState.Ground;
        }

        //Aiming
        if (currentState == SkateboardState.Air && shootBufferCounter > 0)
        {
            currentState = SkateboardState.Aiming;
            shootBufferCounter = 0;
        }
        else if(currentState == SkateboardState.Aiming && AimingInputRelease)
        {
            currentState = SkateboardState.Air;
        }

        if(currentState == SkateboardState.Aiming)
        {

            //
            if (resetAimingInput)
            {
                isReset = true;
            }
            //Shot
            if (moveVerInput == 0)
            {
                isAiming = true;
            }
            if (isAiming && moveVerInput == -1)
            {
                isAiming = false;
                Shoot();
            }
        }

        jumpBufferCounter -= Time.deltaTime;
        coyoteTimeCounter -= Time.deltaTime;
        shootBufferCounter -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (resetAimingInput)
        {
            Debug.Log(Mathf.Abs((int)currentState));
        }
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
            case SkateboardState.Aiming:
                if (isAiming)
                {
                    Time.timeScale = AimingTimeScale;
                    Time.fixedDeltaTime = 0.02F * Time.timeScale;
                }
                else
                {
                    Time.timeScale = AimingTimeScale;
                    Time.fixedDeltaTime = 0.02F * Time.timeScale;
                }
                rb.freezeRotation = false;
                break;
        }
        //CalculateGravity();
        switch (currentState)
        {
            case SkateboardState.Aiming:
                Aiming();
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

        Vector2 targetSpeed = new Vector2(moveHorInput, 0);
        float accelRate;
        Vector2 direction = Vector2.right;

        // on Grounded
        if (coyoteTimeCounter > 0)
        {
            // on Slope Grounded
            if (isOnSlope)
            {
                direction = -slopeNormalPerp;
                //targetSpeed -= slopeNormalPerp.x;

                // not moving(make player slide down slope)
                if (targetSpeed.x == 0)
                {

                    // slope down to right
                    if (slopeNormalPerp.y > 0)
                    {
                        CurrentmaxSpeed = Mathf.Min(CurrentmaxSpeed + Mathf.Abs(slopeNormalPerp.y * accelerationMaxSpeed * Time.deltaTime), maxSpeedBoot);
                        targetSpeed.x = 1;
                        targetSpeed.y = -1f;

                    }
                    // slope down to left
                    else if (slopeNormalPerp.y < 0)
                    {
                        CurrentmaxSpeed = Mathf.Min(CurrentmaxSpeed + Mathf.Abs(slopeNormalPerp.y * accelerationMaxSpeed * Time.deltaTime), maxSpeedBoot);
                        targetSpeed.x = -1;
                        targetSpeed.y = -1f;
                    }
                }
                // moving
                else
                {
                    //go down to slope
                    if (moveHorInput * slopeNormalPerp.y > 0)
                    {
                        CurrentmaxSpeed = Mathf.Min(CurrentmaxSpeed + Mathf.Abs(slopeNormalPerp.y * accelerationMaxSpeed * Time.deltaTime), maxSpeedBoot);
                        targetSpeed.y = -8f;
                    }
                    // go up to slope
                    else if (moveHorInput * slopeNormalPerp.y < 0)
                    {
                        CurrentmaxSpeed = Mathf.Max(CurrentmaxSpeed - Mathf.Abs(slopeNormalPerp.y * decelerationMaxSpeed * Time.deltaTime), maxSpeedNormal);
                        targetSpeed.y = 1f;
                    }
                }
            }
            accelRate = (Mathf.Abs(targetSpeed.x) > 0.01f) ? acceleration : deceleration;
        }
        // on Air
        else
        {
            accelRate = (Mathf.Abs(targetSpeed.x) > 0.01f) ? acceleration * accelerationInAir : deceleration * decelerationInAir;
            // increase acceleration and speed While at the top of the jump
            if (Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold)
            {
                accelRate *= jumpHangAccelerationMult;
                targetSpeed *= jumpHangMaxSpeedMult;    
            }
        }

        // apply user input values ​​or environment direction in horizontal to targetSpeed
        targetSpeed = new Vector2(targetSpeed.x * CurrentmaxSpeed, targetSpeed.y * CurrentmaxSpeed * Mathf.Abs(direction.y));
        //Calculate difference between current velocity and desired velocity
        Vector2 speedDif = targetSpeed - rb.velocity;

        //Calculate force along x-axis to apply to thr player
        Vector2 movement = speedDif * accelRate;
        Debug.Log("Old\ntargetSpeed = " + targetSpeed.x + "\n" + "speedDif = " + speedDif.x + "\n" + "movement = " + movement.x);
        //Convert this to a vector and apply to rigidbody
        Debug.DrawLine(rb.position, rb.position + new Vector2(movement.x * direction.x, movement.y * Mathf.Abs(direction.y)));
        rb.AddForce(new Vector2(movement.x * direction.x, movement.y * Mathf.Abs(direction.y)), ForceMode2D.Force);
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
        /*if (rb.velocity.x != 0)
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
        */

        rb.rotation = 0f;
    }

    void Aiming() 
    {
        if (lastSpinDirection == 0)
        {
            lastSpinDirection = moveHorInput;

        }
        if (isReset)
        {
            // Reset rotation to 0
            if (Mathf.Abs(transform.eulerAngles.z) < resetRotationTrickSpeedMult * 15)
            {
                isReset = false;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
            }
            else
            {
                transform.Rotate(0f, 0f, -Mathf.Sign(transform.rotation.z) * resetRotationTrickSpeedMult * rotationTrickSpeed * Time.deltaTime);
            }
            
        }
        else
        {
            if(moveHorInput * lastSpinDirection > 0)
            {
                spinCheckPoint += moveHorInput * lastSpinDirection * rotationTrickSpeed * Time.deltaTime;
            }
            transform.Rotate(0f, 0f, moveHorInput * rotationTrickSpeed * Time.deltaTime);

        }
    }

    private void ResetTrick()
    {
        bulletAmount = Mathf.Min(bulletAmount + (int)(spinCheckPoint / 90), bulletMaxAmount);

        isReset = false;

        lastSpinDirection = 0;
        spinCheckPoint = 0;
    }

    void EndTrick()
    {
        
    }

    void Shoot()
    {
        if (bulletAmount <= 0) { return; }
        bulletAmount--;
        // Instantiate a new bullet at the fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Get the Rigidbody2D component of the bullet
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // Apply force to the bullet in the calculated direction;
        Vector2 direction = new Vector2(Mathf.Cos((this.rb.rotation - 90) * Mathf.Deg2Rad), Mathf.Sin((this.rb.rotation - 90) * Mathf.Deg2Rad));


        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, aimAssistRadius, enemyLayer);
        float inAngle = transform.eulerAngles.z - 90;

        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, aimAssistRadius);
        Debug.DrawLine(this.rb.position, this.rb.position + new Vector2(Mathf.Cos((inAngle + aimAssistAngle / 2) * Mathf.Deg2Rad), Mathf.Sin((inAngle + aimAssistAngle / 2) * Mathf.Deg2Rad)) * aimAssistRadius, Color.white, 1f);
        Debug.DrawLine(this.rb.position, this.rb.position + new Vector2(Mathf.Cos((inAngle - aimAssistAngle / 2) * Mathf.Deg2Rad), Mathf.Sin((inAngle - aimAssistAngle / 2) * Mathf.Deg2Rad)) * aimAssistRadius, Color.white, 1f);



        if (colliders != null)
        {
            float shotTarget = Mathf.Infinity;
            foreach (Collider2D i in colliders)
            {
                float targetRad = Mathf.Atan2(i.transform.position.y - transform.position.y, i.transform.position.x - transform.position.x);
                float targetAngle = (180 / Mathf.PI) * targetRad;




                if (targetAngle > inAngle - aimAssistAngle/2 && targetAngle < inAngle + aimAssistAngle/2)
                {
                    float AngleDef = Mathf.Abs(targetAngle - inAngle);
                    if (AngleDef < shotTarget)
                    {
                        Debug.DrawLine(transform.position, i.transform.position, Color.gray, 1f);
                        shotTarget = AngleDef;
                        direction = (i.transform.position - transform.position).normalized;

                    }
                }
            }

        }

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

            ResetTrick();
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
    }

    Vector2 CollideSlide(Vector2 origin, Vector2 direction, float length)
    {
        if(length <= 0) { return Vector2.zero; }
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, length, layerGround);
        Vector2 slopeNormalPerp;
        
        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            Debug.DrawRay(origin, hit.point, Color.red);
            if (slopeNormalPerp.x * slopeNormalPerp.y != 0)
            {
                return (hit.point - origin) + CollideSlide(hit.point, slopeNormalPerp, length - slopeNormalPerp.magnitude);
            }
            else
            {
                return Vector2.zero;
            }
        }
        else
        {
            Debug.DrawRay(origin, (direction - origin) * length, Color.red);
            return (direction - origin) * length;
        }
    }
}