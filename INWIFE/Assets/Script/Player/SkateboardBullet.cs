using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardBullet : MonoBehaviour
{
    // Start is called before the first frame update
    private bool hasHit;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnHitTaget()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player"){return;}

        if (collision.tag == "Enemy")
        {
            OnHitTaget();
        }
        Destroy(gameObject);

    }
}
