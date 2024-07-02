using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public Rigidbody2D rb;
    public bool isWallRight;

    private PlayerMoveData moveData;

    [Header("Dash")]
    public float DashRange; //Range of the player's dash
    public float DashTimeToApex; //Time between applying the dash force and reaching the desired dash range. These values also control the player's gravity and dash force.
    [HideInInspector] public float DashForce; //The actual force applied (upwards) to the player when they jump.

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Dash();
        }
    }

    void Dash()
    {
        #region Force
        //Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
        float gravityStrength = -(2 * DashRange) / (DashTimeToApex * DashTimeToApex);
        DashForce = Mathf.Abs(gravityStrength) * DashTimeToApex;

        #endregion

        #region Direction
        // Get the mouse position in the world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the direction from the fire point to the mouse position
        Vector2 targetDirection = (mousePosition - gameObject.transform.position).normalized;

        // Use Mathf.Atan2 to find the angle in radians
        float angleRadians = Mathf.Atan2(targetDirection.y, targetDirection.x);

        // Convert the angle from radians to degrees
        float shootDegrees = Mathf.Rad2Deg * angleRadians;

        // Ensure the angle is positive (0 to 360 degrees)
        shootDegrees = (shootDegrees + 360) % 360;
        #endregion

        Vector2 direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        rb.velocity = new Vector2(0, 0);
        rb.AddForce((direction) * DashRange, ForceMode2D.Impulse);
    }
}
