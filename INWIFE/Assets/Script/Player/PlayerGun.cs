using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    PlayerData data;
    PlayerInput input;
    private Rigidbody2D rb;

    public Transform firePoint;

    public int bulletMaxAmount;
    public int bulletAmount;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        data = GetComponent<Player>().PlayerData;

        bulletMaxAmount = data.bulletMaxAmount;
    }

    public void Shoot()
    {
        bulletAmount--;

        GameObject bullet = Instantiate(data.bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        float targetRad = (rb.rotation - 90) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2((float)Mathf.Cos(targetRad), (float)Mathf.Sin(targetRad));
        Vector2 targetDirection = FindTarget(direction);

        bulletRb.AddForce((targetDirection) * data.bulletSpeed, ForceMode2D.Impulse);
        //Debug.DrawRay(rb.position, direction, Color.green, 1f);
    }

    // return direction for target
    public Vector2 FindTarget(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(rb.position, direction, Mathf.Infinity, data.enemyLayer);

        if (hits.Length != 0)
        {
            Vector2 targetPosition = direction;
            float targetAngle = Mathf.Infinity;
            foreach (RaycastHit2D i in hits)
            {
                float diffAngle = Mathf.Abs(Vector2.Angle(direction, (Vector2)i.transform.position - rb.position));
                Debug.Log(i.rigidbody.gameObject.name);
                if (diffAngle < targetAngle)
                {
                    targetAngle = diffAngle;
                    targetPosition = i.transform.position;
                }
            }
            Debug.DrawRay(rb.position, (targetPosition - rb.position).normalized, Color.red, 1f);
            return (targetPosition - rb.position).normalized;
        }
        Debug.DrawRay(rb.position, direction, Color.blue, 1f);
        return direction;
    }

    public bool CanShoot()
    {
        return bulletAmount > 0;
    }

    public void AddBullet(int amount)
    {
        bulletAmount = Mathf.Min(bulletAmount + amount, bulletMaxAmount);
    }
}