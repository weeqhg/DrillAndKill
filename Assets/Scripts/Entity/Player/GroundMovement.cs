using UnityEngine;

public class MovementController
{
    private PlayerMovement _player;
    private Rigidbody _rb;
    
    private float _rotationSpeed = 20f;
    private float _groundAcceleration = 100f;
    private float _airAcceleration = 35f;
    private float _groundDeceleration = 100f;
    private float _slopeStickForce = 18f;
    private float _bunnyHopDecay = 5f;
    
    private float _currentBonusSpeed;
    
    public MovementController(PlayerMovement player)
    {
        _player = player;
        _rb = player.Rb;
    }
    
    public void HandleMovement(Vector2 moveInput, bool isGrounded)
    {
        if (_player.CameraTransform == null) return;
        
        Vector3 moveDirection = GetCameraRelativeDirection(moveInput);
        Vector3 groundNormal = _player.GetGroundNormal();
        
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        bool onSlope = isGrounded && slopeAngle > 5f;
        
        if (onSlope)
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
        
        float targetSpeed = GetTargetSpeed(moveInput);
        ApplyHorizontalMovement(moveDirection, targetSpeed, onSlope, isGrounded);
    }
    
    public void HandleRotation(bool shouldRotate)
    {
        if (!shouldRotate || _player.CameraTransform == null) return;
        
        float yRotation = _player.CameraTransform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0f, yRotation, 0f);
        
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime));
    }
    
    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        Vector3 forward = _player.CameraTransform.forward;
        Vector3 right = _player.CameraTransform.right;
        
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        Vector3 direction = forward * input.y + right * input.x;
        if (direction.sqrMagnitude > 1f) direction.Normalize();
        
        return direction;
    }
    
    private float GetTargetSpeed(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude < 0.01f) return 0f;
        
        bool canSprint = moveInput.y > 0.1f;
        float baseSpeed = canSprint ? _player.Stats.MoveSpeed * 1.65f : _player.Stats.MoveSpeed;
        
        return baseSpeed + _currentBonusSpeed;
    }
    
    private void ApplyHorizontalMovement(Vector3 direction, float targetSpeed, bool onSlope, bool isGrounded)
    {
        Vector3 velocity = _rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 targetVelocity = direction * targetSpeed;
        
        float accelRate = isGrounded
            ? (targetSpeed > 0.01f ? _groundAcceleration : _groundDeceleration)
            : _airAcceleration;
        
        if (onSlope) accelRate *= 0.6f;
        
        Vector3 newHorizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);
        
        _rb.linearVelocity = new Vector3(newHorizontalVelocity.x, _rb.linearVelocity.y, newHorizontalVelocity.z);
        
        if (isGrounded && onSlope && _rb.linearVelocity.y <= 0.5f)
            _rb.AddForce(Vector3.down * _slopeStickForce, ForceMode.Acceleration);
    }
    
    public void AddBonusSpeed(float amount) => _currentBonusSpeed += amount;
    
    public void DecayBonusSpeed()
    {
        _currentBonusSpeed = Mathf.MoveTowards(_currentBonusSpeed, 0f, _bunnyHopDecay * Time.fixedDeltaTime);
    }
}