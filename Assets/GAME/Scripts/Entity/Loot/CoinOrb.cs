

public class CoinOrb : Collectable, ILootOrb
{
    private int cointAmount;
    public void Initialize(int cointAmount)
    {
        this.cointAmount = cointAmount;
    }
    protected override void Collect()
    {
        if (targetPlayer == null) return;

        MoneyManager moneyManager = targetPlayer.GetComponent<MoneyManager>();
        moneyManager?.AddCoin(cointAmount);

        G.PoolManager?.Return(PoolId.CoinOrb, gameObject);
    }
}