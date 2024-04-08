using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBall : MonoBehaviour
{
    private Rigidbody2D _rb;
    [SerializeField]private CircleCollider2D circleCollider;
    private bool hasHit;
// Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        circleCollider.enabled = false;

    }

    // Update is called once per frame

    public void Init()
    {
    }

    public void DiveDown(float speed)
    {
        _rb.velocity = new Vector2(0, -speed); 
    }

    public void OnHitTaget(Vector2 posTaget)
    {
        Vector2 targetDirection = (_rb.position) - (posTaget);
        targetDirection = targetDirection.normalized;
        
        _rb.velocity = Vector2.Reflect(_rb.velocity, targetDirection);
        //Debug.Log(new Vector2(gameObject.transform.position.x, gameObject.transform.position.y));
        Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + new Vector3(targetDirection.x, targetDirection.y, 0), Color.red, 3);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Player")
        {
            Vector2 collisionPoint = collision.ClosestPoint(transform.position);
            circleCollider.enabled = true;
            
            hasHit = true;
            OnHitTaget(collisionPoint);
        }

        if(collision.tag == "Player" && hasHit)
        {
            Destroy(gameObject);
        }
    }
}
