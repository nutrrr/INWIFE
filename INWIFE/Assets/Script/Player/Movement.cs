using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D RB { get; private set; }
    private PlayerData data;

    public float CurrentmaxSpeed { get; private set; }
    public int FacingDirection { get; private set; }

    private void Awake()
    {
        RB = GetComponentInParent<Rigidbody2D>();
        data = GetComponent<Player>().PlayerData;

        FacingDirection = 1;
        CurrentmaxSpeed = data.maxSpeedNormal;
    }


    public void LogicUpdate()
    {
    }

    // ทำงานหลายรอบในครั้งเดียว
    public void Move(float MoveInput)
    {
        // Handle movement based on current state
        float targetSpeed = MoveInput;
        Vector2 direction = Vector2.right;
        float accelRate;

        accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.acceleration : data.deceleration;

        // apply user input values ​​or environment direction in horizontal to targetSpeed
        targetSpeed = targetSpeed * CurrentmaxSpeed;
        //Calculate difference between current velocity and desired velocity
        float speedDif = targetSpeed - RB.velocity.x;


        //Calculate force along x-axis to apply to thr player
        float movement = speedDif * accelRate;

        //Convert this to a vector and apply to rigidbody
        Debug.DrawLine(RB.position, RB.position + new Vector2(movement * direction.x, direction.y));
        RB.AddForce(movement * direction, ForceMode2D.Force);

        CheckIfShouldFlip((int)MoveInput);
    }


    public void Move(float MoveInput, Vector2 SlopeNormalPerp)
    {
        Vector2 targetSpeed = new Vector2(MoveInput, 0);
        float accelRate;
        Vector2 direction = -SlopeNormalPerp;
        // not moving(make player slide down slope)
        if (targetSpeed.x == 0)
        {
            // slope down to right
            if (SlopeNormalPerp.y > 0)
            {
                targetSpeed.x = 1;
                targetSpeed.y = -5f;

            }
            // slope down to left
            else if (SlopeNormalPerp.y < 0)
            {
                targetSpeed.x = -1;
                targetSpeed.y = -5f;
            }
        }
        // moving
        else
        {
            //go down to slope
            if (MoveInput * SlopeNormalPerp.y > 0)
            {
                targetSpeed.y = -5f;
            }
            // go up to slope
            else if (MoveInput * SlopeNormalPerp.y < 0)
            {
                targetSpeed.y = 1f;
            }
        }

        accelRate = (Mathf.Abs(targetSpeed.x) > 0.01f) ? data.acceleration : data.deceleration;

        // apply user input values ​​or environment direction in horizontal to targetSpeed
        targetSpeed = new Vector2(targetSpeed.x * CurrentmaxSpeed, targetSpeed.y * CurrentmaxSpeed * Mathf.Abs(direction.y));
        //Calculate difference between current velocity and desired velocity
        Vector2 speedDif = targetSpeed - RB.velocity;

        //Calculate force along x-axis to apply to thr player
        Vector2 movement = speedDif * accelRate;

        //Convert this to a vector and apply to rigidbody
        Debug.DrawLine(RB.position, RB.position + new Vector2(movement.x * direction.x, movement.y * Mathf.Abs(direction.y)));
        RB.AddForce(new Vector2(movement.x * direction.x, movement.y * Mathf.Abs(direction.y)), ForceMode2D.Force);
    }

    public void Jump()
    {
        float force = data.jumpForce;
        if (RB.velocity.y < 0)
            force -= RB.velocity.y;
        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    public void CheckIfShouldFlip(int xInput)
    {
        if (xInput != 0 && xInput != FacingDirection)
        {
            Flip();
        }
    }

    public void Flip()
    {
        FacingDirection *= -1;
        RB.transform.Rotate(0.0f, 180.0f, 0.0f);
    }
}
