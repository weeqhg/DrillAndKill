using UnityEngine;

public class FallState : PlayerState
{
    public FallState(PlayerMovement player) : base(player) { }
    public override void Update()
    {
        if (player.IsJump)
        {
            player.SetState(player.JumpState);
            return;
        }
    }
    public override void FixedUpdate()
    {
        player.Jump.HandleGravity();
        player.Jump.FallImpact(player.IsGrounded);

        player.Movement.HandleMovement(player.MoveInput, false);
        player.Movement.HandleRotation(player.MoveInput.sqrMagnitude > 0.01f);

        if (player.IsGrounded)
        {
            player.SetState(player.LandState);
        }
    }
}
