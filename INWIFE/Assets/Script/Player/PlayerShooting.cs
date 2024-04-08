using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;


    private PlayerBall _ball;
    [Header("Skill")]
    public float DiveDownSpeed = 20;

    // Update is called once per frame
    void Update()
    {
        // Check for user input to trigger shooting (e.g., using the spacebar)
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            _ball.DiveDown(DiveDownSpeed);
        }

    }

    void Shoot()
    {
        // Get the mouse position in the world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the direction from the fire point to the mouse position
        Vector2 targetDirection = (mousePosition - firePoint.position).normalized;

        // Use Mathf.Atan2 to find the angle in radians
        float angleRadians = Mathf.Atan2(targetDirection.y, targetDirection.x);

        // Convert the angle from radians to degrees
        float shootDegrees = Mathf.Rad2Deg * angleRadians;

        // Ensure the angle is positive (0 to 360 degrees)
        shootDegrees = (shootDegrees + 360) % 360;
        // Instantiate a new bullet at the fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        _ball = bullet.GetComponent<PlayerBall>();

        // Get the Rigidbody2D component of the bullet
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>(); 

        // Apply force to the bullet in the calculated direction;
        Vector2 direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        rb.AddForce((direction) * bulletSpeed, ForceMode2D.Impulse);
    }
}
