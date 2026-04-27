using UnityEngine;

public class JumpState : PlayerState
{
    private float _timer;
    public JumpState(PlayerMovement player) : base(player) { }

    public override void Enter()
    {
        _timer = 0f;
        player.Jump.QueueJump();

        player.Jump.HandleJump();
    }
    public override void Exit()
    {
        player.Jump.ClearJumpQueued();
    }

    public override void FixedUpdate()
    {
        player.Movement.HandleMovement(player.MoveInput, false);
        player.Movement.HandleRotation(player.MoveInput.sqrMagnitude > 0.01f);

        _timer += Time.fixedDeltaTime;

        if (_timer > 0.2f)
        {
            if (player.IsGrounded)
            {
                player.SetState(player.LandState);
            }
            else
            {
                player.SetState(player.FallState);
            }
        }
    }
}