using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Public variables
    [Header("References")]
    public MovementStats movementStats;
    [SerializeField] private Transform playerFeet;

    //----------------

    private Rigidbody player_rb;
    private bool isGrounded = false;
    private bool isFacingRight = true;
    private Vector2 targetVelocity;
    private float verticalVelocity;
    private bool isFalling;
    private bool initiateJump;
    private bool isJumping;
    private int totalJumpsAllowed;
    private int remainingJumpsAllowed;
    private float _jumpBufferTimer;

    private void Awake()
    {
        player_rb = GetComponent<Rigidbody>();
        totalJumpsAllowed = movementStats.jumpsAllowed;
        remainingJumpsAllowed = totalJumpsAllowed;
    }

    private void Update()
    {
        JumpInputHandler();
    }

    private void FixedUpdate()
    {
        if(isGrounded)
        {
            Move(movementStats.groundAcceleration, movementStats.groundDeceleration, InputManager.moveInput);
        }
        else
        {
            Move(movementStats.airAcceleration, movementStats.airDeceleration, InputManager.moveInput);
        }

        GroundCheck();
        Turn();
        JumpPhysicsHandler();
    }

    #region Move
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (!player_rb) return;

        float targetSpeed = InputManager.sprintInput ? movementStats.maxRunSpeed : movementStats.maxWalkSpeed;
        
        if (moveInput != Vector2.zero)
        {
            Vector2 newVelocity;
            if ((Mathf.Abs(player_rb.linearVelocity.x) > 0) && ((isFacingRight && moveInput.x < 0) || (!isFacingRight && moveInput.x > 0)) && movementStats.retainHorizontalMomentum)
            {
                newVelocity = new Vector2(-1 * player_rb.linearVelocity.x, player_rb.linearVelocity.y);
            }
            else
            {
                targetVelocity = new Vector2(targetSpeed * moveInput.x, player_rb.linearVelocity.y);
                newVelocity = Vector2.Lerp(player_rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }

            player_rb.linearVelocity = newVelocity;
        }
        else if (moveInput == Vector2.zero)
        {
            targetVelocity = new Vector2(0, player_rb.linearVelocity.y);
            Vector2 newVelocity = Vector2.Lerp(player_rb.linearVelocity, targetVelocity, deceleration * Time.fixedDeltaTime);
            player_rb.linearVelocity = newVelocity;
        }
    }

    private void Turn()
    {
        if (isFacingRight && InputManager.moveInput.x < 0)
        {
            player_rb.rotation = Quaternion.Euler(0, 180, 0);
            isFacingRight = false;
        }

        if (!isFacingRight && InputManager.moveInput.x > 0)
        {
            player_rb.rotation = Quaternion.identity;
            isFacingRight = true;
        }

    }
    #endregion

    #region Jump

    private void JumpInputHandler()
    {
        //if ((InputManager.jumpWasPressed && isGrounded) || (InputManager.jumpWasPressed && remainingJumpsAllowed > 0 && !isGrounded && isJumping) || (InputManager.jumpWasPressed && !isGrounded))
        if(InputManager.jumpWasPressed)
        {
            InputManager.jumpWasPressed = false;
            _jumpBufferTimer = movementStats.jumpBufferTimer;
        }
    }
    
    private void JumpPhysicsHandler()
    {
        if (_jumpBufferTimer > 0)
            _jumpBufferTimer -= Time.fixedDeltaTime;
        else
            _jumpBufferTimer = 0;

        Debug.Log("remaining jumps: " + remainingJumpsAllowed);
        if (_jumpBufferTimer > 0 && (isGrounded || remainingJumpsAllowed > 0))
        {
            _jumpBufferTimer = 0;
            isJumping = true;
            isFalling = false;
            verticalVelocity = movementStats.initialJumpVelocity;
            remainingJumpsAllowed--;

            if (remainingJumpsAllowed < 0)
                remainingJumpsAllowed = movementStats.jumpsAllowed - 1;
        }

        //Player ascending
        if (verticalVelocity > 0 && InputManager.jumpIsHeld)
        {
            //Debug.Log("Ascending");
            verticalVelocity += movementStats.gravity * Time.fixedDeltaTime;
        }
        //Player descending
        else if ((verticalVelocity <= 0 || !InputManager.jumpIsHeld) && !isGrounded)
        {
            //Debug.Log("Descending");
            isFalling = true;
            verticalVelocity += movementStats.gravityFastFallMultiplier * movementStats.gravity * Time.fixedDeltaTime;
        }
        //Player landed
        //Overwrites the initial velocity for the buffer. 
        else if (isJumping && isGrounded && isFalling)
        {
            remainingJumpsAllowed = totalJumpsAllowed;
            isFalling = false;
            verticalVelocity = 0;
            isJumping = false;
        }

        player_rb.linearVelocity = new Vector2(player_rb.linearVelocity.x, verticalVelocity);
    }
    #endregion

    #region CollisionChecks
    private void GroundCheck()
    {
        if (playerFeet == null) return;

        isGrounded = Physics.CheckSphere(playerFeet.position, movementStats.groundCheckRadius, movementStats.groundLayer);
    }

    //Add function for headbump

    #endregion

    #region Debug functions
    private void OnDrawGizmos()
    {
        //---------GROUND CHECK GIZMO---------------------
        if (playerFeet == null) return;

        if (isGrounded)
        {
            Gizmos.color = Color.red; // Set the color for the circle
        }
        else
        {
            Gizmos.color = Color.green; // Set the color for the circle
        }
        Gizmos.DrawWireSphere(playerFeet.position, movementStats.groundCheckRadius); // Draw a wire sphere at the ground check position
        //------------------------------------------------
    }
    #endregion
}
