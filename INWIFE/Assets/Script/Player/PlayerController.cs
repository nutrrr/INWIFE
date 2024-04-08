using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    static PlayerController instance;
    [SerializeField] private PlayerMoveData Data;

    [SerializeField] private PlayerInput playerInput;
    public PlayerMovement playerMovement;
    public Pow_Glider glider;

    [Header("Check")]
    [SerializeField] private LayerMask layerGround;
    [SerializeField] bool IsFacingRight;
    [SerializeField] private bool IsGrounded;

    [SerializeField] public float JumpBufferCounter;
    [SerializeField] public float AttackBufferCounter;
    [SerializeField] public float SwapBufferCounter;

    private Rigidbody2D rb;
    private BoxCollider2D collider2d;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }



        rb = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<BoxCollider2D>();

        playerMovement.Init(rb, Data, collider2d, this);
        glider.Init(rb, Data);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        #region Chack Ground
        // Set origin point below player for Raycast
        Vector2 originRight = new Vector2(rb.position.x + (collider2d.size.x / 2.125f), rb.position.y - (collider2d.size.y / 2f));
        Vector2 originLeft = new Vector2(rb.position.x - (collider2d.size.x / 2.125f), rb.position.y - (collider2d.size.y / 2f));

        // Draw Ray from origin point to check ground
        RaycastHit2D hitR = Physics2D.Raycast(originRight, Vector2.down, 0.05f, layerGround);
        RaycastHit2D hitL = Physics2D.Raycast(originLeft, Vector2.down, 0.05f, layerGround);

        // Ray hit the ground
        if (hitR.collider != null || hitL.collider != null)
        {
            IsGrounded = true;
            glider.Refule();
        }
        else
        {
            // Ray not hit the ground
            IsGrounded = false;
        }
        #endregion

        #region InputBuffer
        JumpBufferCounter -= Time.deltaTime;
        AttackBufferCounter -= Time.deltaTime;
        SwapBufferCounter -= Time.deltaTime;

        if (playerInput.GetJumpInput(0))
        {
            // Reset Jump Buffer time
            JumpBufferCounter = Data.jumpInputBufferTime;
        }
        if (playerInput.GetAttackInput())
        {
            // Reset Jump Buffer time
            AttackBufferCounter = Data.jumpInputBufferTime;
        }
        if (playerInput.GetSwapInput())
        {
            // Reset Jump Buffer time
            SwapBufferCounter = Data.jumpInputBufferTime;
        }
        #endregion

        IsFacingRight = playerMovement.IsFacingRight;
        HandleInput();

        //Movement
        if ((playerInput.GetGlideInput(0) && !IsGrounded && glider.CanGilde()))
        {

        }
        else
        {
            HandleMovement();
        }

    }


    private void FixedUpdate()
    {
        //Movement
        if ((playerInput.GetGlideInput(0) && !IsGrounded && glider.CanGilde()))
        {
            HandleGlider();
        }
        else
        {
            FixedHandleMovement();
        }
    }
    void HandleInput()
    {
        // Forward input to the PlayerInput script
        playerInput.HandleInput();
    }

    void HandleMovement()
    {
        // Forward input and handle movement in the PlayerMovement script
        playerMovement.InitTheInput(playerInput.GetHorizontalInput(), JumpBufferCounter, playerInput.GetJumpInput(1));
        playerMovement.UpdatePlayerMove();
    }
    void FixedHandleMovement()
    {
        // Forward input and handle movement in the PlayerMovement script
        playerMovement.FixedUpdatePlayerMove();
    }
    void HandleGlider()
    {
        glider.HandleGlider();
        glider.InitTheInput(playerInput.GetHorizontalInput(), playerInput.GetGlideInput(0), playerInput.GetGlideInput(1), IsFacingRight);
    }
}