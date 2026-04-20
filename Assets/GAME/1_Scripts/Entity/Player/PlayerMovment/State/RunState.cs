using UnityEngine;

public class RunState : PlayerState
{
    public RunState(PlayerMovement player) : base(player) { }

    public override void Update()
    {
        if (player.MoveInput.sqrMagnitude < 0.01f)
            player.SetState(player.IdleState);

        if (!player.IsGrounded)
            player.SetState(player.JumpState);
    }

    public override void FixedUpdate()
    {
        player.Movement.HandleMovement(player.MoveInput, true);
        player.Movement.HandleRotation(true);
    }
}
