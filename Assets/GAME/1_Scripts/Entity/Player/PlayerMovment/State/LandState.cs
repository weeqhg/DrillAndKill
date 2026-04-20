using UnityEngine;

public class LandState : PlayerState
{
    private float _timer;

    public LandState(PlayerMovement player) : base(player) { }

    public override void Enter()
    {
        _timer = 0.1f;
    }

    public override void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            if (player.MoveInput.sqrMagnitude > 0.01f)
                player.SetState(player.RunState);
            else
                player.SetState(player.IdleState);
        }
    }
}
