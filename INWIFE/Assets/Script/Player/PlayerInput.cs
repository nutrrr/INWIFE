using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Vector2 movementInput;
    [SerializeField] private bool jumpInput;
    [SerializeField] private bool jumpInputRelease;
    [SerializeField] private bool gildeInput;
    [SerializeField] private bool glideInputRelease;

    [SerializeField] private bool swapInput;
    [SerializeField] private bool attackInput;

    public void HandleInput()
    {
        // Get the horizontal and vertical input axes
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction
        movementInput = new Vector2(horizontalInput, verticalInput).normalized;

        // Check for jump input
        jumpInput = Input.GetKeyDown(KeyCode.W);
        jumpInputRelease = Input.GetKeyUp(KeyCode.W);

        gildeInput = Input.GetKey(KeyCode.K);
        glideInputRelease = Input.GetKeyUp(KeyCode.K);


        attackInput = Input.GetKeyDown(KeyCode.J);
        swapInput = Input.GetKeyDown(KeyCode.L);

    }

    public Vector2 GetHorizontalInput()
    {
        return movementInput;
    }
    public bool GetGlideInput(int type)
    {
        switch (type)
        {
            case 0:
                return gildeInput;
            case 1:
                return glideInputRelease;
            default:
                return gildeInput;
        }

    }

    public bool GetJumpInput(int type)
    {
        switch (type)
        {
            case 0:
                return jumpInput;
            case 1:
                return jumpInputRelease;
            default:
                return jumpInput;
        }
    }

    public bool GetSwapInput()
    {
        return swapInput;
    }

    public bool GetAttackInput()
    {
        return attackInput;
    }
}