public abstract class PlayerState
{
    protected PlayerMovement player;

    protected PlayerState(PlayerMovement player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }

    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
