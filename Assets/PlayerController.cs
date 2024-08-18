using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GroundedMovement))]
public class PlayerController : MonoBehaviour
{
    #region Variables

    // References to components on this object, its parent or any of its children
    private GroundedMovement groundedMovement;

    [Header("MOVEMENT")]
    [SerializeField] private float _jumpApexSpeedThreshold = 4f;
    [SerializeField] private float _fastFallGravityScaleMultiplier = 3f;
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _jumpBuffer = 0.1f;
    [HideInInspector] public float LastTimeJumpInputReceived { get; set; }
    [HideInInspector] public bool HasPressedJumpThisFrame { get; private set; }
    [HideInInspector] public float HorizontalInputDirection { get; set; }
    [HideInInspector] public float VerticalInputDirection { get; set; }

    [Header("ATTACKING/MINING")]
    [SerializeField] private float _attackBuffer = 0f;
    [SerializeField] private bool _canAttackWhilstAirborne = true;

    [HideInInspector] public bool IsAttacking { get; private set; }
    [HideInInspector] public float LastTimePrimaryAttackInputReceived { get; set; }
    [HideInInspector] public float LastTimeSecondaryAttackInputReceived { get; set; }
    [HideInInspector] public float HorizontalLookDirection { get; set; }
    [HideInInspector] public float VerticalLookDirection { get; set; }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        groundedMovement = GetComponent<GroundedMovement>();
    }

    private void Start()
    {
        HorizontalLookDirection = 1;
        VerticalLookDirection = 0;
    }

    private void Update()
    {
        CheckUserInput();
        TryJumping();
        TryRunning();
        TryFlipping();
        TryFastFalling();
    }

    #endregion

    #region Input Methods

    // Check for user input and update the input variables accordingly
    private void CheckUserInput()
    {
        HorizontalInputDirection = Input.GetAxisRaw("Horizontal");
        VerticalInputDirection = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Jump"))
        {
            LastTimeJumpInputReceived = Time.time;
        }
        if (Input.GetButton("Fire1"))
        {
            
        }
        if (Input.GetButton("Fire2"))
        {
            // TODO: Implement secondary attack
        }
    }

    #endregion

    #region Movement Methods

    // Flip the character if the direction of the horizontal input does not match the player's current direction
    private void TryFlipping()
    {
        if (HorizontalInputDirection != 0 && HorizontalLookDirection != HorizontalInputDirection)
        {
            groundedMovement.Flip();
            HorizontalLookDirection *= -1f;
            groundedMovement.HorizontalLookDirection = HorizontalLookDirection;
        }
    }

    // If the horizontal input direction of the player is not zero, accelerate the player in that direction. If it is zero, decelerate the player.
    private void TryRunning()
    {
        if (HorizontalInputDirection > 0)
        {
            groundedMovement.RunRight();
        }
        else if (HorizontalInputDirection < 0)
        {
            groundedMovement.RunLeft();
        }
        else
        {
            groundedMovement.Brake();
        }
    }

    // Check whether a jump is possible this frame and jump if so
    private void TryJumping()
    {
        bool hasJumpBuffer = Time.time - LastTimeJumpInputReceived <= _jumpBuffer;
        bool hasCoyoteTime = Time.time - groundedMovement.LastTimeGrounded <= _coyoteTime;
        bool isGrounded = groundedMovement.IsGrounded;
        bool isJumping = groundedMovement.HasJumpedSinceLeftGround;
        if (hasJumpBuffer && (isGrounded || hasCoyoteTime) && !isJumping)
        {
            // TODO: Implement async jump
            groundedMovement.Jump();
        }
    }

    // If the player is either falling or has ended their jump early, increase the gravity scale of the player
    // Else, set it to the default gravity scale of the player
    private void TryFastFalling()
    {
        bool isFalling = groundedMovement.Velocity.y <= _jumpApexSpeedThreshold && !groundedMovement.IsGrounded;
        //TODO: Remove jump input check here and fully decouple this class from input
        bool hasEndedJumpEarly = groundedMovement.Velocity.y > _jumpApexSpeedThreshold && !Input.GetButton("Jump") && !groundedMovement.IsGrounded;
        if (isFalling || hasEndedJumpEarly)
        {
            groundedMovement.GravityScale = groundedMovement.DefaultGravityScale * _fastFallGravityScaleMultiplier;
        }
        else
        {
            groundedMovement.GravityScale = groundedMovement.DefaultGravityScale;
        }
    }

    #endregion

    
}
