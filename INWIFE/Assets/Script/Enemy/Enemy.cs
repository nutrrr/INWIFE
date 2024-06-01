using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private GameObject taget;
    private Rigidbody2D rb;

    public float speed;
    private void Start()
    {
        taget = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 targetDirection = (taget.transform.position - gameObject.transform.position).normalized;
        rb.velocity = targetDirection * speed;

    }
}
