public class SlideState : PlayerState
{
    public SlideState(PlayerMovement player) : base(player) { }

    public override void Enter()
    {
        player.Animation.ToggleSlider(true);
    }

    public override void Update()
    {
        if (!player.IsSliding)
        {
            player.SetState(player.RunState);
            return;
        }

        if (!player.IsGrounded)
        {
            player.SetState(player.FallState);
            return;
        }

        if (player.IsJump)
        {
            player.SetState(player.JumpState);
            return;
        }
    }

    public override void Exit()
    {
        player.Animation.ToggleSlider(false);
        player.Slide.StopSlideAudio();
    }

    public override void FixedUpdate()
    {
        player.Slide.UpdateState(player.IsGrounded, player.Rb.linearVelocity);
        player.Slide.HandleMovement(player.MoveInput, player.IsGrounded);
        player.Movement.HandleRotation(player.MoveInput.sqrMagnitude > 0.01f);
    }
}