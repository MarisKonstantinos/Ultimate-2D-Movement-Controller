using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    public static InputManager InputManagerInstance { get { return _instance; } }

    public static Vector2 moveInput;
    public static bool sprintInput;
    public static bool jumpWasPressed;
    public static bool jumpWasReleased;
    public static bool jumpIsHeld;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            sprintInput = true;
        }

        if(context.canceled)
        {
            sprintInput = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) 
        { 
            jumpWasPressed = true;
            jumpIsHeld = true;
        }

        if (context.canceled) {
            jumpIsHeld = false;
            jumpWasPressed = false;
            jumpWasReleased = true; 
        }
    }
}
