using System;
using System.Linq.Expressions;
using UnityEngine;

public class JumpController
{
    private SoundData windData;
    private SoundData landData;
    private SoundHandle wind;

    private readonly PlayerMovement _player;
    private readonly Rigidbody _rb;

    private readonly float _jumpRiseGravity;
    private readonly float _jumpFallGravity;

    private float _lastLandingTime;
    private int _jumpsRemaining;
    private bool _jumpQueued;

    // Bunny hop
    private const float BunnyHopWindow = 0.1f;
    private const float BunnyHopSpeedBonus = 6f;



    public JumpController(PlayerMovement player, float riseGravity, float fallGravity)
    {
        windData = Resources.Load<SoundData>("Audio/SFX/Wind");
        landData = Resources.Load<SoundData>("Audio/SFX/LandSmallObject");

        _player = player;
        _rb = player.Rb;

        _jumpRiseGravity = riseGravity;
        _jumpFallGravity = fallGravity;

        _jumpsRemaining = _player.MaxJump;
    }

    public void QueueJump() => _jumpQueued = true;
    public void ClearJumpQueued() => _jumpQueued = false;

    public void HandleJump()
    {
        UpdateLandingState(_player.IsGrounded);

        if (_jumpQueued && CanJump())
        {
            PerformJump();
        }
    }

    private bool CanJump()
    {
        if (_player.IsGrounded && _jumpsRemaining > 0) return true;
        if (!_player.IsGrounded && _jumpsRemaining >= 1) return true;
        return false;
    }

    private void PerformJump()
    {
        _jumpsRemaining--;

        _player.Animation.TriggerJump();

        //ApplyBunnyHop();

        ResetVerticalVelocity();

        float jumpForce = Mathf.Sqrt(_player.JumpHeight * 1000 * -2f * Physics.gravity.y);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ApplyBunnyHop()
    {
        float timeSinceLand = Time.time - _lastLandingTime;

        if (timeSinceLand > BunnyHopWindow) return;

        float speed = _rb.linearVelocity.magnitude;
        float maxSpeed = 12f;

        float factor = Mathf.Clamp01(1f - (speed / maxSpeed));
        Debug.Log(factor);
        _player.AddBonusSpeed(BunnyHopSpeedBonus * factor);
    }

    private void ResetVerticalVelocity()
    {
        Vector3 velocity = _rb.linearVelocity;
        velocity.y = 0;
        _rb.linearVelocity = velocity;
    }

    private void UpdateLandingState(bool isGrounded)
    {
        if (isGrounded)
        {
            _lastLandingTime = Time.time;
            _jumpsRemaining = _player.MaxJump;
        }
    }

    public void HandleGravity()
    {
        Vector3 velocity = _rb.linearVelocity;

        if (velocity.y > 0)
        {
            velocity.y += Physics.gravity.y * (_jumpRiseGravity - 1f) * Time.fixedDeltaTime;
        }
        else
        {
            velocity.y += Physics.gravity.y * (_jumpFallGravity - 1f) * Time.fixedDeltaTime;
        }

        _rb.linearVelocity = velocity;
    }

    public void FallImpact(bool isGrounded)
    {
        float verticalSpeed = _rb.linearVelocity.y;
        bool shouldPlayWind = !isGrounded && verticalSpeed < -9f;

        if (shouldPlayWind && wind == null)
        {
            wind = G.AudioManager?.Play(windData);
        }
    }

    public void LandImpact()
    {
        Vector3 velocity = _rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        G.AudioManager?.Stop(wind);
        wind = null;
        G.AudioManager?.Play(landData);
        Vector3 offset = horizontalVel * 0.15f;
        Vector3 spawnPos = GetGroundPoint() + offset;
        G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Land, spawnPos, 0.5f);
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