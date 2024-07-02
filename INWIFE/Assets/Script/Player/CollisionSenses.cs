using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSenses : MonoBehaviour
{
    private Rigidbody2D RB;

    private PlayerData data;

    [Header("Slope")]
    [SerializeField] private float maxSlopeAngle = 60f;
    [SerializeField] private float slopeCheckDistance = 0.05f;
    public Vector2 SlopeNormalPerp { get; private set; }
    public float SlopeDownAngle { get; private set; }
    public float SlopeSideAngle { get; private set; }
    private float lastSlopeAngle;

    public float CoyoteTimeCounter { get; private set; }

    public bool AttachedGround { get; private set; }

    public bool isOnSlope { get; private set; }
    public bool isFloating { get; set; }
    private bool isFalling;

    private void Awake()
    {
        RB = GetComponentInParent<Rigidbody2D>();
        data = GetComponent<Player>().PlayerData;
    }

    public void LogicUpdate()
    {
        if (RB.velocity.y <= 0)
        {
            isFloating = false;
        }

        if (!isFloating)
        {
            CheckGrounded();
        }
        SlopeCheck();

        CoyoteTimeCounter -= Time.deltaTime;
    }

    public void JumpHasDone()
    {
        CoyoteTimeCounter = 0;
        isFloating = true;
    }

    public bool IsGrounded()
    {
        return CoyoteTimeCounter > 0;
    }

    void CheckGrounded()
    {
        CapsuleCollider2D collider2d = GetComponent<CapsuleCollider2D>();
        // Set origin point below player for Raycast
        Vector2 originRight = new Vector2(RB.position.x + (collider2d.size.x / 2f), RB.position.y - (collider2d.size.y / 2f));
        Vector2 originLeft = new Vector2(RB.position.x - (collider2d.size.x / 2f), RB.position.y - (collider2d.size.y / 2f));
        // Draw Ray from origin point to check ground
        RaycastHit2D hitR = Physics2D.Raycast(originRight, Vector2.down, 0.05f, data.layerGround);
        RaycastHit2D hitL = Physics2D.Raycast(originLeft, Vector2.down, 0.05f, data.layerGround);

        // Ray hit the ground
        if (hitR.collider != null || hitL.collider != null)
        {
            CoyoteTimeCounter = data.coyoteTime;
            AttachedGround = true;
            return;
        }

        // Ray not hit the ground
        AttachedGround = false;
        return;
    }

    private void SlopeCheck()
    {
        CapsuleCollider2D collider2d = GetComponent<CapsuleCollider2D>();
        Vector2 origin = new Vector2(RB.position.x, RB.position.y - (collider2d.size.y / 2f));

        SlopeCheckHorizontal(origin);
        SlopeCheckVertical(origin);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, Vector2.right, slopeCheckDistance, data.layerGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -Vector2.right, slopeCheckDistance, data.layerGround);

        if (slopeHitFront)
        {
            isOnSlope = true;

            SlopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            isOnSlope = true;

            SlopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            SlopeSideAngle = 0.0f;
            isOnSlope = false;
        }

    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, data.layerGround);

        if (hit)
        {

            SlopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            SlopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (SlopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }

            lastSlopeAngle = SlopeDownAngle;

            Debug.DrawRay(hit.point, SlopeNormalPerp, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }

        if (SlopeDownAngle > maxSlopeAngle || SlopeSideAngle > maxSlopeAngle)
        {
        }
        else
        {
        }
    }

    Vector2 CollideSlide(Vector2 origin, Vector2 direction, float length)
    {
        if (length <= 0) { return Vector2.zero; }
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, length, data.layerGround);
        Vector2 slopeNormalPerp;

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            Debug.DrawRay(origin, hit.point, Color.red);
            if (slopeNormalPerp.x * slopeNormalPerp.y != 0)
            {
                return (hit.point - origin) + CollideSlide(hit.point, slopeNormalPerp, length - slopeNormalPerp.magnitude);
            }
            else
            {
                return Vector2.zero;
            }
        }
        else    
        {
            Debug.DrawRay(origin, (direction - origin) * length, Color.red);
            return (direction - origin) * length;
        }
    }

}
