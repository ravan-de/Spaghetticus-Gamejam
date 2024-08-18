using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class GroundedMovement : MonoBehaviour
{
    // References to components on this object, its parent or any of its children
    private Rigidbody2D _rigidbody;
    private BoxCollider2D _collider;

    [Header("GROUNDED MOVEMENT")]
    [SerializeField] private float _runningAcceleration = 90f;
    [SerializeField] private float _topRunningSpeed = 12f;
    [SerializeField] private float _runningDeceleration = 120f;
    [SerializeField] private float _jumpHeight = 12f;
    [SerializeField] private float _topFallingSpeed = -48f;
    [SerializeField] private float _topRisingSpeed = 60f;
    [HideInInspector] public float DefaultGravityScale { get; private set; }
    [HideInInspector] public float GravityScale { get { return _rigidbody.gravityScale; } set { _rigidbody.gravityScale = value; } }
    [HideInInspector] public Vector2 Velocity { get { return _rigidbody.velocity; } }
    [HideInInspector] public float LastTimeGrounded { get; private set; }
    [HideInInspector] public bool IsGrounded { get; private set; }
    [HideInInspector] public bool IsFalling { get; private set; }
    [HideInInspector] public bool HasJumpedSinceLeftGround { get; private set; }
    [HideInInspector] public float HorizontalLookDirection { get; set; }

    [Header("COLLISION")]
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private float _detectionRayLength = 0.1f;

    #region Unity Methods

    private void Awake()
    {
        _rigidbody = this.GetComponent<Rigidbody2D>();
        _collider = this.GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        DefaultGravityScale = _rigidbody.gravityScale;
        HorizontalLookDirection = 1;
    }

    private void Update()
    {
        CheckIfGrounded();
        CheckIfFalling();
        ClampVerticalVelocity();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        if (_collider != null)
        {
            Gizmos.DrawRay(_collider.bounds.center + new Vector3(_collider.bounds.extents.x, 0), (transform.up * -1f) * (_collider.bounds.extents.y + _detectionRayLength));
            Gizmos.DrawRay(_collider.bounds.center - new Vector3(_collider.bounds.extents.x, 0), (transform.up * -1f) * (_collider.bounds.extents.y + _detectionRayLength));
        }
    }

    #endregion

    #region Interface Methods

    // Called when this object is knocked back by something like an enemy attack
    public void OnKnockback(Vector2 knockbackDirection, float knockbackPower)
    {
        _rigidbody.velocity += knockbackDirection.normalized * knockbackPower;
    }

    #endregion

    #region Check Methods

    // Check if the entity is currently grounded based on its vertical velocity and the presence of a ground layer collider directly below the entity.
    private void CheckIfGrounded()
    {
        bool isGrounded = ColliderHitAt(transform.up * -1f) != null && Mathf.Abs(_rigidbody.velocity.y) < 0.02f;
        IsGrounded = isGrounded;
        HasJumpedSinceLeftGround = isGrounded ? false : HasJumpedSinceLeftGround;
        LastTimeGrounded = isGrounded ? Time.time : LastTimeGrounded;
    }

    // Check if the entity is currently falling based on the vertical velocity and jump apex threshold of the entity.
    private void CheckIfFalling()
    {
        bool isFalling = (_rigidbody.velocity.y <= 0.02f) && !IsGrounded;
        IsFalling = isFalling;
    }

    #endregion

    #region Movement Methods

    public void StartMovingTowards(Vector2 movementDirection)
    {
        throw new System.NotImplementedException();
    }

    public void StopMoving()
    {
        throw new System.NotImplementedException();
    }

    // Flip this entity on the x-axis by rotating it 180 degrees
    public void Flip()
    {
        transform.Rotate(0f, 180f, 0f);
    }

    // Accelerate the velocity of this entity to the right, accounting for its acceleration and top speed.
    public void RunRight()
    {
        float newRunningSpeed = Mathf.Min(_rigidbody.velocity.x + (_runningAcceleration * Time.deltaTime), _topRunningSpeed);
        _rigidbody.velocity = new Vector2(newRunningSpeed, _rigidbody.velocity.y);
    }

    // Accelerate the velocity of this entity to the left, accounting for its acceleration and top speed.
    public void RunLeft()
    {
        float newRunningSpeed = Mathf.Max(_rigidbody.velocity.x - (_runningAcceleration * Time.deltaTime), -_topRunningSpeed);
        _rigidbody.velocity = new Vector2(newRunningSpeed, _rigidbody.velocity.y);
    }

    // Decelerate the entity to standstill
    public void Brake()
    {
        if (Mathf.Abs(_rigidbody.velocity.x) > 0.005f)
        {
            float movementDirection = _rigidbody.velocity.x / Mathf.Abs(_rigidbody.velocity.x);
            _rigidbody.velocity -= Vector2.right * movementDirection * _runningDeceleration * Time.deltaTime;
            float newMovementDirection = _rigidbody.velocity.x / Mathf.Abs(_rigidbody.velocity.x);
            if (movementDirection != newMovementDirection)
            {
                _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
            }
        }
    }

    // Make the entity jump based on the given jump height. Returns true if the entity was succesful in performing a jump, else returns false.
    public void Jump()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpHeight);
        HasJumpedSinceLeftGround = true;
    }

    // Ensure the entity will never fall faster than the top falling speed and rise faster top rising speed.
    private void ClampVerticalVelocity()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, Mathf.Clamp(_rigidbody.velocity.y, _topFallingSpeed, _topRisingSpeed));
    }

    // TODO: Make flying/freeform/weightless movement class in the future
    // Move this entity in the direction of the move direction parameter with the given move speed.
    private void MoveTowards(Vector2 moveDirection)
    {
        _rigidbody.velocity += moveDirection.normalized * _runningAcceleration * Time.deltaTime;
        _rigidbody.velocity = Vector2.ClampMagnitude(_rigidbody.velocity, _topRunningSpeed);
    }


    #endregion

    #region Collision detection methods

    // This method casts a box in the parameter direction and returns whether a collider was hit
    private Collider2D ColliderHitAt(Vector2 direction)
    {
        Vector3 boxOrigin = _collider.bounds.center;
        Vector3 boxSize = new Vector3(_collider.bounds.size.x - 0.02f, _collider.bounds.size.y);
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxOrigin, boxSize, 0f, direction, _detectionRayLength, _groundLayers);
        return raycastHit.collider;
    }



    #endregion
}
