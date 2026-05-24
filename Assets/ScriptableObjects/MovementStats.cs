using UnityEngine;

[CreateAssetMenu(fileName = "MovementStats", menuName = "Scriptable Objects/MovementStats")]
public class MovementStats : ScriptableObject
{
    [Header("Movement Stats")]
    [Range(0,100)]
    public float maxWalkSpeed;

    [Range(0, 100)]
    public float maxRunSpeed;

    [Range(0, 10)]
    public float groundAcceleration;

    [Range(0, 10)]
    public float groundDeceleration;

    [Range(0, 10)]
    public float airAcceleration;

    [Range(0, 10)]
    public float airDeceleration;

    public bool retainHorizontalMomentum = true;
    //---------------------------------------
    [Header("Jump")]

    [Range(1f,50f)] public float jumpHeight;
    [Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.054f;
    [Range(1f, 5f)] public float gravityFallMultiplier;
    [Range(1f, 5f)] public float gravityFastFallMultiplier;
    [Range(1f, 3f)] public int jumpsAllowed;
    [Range(0.0f, 0.5f)] public float jumpBufferTimer;
    public float timeTillApexPoint;

    public float gravity { get; private set; }
    public float initialJumpVelocity { get; private set; }

    public float adjustedJumpHeight { get; private set; }

    private void OnValidate()
    {
        InitializeValues();
    }

    private void OnEnable()
    {
        InitializeValues();
    }

    private void InitializeValues()
    {
        adjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
        gravity = -(2 * adjustedJumpHeight) / Mathf.Pow(timeTillApexPoint, 2);
        initialJumpVelocity = (2 * adjustedJumpHeight) / timeTillApexPoint;
    }
    //---------------------------------------

    [Header("Ground check")]
    public LayerMask groundLayer;

    [Range(0, 0.5f)]
    public float groundCheckRadius = 0.3f;
}
