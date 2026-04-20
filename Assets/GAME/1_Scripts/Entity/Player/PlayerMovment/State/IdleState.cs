using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerMovement player) : base(player) { }

    public override void Update()
    {
        if (player.MoveInput.sqrMagnitude > 0.01f)
            player.SetState(player.RunState);

        if (!player.IsGrounded)
            player.SetState(player.JumpState);
    }

    public override void FixedUpdate()
    {
        player.Rb.linearVelocity = new Vector3(0, player.Rb.linearVelocity.y, 0);
    }
}