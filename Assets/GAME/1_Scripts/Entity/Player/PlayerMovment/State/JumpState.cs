using UnityEngine;

public class JumpState : PlayerState
{
    public JumpState(PlayerMovement player) : base(player) { }

    public override void Enter()
    {
        player.Jump.QueueJump();
    }

    public override void FixedUpdate()
    {
        player.Jump.Update(player.IsGrounded, player.Rb.linearVelocity.y);
        player.Jump.HandleGravity(player.Rb);

        if (player.Rb.linearVelocity.y <= 0)
        {
            player.SetState(player.FallState);
        }
    }
}