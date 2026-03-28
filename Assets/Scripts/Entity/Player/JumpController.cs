using UnityEngine;

public class JumpController
{
    private PlayerMovement _player;
    private Rigidbody _rb;

    private float _jumpRiseGravity;
    private float _jumpFallGravity;
    private float _bunnyHopWindow = 0.3f;
    private float _bunnyHopSpeedBonus = 6f;

    private int _jumpsRemaining;
    private float _lastLandingTime;
    private bool _isJumping;
    private bool _jumpQueued;

    public JumpController(PlayerMovement player, float riseGravity, float fallGravity)
    {
        _player = player;
        _rb = player.Rb;
        _jumpRiseGravity = riseGravity;
        _jumpFallGravity = fallGravity;
    }

    public void Initialize()
    {
        _jumpsRemaining = _player.Stats.MaxJump;
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
        if (!isGrounded && _jumpsRemaining >= 1) return true;
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

        float jumpForce = Mathf.Sqrt(_player.Stats.JumpHeight * 1000 * -2f * Physics.gravity.y);
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
        if (isGrounded) _jumpsRemaining = _player.Stats.MaxJump;
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
}