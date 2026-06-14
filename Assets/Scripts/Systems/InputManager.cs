using UnityEngine;

public class InputManager : MonoBehaviour
{
    // === SINGLETON ===
    public static InputManager Instance { get; private set; }

    // === INPUT BUFFER ===
    private float moveInput = 0f;
    private bool jumpPressed = false;
    private bool interactPressed = false;
    private bool swapPressed = false;
    private bool abilityPressed = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        // === MOVEMENT ===
        moveInput = Input.GetAxis("Horizontal");

        // === JUMP ===
        if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpPressed = true;
        }

        // === INTERACT ===
        if (Input.GetKeyDown(KeyCode.F) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            interactPressed = true;
        }

        // === SWAP CHARACTER ===
        if (Input.GetKeyDown(KeyCode.Q) ||
            Input.GetKeyDown(KeyCode.JoystickButton4)) // LB on gamepad
        {
            swapPressed = true;
        }

        // === ABILITY (Windy: fly) ===
        if (Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.JoystickButton2)) // Y on gamepad
        {
            abilityPressed = true;
        }
    }

    // === GETTERS (consume input once per frame) ===
    public float GetMoveInput()
    {
        return moveInput;
    }

    public bool GetJumpPressed()
    {
        bool result = jumpPressed;
        jumpPressed = false;
        return result;
    }

    public bool GetInteractPressed()
    {
        bool result = interactPressed;
        interactPressed = false;
        return result;
    }

    public bool GetSwapPressed()
    {
        bool result = swapPressed;
        swapPressed = false;
        return result;
    }

    public bool GetAbilityPressed()
    {
        bool result = abilityPressed;
        abilityPressed = false;
        return result;
    }

    public void DebugPrintInput()
    {
        Debug.Log($"Move: {moveInput} | Jump: {jumpPressed} | Swap: {swapPressed}");
    }
}