public class FlyState : PlayerState
{
    public FlyState(PlayerMovement player) : base(player) { }

    public override void Enter()
    {
        player.SetFlying(true);
    }

    public override void Exit()
    {
        player.SetFlying(false);
    }

    public override void FixedUpdate()
    {
        player.Fly.HandleFlight();
        player.Movement.HandleRotation(true);
    }
}