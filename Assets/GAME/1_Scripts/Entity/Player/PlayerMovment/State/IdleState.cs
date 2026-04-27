using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerMovement player) : base(player) { }

    public override void Update()
    {
        if (player.IsFlying)
        {
            player.SetState(player.FlyState);
            return;
        }

        if (player.IsSliding)
        {
            player.SetState(player.SlideState);
            return;
        }

        if (player.IsJump)
        {
            player.SetState(player.JumpState);
            return;
        }

        if (player.IsGrounded && player.MoveInput.sqrMagnitude > 0.01f)
            player.SetState(player.RunState);
    }

    public override void FixedUpdate()
    {
        player.Rb.linearVelocity = new Vector3(0, player.Rb.linearVelocity.y, 0);
        player.Movement.HandleRotation(player.IsShoot);
    }
}