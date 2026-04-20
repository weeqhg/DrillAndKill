using UnityEngine;

public class FallState : PlayerState
{
    public FallState(PlayerMovement player) : base(player) { }

    public override void FixedUpdate()
    {
        player.Jump.Update(player.IsGrounded, player.Rb.linearVelocity.y);
        player.Jump.HandleGravity(player.Rb);
        player.Jump.HandleAirSounds(player.IsGrounded);
        player.Jump.ClearJumpQueued();

        if (player.IsGrounded)
        {
            player.SetState(player.LandState);
        }
    }
}
