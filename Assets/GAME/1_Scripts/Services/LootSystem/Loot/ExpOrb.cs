
public class ExpOrb : Collectable, ILootOrb
{
    private int expAmount;
    public void Initialize(int expAmount)
    {
        this.expAmount = expAmount;
    }
    protected override void Collect()
    {
        if (targetPlayer == null) return;

        LevelManager levelManager = targetPlayer.GetComponent<LevelManager>();
        levelManager?.AddExp(expAmount);

        G.PoolManager?.Return(PoolId.ExpOrb, gameObject);
    }
}