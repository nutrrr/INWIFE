using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardBullet : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]private LayerMask hitableLayer;
    private bool hasHit;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnHitTaget(GameObject taget)
    {
        Destroy(taget);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            OnHitTaget(collision.gameObject);
        }
        if ((hitableLayer & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
        {
            Destroy(gameObject);
        }

    }
}
