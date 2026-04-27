using UnityEngine;

public class RunState : PlayerState
{
    public RunState(PlayerMovement player) : base(player) { }

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

        if (player.IsGrounded && player.MoveInput.sqrMagnitude < 0.01f)
            player.SetState(player.IdleState);
    }

    public override void FixedUpdate()
    {
        player.Movement.HandleMovement(player.MoveInput, true);
        player.Movement.HandleRotation(player.MoveInput.sqrMagnitude > 0.01f);
        player.Movement.DecayBonusSpeed();
    }
}
