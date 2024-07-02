using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Rigidbody2D rb;
    public float smoothPositionSpeed = 10;
    public float smoothRotationSpeed = 0.125f;
    public float velocityOffset = 5f;
    public Vector2 MaxOffset;
    public Vector3 locationOffset;
    public Vector3 rotationOffset;

    private void Start()
    {
        transform.position = rb.transform.position + locationOffset;
    }

    void Update()
    {
        float posSpeed = smoothPositionSpeed;
        if (rb.velocity.sqrMagnitude < 5)
        {
            posSpeed = 0;
        }
        Vector3 value = new Vector2(Mathf.Min(Mathf.Abs(rb.velocity.x), MaxOffset.x), Mathf.Min(Mathf.Abs(rb.velocity.y), MaxOffset.y));
        Vector3 dis = rb.velocity.normalized * value;
        Vector3 desiredPosition = rb.transform.position + dis + locationOffset;
        Vector3 smoothedPosition = Vector3.MoveTowards(transform.position, desiredPosition, (Mathf.Abs(rb.velocity.sqrMagnitude)) * Time.deltaTime);
        transform.position = smoothedPosition;

        //Quaternion desiredrotation = rb.transform.rotation * Quaternion.Euler(rotationOffset);
        //Quaternion smoothedrotation = Quaternion.Lerp(transform.rotation, desiredrotation, smoothRotationSpeed * Time.fixedDeltaTime);
        //transform.rotation = smoothedrotation;
    }
}