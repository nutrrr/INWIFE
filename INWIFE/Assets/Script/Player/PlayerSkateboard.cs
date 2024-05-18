using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkateboard : MonoBehaviour
{
    public enum SkateboardState { Air, Ground, Trick } // Define possible states
    [SerializeField] private LayerMask layerGround;

    private Rigidbody2D rb;
    private SkateboardState currentState = SkateboardState.Ground; // Initial state
    [Header("Move")]
    [SerializeField] private float maxSpeed = 10f; // Maximum speed
    private float maxSpeedAir = 5f; // Reduced max speed in air (optional)
    [SerializeField] private float acceleration = 5f; // Acceleration rate
    [SerializeField] private float deceleration = 2f; // Deceleration rate
    [SerializeField] private float jumpHeight = 2f; // Maximum jump height
    [SerializeField] private float jumpTimeToPeak = 0.5f; // Time to reach the peak of the jump
    [SerializeField] private float fallGravityMult = 0.5f; //Multiplier to the player's gravityScale when falling.
    [SerializeField] private float maxFallSpeed = 20f; //Maximum fall speed when falling.
    [SerializeField] private bool groundContact; // Flag for ground contact

    [Range(0.01f, 1f)] [SerializeField] private float TrickTimeScale; // Rotation speed
    [SerializeField] private float rotationSpeed = 100f; // Rotation speed
    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;


    private float moveInput;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal"); // Get horizontal input (left/right)
        bool shootInput = Input.GetKeyDown(KeyCode.Space);
        CheckGrounded();

        // Handle state transitions based on input and ground contact
        if (Input.GetKeyDown(KeyCode.Space) && currentState == SkateboardState.Ground) // Jump
        {
            Jump();
            currentState = SkateboardState.Air;
        }

        if (groundContact)
        {
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
        else if (currentState == SkateboardState.Ground) // Transition from ground to air
        {
            currentState = SkateboardState.Air;
        }

        //Trick
        if (currentState == SkateboardState.Air && moveInput != 0f)
        {
            currentState = SkateboardState.Trick;
        }
        else if(currentState == SkateboardState.Trick && moveInput == 0f)
        {
            currentState = SkateboardState.Air;
        }

        //Shot
        if (!groundContact && shootInput)
        {
            Shoot();
        }
    }

    void FixedUpdate()
    {
        // Apply physics based on current state
        switch (currentState)
        {
            case SkateboardState.Air:
                ApplyAirPhysics();
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                rb.freezeRotation = true;
                break;
            case SkateboardState.Ground:
                ApplyGroundPhysics();
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                rb.freezeRotation = true;
                break;
            case SkateboardState.Trick:
                ApplyTrickPhysics();
                Time.timeScale = TrickTimeScale;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                rb.freezeRotation = false;
                break;
        }

        Move(1);
    }

    void Move(float lerpAmount)
    {
        // Handle movement based on current state
        switch (currentState)
        {
            case SkateboardState.Air:
                // No horizontal movement in air (comment out the line)
                // rb.velocity = new Vector2(moveInput * maxSpeedAir, rb.velocity.y);
                break;
            case SkateboardState.Ground:
                float targetSpeed = moveInput * maxSpeed;
                float speedDifference = targetSpeed - rb.velocity.x;
                float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
                float movement = speedDifference * accelRate;

                rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
                break;
            case SkateboardState.Trick:
                // Handle trick-specific movement here (if applicable)
                transform.Rotate(0f, 0f, moveInput * rotationSpeed * Time.deltaTime);
                break;
        }
    }

    void Jump()
    {
        // Calculate initial jump velocity to reach desired height and time to peak
        float initialYVelocity = Mathf.Abs(2 * jumpHeight * Physics.gravity.y / jumpTimeToPeak);

        // Apply upward force for jump
        rb.AddForce(Vector2.up * initialYVelocity);
    }

    void Land()
    {
        // Apply landing effects (e.g., sound, visual effects)
        rb.rotation = 0f;

    }

    void ApplyAirPhysics()
    {
        // Apply gravity and air resistance in air
        if(rb.velocity.y < 0)
        {
            rb.gravityScale = Mathf.Abs(1 * fallGravityMult);
        }
        else
        {
            rb.gravityScale = 2f;
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, maxFallSpeed));
        // Handle air drag (optional)
        // rb.AddForce(new Vector2(-rb.velocity.x * airDrag * Time.deltaTime, 0f));
    }

    void ApplyGroundPhysics()
    {
        // Apply friction on ground
        float currentSpeed = rb.velocity.x;
        float frictionForce = Mathf.Sign(currentSpeed) * deceleration * Time.deltaTime;

        if (Mathf.Abs(currentSpeed) < Mathf.Abs(frictionForce))
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else
        {
            rb.AddForce(new Vector2(-frictionForce, 0f));
        }
    }

    void ApplyTrickPhysics()
    {
        // Apply gravity and air resistance in air
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = Mathf.Abs(1 * fallGravityMult);
        }
        else
        {
            rb.gravityScale = 2f;
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, maxFallSpeed));

    }
    void StartTrick() // Trigger trick when pressing movement input in air
    {
        if (currentState == SkateboardState.Air && Input.GetAxis("Horizontal") != 0f)
        {
            // Prepare for trick execution (e.g., animation, sound effects)
            currentState = SkateboardState.Trick;
        }
        else
        {
            currentState = SkateboardState.Air;
        }
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
        CircleCollider2D collider2d = GetComponent<CircleCollider2D>();
        // Set origin point below player for Raycast
        Vector2 originRight = new Vector2(rb.position.x + (collider2d.radius / 1f), rb.position.y - (collider2d.radius / 1f));
        Vector2 originLeft = new Vector2(rb.position.x - (collider2d.radius / 1f), rb.position.y - (collider2d.radius / 1f));

        // Draw Ray from origin point to check ground
        RaycastHit2D hitR = Physics2D.Raycast(originRight, Vector2.down, 0.5f, layerGround);
        RaycastHit2D hitL = Physics2D.Raycast(originLeft, Vector2.down, 0.5f, layerGround);

        // Ray hit the ground
        if (hitR.collider != null || hitL.collider != null)
        {
            groundContact = true;
            return;
        }

        // Ray not hit the ground
        groundContact = false;
        return;
    }
}