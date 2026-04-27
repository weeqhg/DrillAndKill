using UnityEngine;

public class AnimationController
{
    private Animator _animator;

    private int _moveXHash;
    private int _moveYHash;
    private int _moveSpeedHash;
    private int _isGroundedHash;
    private int _isSlidingHash;
    private int _jumpTriggerHash;
    private int _isFlyHash;

    private float _smoothTime = 0.1f;

    public AnimationController(Animator animator)
    {
        _animator = animator;
        _animator.enabled = true;
        if (_animator == null) return;

        _moveXHash = Animator.StringToHash("MoveX");
        _moveYHash = Animator.StringToHash("MoveY");
        _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        _isGroundedHash = Animator.StringToHash("IsGrounded");
        _isSlidingHash = Animator.StringToHash("IsSliding");
        _jumpTriggerHash = Animator.StringToHash("Jump");
        _isFlyHash = Animator.StringToHash("IsFly");
    }

    public void UpdateAnimator(Vector2 moveInput, bool isGrounded, bool isFlying)
    {
        if (_animator == null) return;

        float moveAmount = moveInput.magnitude;
        float moveSpeed = moveInput.y > 0.1f ? moveAmount * 2f : moveAmount;

        _animator.SetFloat(_moveXHash, moveInput.x, _smoothTime, Time.deltaTime);
        _animator.SetFloat(_moveYHash, moveInput.y, _smoothTime, Time.deltaTime);
        _animator.SetFloat(_moveSpeedHash, moveSpeed, _smoothTime, Time.deltaTime);
        _animator.SetBool(_isGroundedHash, isGrounded);
        _animator.SetBool(_isFlyHash, isFlying);
    }

    public void TriggerJump() => _animator?.SetTrigger(_jumpTriggerHash);
    public void ToggleSlider(bool value) => _animator.SetBool(_isSlidingHash, value);
}