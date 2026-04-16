using UnityEngine;

public class JumpController
{
    private PlayerMovement _player;
    private Rigidbody _rb;
    private EventSFX _eventSFX;

    private float _jumpRiseGravity;
    private float _jumpFallGravity;
    private float _bunnyHopWindow = 0.3f;
    private float _bunnyHopSpeedBonus = 6f;
    private float _lastLandingTime;
    private bool _isJumping;
    private bool _jumpQueued;
    private int _jumpsRemaining;
    public JumpController(PlayerMovement player, float riseGravity, float fallGravity)
    {
        _player = player;
        _rb = player.Rb;
        _jumpRiseGravity = riseGravity;
        _jumpFallGravity = fallGravity;
        _jumpsRemaining = _player.MaxJump;
        _eventSFX = player.GetComponent<EventSFX>();
    }

    public void QueueJump() => _jumpQueued = true;
    public void ClearJumpQueued() => _jumpQueued = false;

    public void Update(bool isGrounded, float verticalVelocity)
    {
        UpdateLandingState(isGrounded, verticalVelocity);

        if (_jumpQueued && CanJump(isGrounded))
        {
            PerformJump();
        }
    }

    private bool CanJump(bool isGrounded)
    {
        if (isGrounded && _jumpsRemaining > 0) return true;
        if (!isGrounded && _jumpsRemaining > 1) return true;
        return false;
    }

    private void PerformJump()
    {
        _jumpsRemaining--;

        if (Time.time - _lastLandingTime <= _bunnyHopWindow)
            _player.AddBonusSpeed(_bunnyHopSpeedBonus);

        _player.SetSliding(false);
        _player.IsJumping = true;
        _isJumping = true;

        Vector3 velocity = _rb.linearVelocity;
        velocity.y = 0;
        _rb.linearVelocity = velocity;

        float jumpForce = Mathf.Sqrt(_player.JumpHeight * 1000 * -2f * Physics.gravity.y);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        _player.OnJumpPerformed();
    }

    private void UpdateLandingState(bool isGrounded, float verticalVelocity)
    {
        if (isGrounded && verticalVelocity <= 0)
        {
            if (_isJumping)
            {
                _isJumping = false;
                _player.IsJumping = false;
            }
            _lastLandingTime = Time.time;
        }
        if (isGrounded) _jumpsRemaining = _player.MaxJump;
    }

    public void HandleGravity(ref Rigidbody rb)
    {
        if (!_isJumping) return;

        Vector3 velocity = rb.linearVelocity;

        if (velocity.y > 0)
        {
            velocity.y += Physics.gravity.y * (_jumpRiseGravity - 1f) * Time.fixedDeltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y += Physics.gravity.y * (_jumpFallGravity - 1f) * Time.fixedDeltaTime;
        }

        rb.linearVelocity = velocity;
    }

    private bool _windSoundActive = false;

    public void HandleAirSounds(bool isGrounded)
    {
        float verticalSpeed = _rb.linearVelocity.y;

        bool shouldPlayWind = !isGrounded && verticalSpeed < -9f;

        if (shouldPlayWind && !_windSoundActive)
        {
            _windSoundActive = true;
            _eventSFX.ToggleWindSound(true);
        }
        else if (!shouldPlayWind && _windSoundActive)
        {
            Vector3 velocity = _rb.linearVelocity;
            Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);

            _windSoundActive = false;
            _eventSFX.ToggleWindSound(false);

            Vector3 offset = horizontalVel * 0.2f; // подбирается

            Vector3 spawnPos = GetGroundPoint() + offset;

            G.PoolManager?  .CallWithAutoReturn(PoolId.Dust_Land, spawnPos, 0.5f);
        }
    }

    private Vector3 GetGroundPoint()
    {
        Vector3 origin = _player.transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2f))
        {
            return hit.point;
        }

        return _player.transform.position;
    }
}