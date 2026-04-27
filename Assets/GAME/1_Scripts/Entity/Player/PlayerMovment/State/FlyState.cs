public class FlyState : PlayerState
{
    public FlyState(PlayerMovement player) : base(player) { }

    public override void Update()
    {
        if (!player.IsFlying)
        {
            player.SetState(player.FallState);
        }
    }

    public override void FixedUpdate()
    {
        player.Fly.SetVerticalInput(player.IsUpPressed, player.IsDownPressed);
        player.Fly.HandleFlight();
        player.Movement.HandleMovement(player.MoveInput, true);
        player.Movement.HandleRotation(true);
    }
}