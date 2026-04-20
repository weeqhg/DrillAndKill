public class SlideState : PlayerState
{
    public SlideState(PlayerMovement player) : base(player) { }

    public override void Enter()
    {
        player.SetSliding(true);
    }

    public override void Exit()
    {
        player.SetSliding(false);
    }

    public override void Update()
    {
        if (!player.IsSliding)
            player.SetState(player.RunState);
    }

    public override void FixedUpdate()
    {
        player.Slide.HandleMovement(player.MoveInput, player.IsGrounded);
    }
}