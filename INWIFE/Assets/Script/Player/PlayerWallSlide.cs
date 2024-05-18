using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlide : MonoBehaviour
{
    public float horizontalInput;// out put -1, 0, 1

    private float wallSlidingSpeed = 2f;
    public bool isWallSliding;

    public float jumpForce = 50f;

    [SerializeField] private LayerMask layerGround;
    private Rigidbody2D rb;
    private BoxCollider2D collider2d;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        WallSlide();
    }

    public void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontalInput != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));

            //Wall Jump
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("WallJump");
                rb.velocity = new Vector2();
                WallJump();
            }
        }
        else
        {
            isWallSliding = false;
        }
    }
    
    private void WallJump()
    {

        // Perform the jump
        #region Perform Jump
        //We increase the force applied if we are falling
        //This means we'll always feel like we jump the same amount 
        //(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
        float force = jumpForce;
        if (rb.velocity.y < 0)
            force -= rb.velocity.y;
        Vector2 jumpDirection = new Vector2(-0.5f * horizontalInput, 0.5f);
        rb.AddForce(jumpDirection * force, ForceMode2D.Impulse);
        #endregion
        
    }

    private bool IsWalled()
    {
        // Set origin point below player for Raycast
        Vector2 originTop = new Vector2(rb.position.x + ((collider2d.size.x / 2f) * horizontalInput), rb.position.y - (collider2d.size.y / 2f));
        Vector2 originDown = new Vector2(rb.position.x + ((collider2d.size.x / 2f) * horizontalInput), rb.position.y - (collider2d.size.y / 2f));

        // Draw Ray from origin point to check ground
        RaycastHit2D hitR = Physics2D.Raycast(originTop, Vector2.right * horizontalInput, 0.05f, layerGround);
        RaycastHit2D hitL = Physics2D.Raycast(originDown, Vector2.right * horizontalInput, 0.05f, layerGround);

        // Ray hit the ground
        return hitR || hitL;
    }

    private bool IsGrounded()
    {
        // Set origin point below player for Raycast
        Vector2 originRight = new Vector2(rb.position.x + (collider2d.size.x / 2f), rb.position.y - (collider2d.size.y / 2f));
        Vector2 originLeft = new Vector2(rb.position.x - (collider2d.size.x / 2f), rb.position.y - (collider2d.size.y / 2f));

        // Draw Ray from origin point to check ground
        RaycastHit2D hitR = Physics2D.Raycast(originRight, Vector2.down, 0.05f, layerGround);
        RaycastHit2D hitL = Physics2D.Raycast(originLeft, Vector2.down, 0.05f, layerGround);

        // Ray hit the ground
        return hitR || hitL;
    }

}
