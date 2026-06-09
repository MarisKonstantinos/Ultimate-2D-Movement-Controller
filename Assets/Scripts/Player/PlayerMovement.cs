using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //------PUBLIC VARS------
    [Header("References")]
    public MovementStats movementStats;
    [SerializeField] private Transform playerFeet;
    [SerializeField] private Transform playerHead;

    //------PRIVATE VARS------
    private Rigidbody player_rb;
    private Vector2 targetVelocity;
    private float verticalVelocity;

    //------CHECKS------
    private bool isGrounded = false;
    private bool hitCeiling = false;
    private bool isFacingRight = true;
    private bool isFastFalling;
    private bool isFalling;
    private bool isJumping;
    private int remainingJumpsAllowed;

    //------TIMERS------
    private float _jumpBufferTimer;
    private float _apexHangTimer;
    private float _coyoteTimer;

    //------DEBUGS------
    [Header("Debug")]
    [SerializeField] private bool drawJumpTrajectory = true;
    [SerializeField] private int trajectoryResolution = 60;
    [SerializeField] private float trajectoryTimeStep = 0.05f;
    [SerializeField] private Color walkTrajectoryColor = Color.yellow;
    [SerializeField] private Color runTrajectoryColor = Color.cyan;

    private void Awake()
    {
        player_rb = GetComponent<Rigidbody>();
        remainingJumpsAllowed = movementStats.jumpsAllowed;
    }

    private void Update()
    {
        JumpInputHandler();
    }

    private void FixedUpdate()
    {
        if (isGrounded)
        {
            Move(movementStats.groundAcceleration, movementStats.groundDeceleration, InputManager.moveInput);
        }
        else
        {
            Move(movementStats.airAcceleration, movementStats.airDeceleration, InputManager.moveInput);
        }

        GroundCheck();
        CeilingCheck();
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
        if (InputManager.jumpWasPressed)
        {
            InputManager.jumpWasPressed = false;
            _jumpBufferTimer = movementStats.jumpBufferTimer;
        }
    }

    private void JumpPhysicsHandler()
    {
        TimersCountdown();

        //Initiating jump
        if (_jumpBufferTimer > 0 && (isGrounded || _coyoteTimer > 0 || (remainingJumpsAllowed > 0 && isJumping)))
        {
            _jumpBufferTimer = 0;
            isJumping = true;
            isFalling = false;
            isFastFalling = false;
            

            //If the player is falling without jumping he shouldn't be able to use more jumps
            if (isGrounded || (remainingJumpsAllowed > 0 && !isFastFalling))
            {
                remainingJumpsAllowed--;
            }
            else
            {
                _coyoteTimer = 0;
                remainingJumpsAllowed = 0;
            }

            verticalVelocity = movementStats.initialJumpVelocity;
        }

        //Player ascending
        if (verticalVelocity > 0 && InputManager.jumpIsHeld && !hitCeiling)
        {
            //Debug.Log("Ascending");
            verticalVelocity += movementStats.gravity * Time.fixedDeltaTime;
        }
        //Player falling
        else if (isJumping && !isGrounded && !hitCeiling && !isFastFalling && (verticalVelocity <= 0 || !InputManager.jumpIsHeld))
        {
            //Debug.Log("Descending");
            if(!isFalling)
            {
                isFalling = true;
                _apexHangTimer = movementStats.apexHangTime;
            }

            float gravity = movementStats.gravityFallMultiplier * 
                            movementStats.gravity * 
                            Time.fixedDeltaTime;

            if (verticalVelocity <= 0 && _apexHangTimer > 0)
            {
                verticalVelocity = 0;
            }
            else
            {
                verticalVelocity += gravity;
            }
        }
        //Player either falls of a cliff or hit a ceiling while jumping
        else if((verticalVelocity <= 0 && !isJumping && !isGrounded) || hitCeiling || (isFastFalling && isJumping && !isGrounded))
        {
            //Debug.Log("Fast falling ");
            if (!isFastFalling)
            {
                verticalVelocity = 0;
                _coyoteTimer = movementStats.coyoteTime;
                isFastFalling = true;
            }
            verticalVelocity += movementStats.gravityFastFallMultiplier * movementStats.gravity * Time.fixedDeltaTime;
        }
        //Player landed 
        else if (isGrounded && (isFalling || isFastFalling))
        {
            remainingJumpsAllowed = movementStats.jumpsAllowed;
            verticalVelocity = 0;
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
        }

        verticalVelocity = Mathf.Clamp(verticalVelocity, -movementStats.maxFallSpeed, movementStats.initialJumpVelocity); 
        player_rb.linearVelocity = new Vector2(player_rb.linearVelocity.x, verticalVelocity);
    }

    private void TimersCountdown()
    {
        if (_jumpBufferTimer > 0)
            _jumpBufferTimer -= Time.fixedDeltaTime;
        else
            _jumpBufferTimer = 0;

        if (_apexHangTimer > 0)
            _apexHangTimer -= Time.fixedDeltaTime;
        else
            _apexHangTimer = 0;

        if (_coyoteTimer > 0)
            _coyoteTimer -= Time.fixedDeltaTime;
        else
            _coyoteTimer = 0;
    }
    #endregion

    #region CollisionChecks
    private void GroundCheck()
    {
        if (playerFeet == null) return;

        isGrounded = Physics.CheckSphere(playerFeet.position, movementStats.groundCheckRadius, movementStats.groundLayer);
    }

    private void CeilingCheck()
    {
        if(playerHead == null) return;

        hitCeiling = Physics.CheckSphere(playerHead.position, movementStats.ceillingCheckRadius, movementStats.groundLayer);
    }
    #endregion

    #region Debug functions
    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawGroundChecks();

            if (drawJumpTrajectory)
            {
                DrawJumpTrajectory(movementStats.maxWalkSpeed, walkTrajectoryColor);
                DrawJumpTrajectory(movementStats.maxRunSpeed, runTrajectoryColor);
            }
        }

        private void DrawGroundChecks()
        {
            if (playerFeet == null) return;
         
            //---------GROUND CHECK GIZMO---------------------
            Gizmos.color = isGrounded ? Color.red : Color.green;
            Gizmos.DrawWireSphere(playerFeet.position, movementStats.groundCheckRadius); // Draw a wire sphere at the ground check position

            //---------CEILLING CHECK GIZMO---------------------
            Gizmos.color = hitCeiling ? Color.red : Color.green;
            Gizmos.DrawWireSphere(playerHead.position, movementStats.groundCheckRadius); // Draw a wire sphere at the ground check position
        }

        private void DrawJumpTrajectory(float horizontalVelocity, Color color)
        {
            Gizmos.color = color;

            Vector3 currentPosition = transform.position;

            float verticalVelocity =
                movementStats.initialJumpVelocity;

            bool simulatedFalling = false;
            float simulatedApexHangTimer = 0f;

            Vector3 previousPoint = currentPosition;

            for (int i = 0; i < trajectoryResolution; i++)
            {
                //------------------------------------
                // Horizontal Movement
                //------------------------------------
                currentPosition.x +=
                    horizontalVelocity * trajectoryTimeStep;

                //------------------------------------
                // Ascending
                //------------------------------------
                if (verticalVelocity > 0)
                {
                    verticalVelocity +=
                        movementStats.gravity *
                        trajectoryTimeStep;
                }
                //------------------------------------
                // Start Apex Hang
                //------------------------------------
                else if (!simulatedFalling)
                {
                    simulatedFalling = true;
                    simulatedApexHangTimer =
                        movementStats.apexHangTime;
                }

                //------------------------------------
                // Apex Hang
                //------------------------------------
                if (simulatedFalling &&
                    simulatedApexHangTimer > 0)
                {
                    simulatedApexHangTimer -=
                        trajectoryTimeStep;

                    verticalVelocity = 0;
                }
                //------------------------------------
                // Falling
                //------------------------------------
                else if (simulatedFalling)
                {
                    verticalVelocity +=
                        movementStats.gravity *
                        movementStats.gravityFallMultiplier *
                        trajectoryTimeStep;
                }

                //------------------------------------
                // Apply Vertical Movement
                //------------------------------------
                currentPosition.y +=
                    verticalVelocity * trajectoryTimeStep;

                Gizmos.DrawLine(previousPoint, currentPosition);

                previousPoint = currentPosition;
            }
        }
    #endif
    #endregion
}

