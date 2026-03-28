using UnityEngine;

public class SlideController
{
    private PlayerMovement _player;
    private Rigidbody _rb;

    private float _slideSlopeAngle = 15f;
    private float _slideForce = 35f;
    private float _slideDeceleration = 10f;
    private float _slideControl = 0.15f;
    private float _slideEnterMinSpeed = 2.5f;

    private bool _isSliding;
    private bool _slidePressed;
    private EventSFX _eventSFX;

    public SlideController(PlayerMovement player)
    {
        _player = player;
        _rb = player.Rb;
        _eventSFX = player.GetComponent<EventSFX>();
    }

    public void SetSlideInput(bool pressed) => _slidePressed = pressed;

    public void UpdateState(bool isGrounded, Vector3 velocity)
    {
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;

        if (!_isSliding && _slidePressed && isGrounded && horizontalSpeed >= _slideEnterMinSpeed)
        {
            _isSliding = true;
            _player.SetSliding(true);
            _eventSFX.PlaySliceSound();
        }
        else if (_isSliding && (!_slidePressed || !isGrounded))
        {
            _eventSFX.StopSlideSound();      
            _player.SetSliding(false);
            _isSliding = false;
        }
        else if (_isSliding && horizontalSpeed < 1f)
        {
            _eventSFX.StopSlideSound();
        }
    }

    public void HandleMovement(Vector2 moveInput, bool isGrounded)
    {
        Vector3 velocity = _rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        Vector3 inputDirection = GetCameraRelativeDirection(moveInput);
        Vector3 controlledVelocity = Vector3.Lerp(
            horizontalVelocity,
            inputDirection * horizontalVelocity.magnitude,
            _slideControl * Time.fixedDeltaTime
        );

        Vector3 groundNormal = _player.GetGroundNormal();
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        bool onSlope = isGrounded && slopeAngle >= _slideSlopeAngle;

        if (onSlope)
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            controlledVelocity += slopeDirection * _slideForce * Time.fixedDeltaTime;
        }
        else
        {
            controlledVelocity = Vector3.MoveTowards(
                controlledVelocity, Vector3.zero, _slideDeceleration * Time.fixedDeltaTime);
        }

        _rb.linearVelocity = new Vector3(controlledVelocity.x, _rb.linearVelocity.y, controlledVelocity.z);

        if (controlledVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(controlledVelocity.normalized, Vector3.up);
            Quaternion newRotation = Quaternion.Slerp(_rb.rotation, targetRotation, 20f * Time.fixedDeltaTime);
            _rb.MoveRotation(newRotation);
        }
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
}