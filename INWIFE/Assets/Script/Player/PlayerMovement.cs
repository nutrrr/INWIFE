using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask layerGround;

    private Rigidbody2D rb;
    private PlayerMoveData Data;
    private PlayerController control;

    public bool IsJumping { get; private set; }
    public bool IsFalling { get; private set; }
    public bool IsFacingRight { get; private set; }
    public bool IsGrounded { get; private set; }
    [SerializeField] private bool isJumpCut;
    [SerializeField] private float inputBufferCounter;
    [SerializeField] private float coyoteTimeCounter;

    private BoxCollider2D collider2d;
    private Vector2 movementInput;
    private bool jumpInputRelease;

    public void Init(Rigidbody2D rb, PlayerMoveData data, BoxCollider2D collider2d, PlayerController control)
    {
        this.rb = rb;
        this.Data = data;
        this.collider2d = collider2d;
        this.control = control;
    }

    private void Start()
    {
        IsFacingRight = true;
        SetGravityScale(Data.gravityScale);
    }

    private void Update()
    {
        #region JUMP CHECKS
        if (IsJumping && rb.velocity.y < 0)
        {
            IsJumping = false;
            IsFalling = true;

        }
        #endregion
    }

    public void UpdatePlayerMove()
    {
        if (!IsJumping)
        {
            // Update grounded status
            CheckGrounded();
        }

        // Chack is Ground
        IsGrounded = coyoteTimeCounter > 0 && !IsJumping;


        #region Handle Jump
        // On Ground and not jump
        if (coyoteTimeCounter > 0 && !IsJumping)
        {
            isJumpCut = false;
            if (!IsJumping)
            {
                IsFalling = false;
            }
        }

        //Jump
        if (control.JumpBufferCounter > 0f && (IsGrounded || coyoteTimeCounter > 0f))
        {
            Debug.Log("jump");
            //reset coyote time
            coyoteTimeCounter = 0f;

            IsJumping = true;
            IsFalling = false;
            Jump();
        }

        // Release Jump
        if ((jumpInputRelease && IsJumping && rb.velocity.y > 0))
        {
            JumpRelease();
        }

        // Coyote time countdown
        coyoteTimeCounter -= Time.deltaTime;
        #endregion

        if (movementInput.x != 0)
        {
            CheckDirectionToFace(movementInput.x > 0);
        }

        #region GRAVITY
        if (rb.velocity.y < 0)
        {

            //Much higher gravity if holding down
            SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFastFallSpeed));
        }
        else if (isJumpCut)
        {
            //Higher gravity if jump button released
            SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFallSpeed));
        }
        else if ((IsJumping || IsFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
        {
            //Decrease Gravity when at the apex of their jump
            SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
        }
        else if (rb.velocity.y < 0)
        {
            //Higher gravity if falling
            SetGravityScale(Data.gravityScale * Data.fallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFallSpeed));
        }
        else
        {
            //Default gravity if standing on a platform or moving upwards
            SetGravityScale(Data.gravityScale);
        }
        #endregion

    }

    public void FixedUpdatePlayerMove()
    {
        // Handle movement
        HandleMovement(1);
    }

    void HandleJumpInput()
    {


        // On Ground and not jump
        if (coyoteTimeCounter > 0 && !IsJumping)
        {
            isJumpCut = false;
            if (!IsJumping)
            {
                IsFalling = false;
            }
        }

        //Jump
        if (control.JumpBufferCounter > 0f && (IsGrounded || coyoteTimeCounter > 0f))
        {
            Debug.Log("jump");
            //reset coyote time
            coyoteTimeCounter = 0f;
            IsJumping = true;
            IsFalling = false;
            Jump();
        }

        if ((jumpInputRelease && IsJumping && rb.velocity.y > 0))
        {
            JumpRelease();
        }


        // Coyote time countdown
        coyoteTimeCounter -= Time.deltaTime;
    }
    private void HandleMovement(float lerpAmount)
    {
        //Calculate the direction we want to move in and our desired velocity
        float targetSpeed = movementInput.x * Data.runMaxSpeed;
        //We can reduce are control using Lerp() this smooths changes to are direction and speed
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

        #region Calculate AccelRate
        float accelRate;

        //Gets an acceleration value based on if we are accelerating (includes turning) 
        //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
        if (coyoteTimeCounter > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
        #endregion

        #region Add Bonus Jump Apex Acceleration
        //Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
        if ((IsJumping || IsFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetSpeed *= Data.jumpHangMaxSpeedMult;
        }
        #endregion

        #region Conserve Momentum
        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        if (Data.doConserveMomentum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && coyoteTimeCounter < 0)
        {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
            accelRate = 0;
        }
        #endregion

        //Calculate difference between current velocity and desired velocity
        float speedDif = targetSpeed - rb.velocity.x;
        //Calculate force along x-axis to apply to thr player

        float movement = speedDif * accelRate;

        //Convert this to a vector and apply to rigidbody
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        /*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
    }

    private void Turn()
    {
        //stores scale and flips the player along the x axis, 
        //Vector3 scale = transform.localScale;
        //scale.x *= -1;
        //transform.localScale = scale;
        transform.Rotate(0f, 180f, 0f);

        IsFacingRight = !IsFacingRight;
    }

    void Jump()
    {
        // Reset inputBufferCounter to prevent multiple jumps
        control.JumpBufferCounter = 0;
        // Perform the jump
        #region Perform Jump
        //We increase the force applied if we are falling
        //This means we'll always feel like we jump the same amount 
        //(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
        float force = Data.jumpForce;
        if (rb.velocity.y < 0)
            force -= rb.velocity.y;

        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion    }
    }

    void JumpRelease()
    {
        if (IsJumping && rb.velocity.y > 0)
        {
            isJumpCut = true;
        }
    }

    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }

    #region CHECK METHODS
    void CheckGrounded()
    {
        // Set origin point below player for Raycast
        Vector2 originRight = new Vector2(rb.position.x + (collider2d.size.x / 2.125f), rb.position.y - (collider2d.size.y / 2f));
        Vector2 originLeft = new Vector2(rb.position.x - (collider2d.size.x / 2.125f), rb.position.y - (collider2d.size.y / 2f));

        // Draw Ray from origin point to check ground
        RaycastHit2D hitR = Physics2D.Raycast(originRight, Vector2.down, 0.025f, layerGround);
        RaycastHit2D hitL = Physics2D.Raycast(originLeft, Vector2.down, 0.025f, layerGround);

        // Ray hit the ground
        if (hitR.collider != null || hitL.collider != null)
        {
            coyoteTimeCounter = Data.coyoteTime;
            return;
        }

        // Ray not hit the ground
        return;

    }

    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }
    #endregion

    public void InitTheInput(Vector2 input, float jumpInput, bool jumpInputRelease)
    {
        this.movementInput = input;
        this.jumpInputRelease = jumpInputRelease;
    }

    public bool getIsisGrounded()
    {
        return IsGrounded;
    }

}