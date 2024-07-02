using System;
using System.Collections.Generic;
using UnityEngine;

public class Pow_Glider : MonoBehaviour
{

    private bool isFacingRight;
    [SerializeField] public float fuel;
    private float fuelCounter;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMult;
    [SerializeField] private float fastFallGravityMult;


    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float maxFastFallSpeed;
    public float glideMinSpeed;
    public float glideMaxSpeed;
    public float glideFastMaxSpeed;

    [Range(0f, 1)] public float acceleration; //Multipliers applied to acceleration rate.
    [Range(0f, 1)] public float decceleration; //Multipliers applied to decceleration rate.

    //Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
    public float AccelAmount()
    {
        return (50 * acceleration) / glideMaxSpeed;

    }
    public float DeccelAmount()
    {
        return (50 * decceleration) / glideMaxSpeed;
    }


    [Range(0f, 1)] public float fastAcceleration; //Multipliers applied to acceleration rate.

    //Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
    public float fastAccelAmount()
    {
        return (50 * acceleration) / glideMaxSpeed;

    }

    private PlayerMoveData data;
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool glideInput;
    private bool glideInputRelease;
    public void Init(Rigidbody2D rb, PlayerMoveData data)
    {
        this.rb = rb;
        this.data = data;

    }

    public void InitTheInput(Vector2 input, bool glideInput, bool glideInputRelease, bool IsFacingRight)
    {
        movementInput = input;
        this.glideInput = glideInput;
        this.glideInputRelease = glideInputRelease;
        isFacingRight = IsFacingRight;

    }

    private void Start()
    {
        fuelCounter = fuel;

    }

    public void HandleGlider()
    {

        if (CanGilde())
        {
            Glide();
            fuelCounter -= Time.fixedDeltaTime;

        }

    }

    void Glide()
    {
        int intIsFacingRight = isFacingRight ? 1 : -1;
        /* isRight ? 1 : -1     is if else statement
         * if(isRight){ return 1; }
         * else{ retrun -1; }
         */

        if (movementInput.x == 0)
        {
            #region Horizontal
            //Calculate the direction we want to move in and our desired velocity
            float targetSpeed = (intIsFacingRight) * glideMaxSpeed;
            #region Calculate AccelRate
            float accelRate;

            //Gets an acceleration value based on if we are accelerating (includes turning) 
            //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
            accelRate = AccelAmount() * acceleration;
            #endregion
            //Calculate difference between current velocity and desired velocity
            float speedDif = targetSpeed - rb.velocity.x;
            //Calculate force along x-axis to apply to thr player

            float movement = speedDif * accelRate;

            //Convert this to a vector and apply to rigidbody
            rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
            //rb.velocity = new Vector2(Mathf.Max(intIsFacingRight * glideMinSpeed, rb.velocity.x + (Time.fixedDeltaTime * speedDif * accelRate) / rb.mass), rb.velocity.y);
            #endregion

        }
        else if (movementInput.x == intIsFacingRight)
        {
            #region Horizontal
            //Calculate the direction we want to move in and our desired velocity
            float targetSpeed = movementInput.x * glideFastMaxSpeed;
            #region Calculate AccelRate
            float accelRate;

            //Gets an acceleration value based on if we are accelerating (includes turning) 
            //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
            accelRate = fastAccelAmount() * fastAcceleration;
            #endregion
            //Calculate difference between current velocity and desired velocity
            float speedDif = targetSpeed - rb.velocity.x;
            //Calculate force along x-axis to apply to thr player

            float movement = speedDif * accelRate;

            //Convert this to a vector and apply to rigidbody
            rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
            #endregion
        }
        else
        {
            #region Horizontal
            //Calculate the direction we want to move in and our desired velocity
            float targetSpeed = movementInput.x * glideMinSpeed;
            #region Calculate AccelRate
            float accelRate;

            //Gets an acceleration value based on if we are accelerating (includes turning) 
            //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
            accelRate = DeccelAmount() * decceleration;
            #endregion
            //Calculate difference between current velocity and desired velocity
            float speedDif = targetSpeed - rb.velocity.x;
            //Calculate force along x-axis to apply to thr player

            float movement = speedDif * accelRate;

            //Convert this to a vector and apply to rigidbody
            rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
            #endregion
        }

        #region Vertical
        if (rb.velocity.y <= 0 && movementInput.x == (intIsFacingRight))
        {
            //Much higher gravity if holding move forward
            SetGravityScale(data.gravityScale * fastFallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFastFallSpeed));
        }
        else if (rb.velocity.y <= 0)
        {
            //Default gravity if do nothing
            SetGravityScale(data.gravityScale * fallGravityMult);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));

        }
        else
        {
            //Default gravity if standing on a platform or moving upwards
            SetGravityScale(data.gravityScale);
        }
        #endregion



    }

    public bool CanGilde()
    {
        if (fuelCounter > 0)
        {
            return true;
        }
        return false;


    }

    public void Refule()
    {
        fuelCounter = fuel;
    }


    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;

    }
}