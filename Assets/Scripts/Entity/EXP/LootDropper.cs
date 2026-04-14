using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [SerializeField] private int orbCount = 3;
    [SerializeField] private int coinCount = 5;
    [SerializeField] private Transform spawnPoint;

    private int exp;
    private int coin;

    public void Initialize(int expAmount, int coinAmount)
    {
        exp = expAmount;
        coin = coinAmount;
    }
    public void DropLoot()
    {
        ExpOrdb();
        DropGold();
    }

    private void DropGold()
    {
        int totalCoin = coin;
        int goldPerCoin = Mathf.Max(1, totalCoin / coinCount);

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = spawnPoint.position + Vector3.up * 2f;

            GameObject coin = PoolManager.Instance.Get(PoolId.CoinOrb, spawnPos);

            if (coin != null)
            {
                var coinComponent = coin.GetComponent<CoinOrb>();
                if (coinComponent != null)
                {
                    coinComponent.Initialize(goldPerCoin);

                    Vector2 horizontal = Random.insideUnitCircle.normalized;

                    Vector3 dir = new Vector3(
                        horizontal.x,
                        Random.Range(0.6f, 1f),
                        horizontal.y
                    );

                    float force = Random.Range(1.5f, 3.5f);

                    coinComponent.Launch(dir, force);
                }
            }
        }
    }

    private void ExpOrdb()
    {
        int totalExp = exp;
        int expPerOrb = Mathf.Max(1, totalExp / orbCount);

        for (int i = 0; i < orbCount; i++)
        {
            Vector3 spawnPos = spawnPoint.position + Vector3.up * 3f;

            GameObject orb = PoolManager.Instance.Get(PoolId.ExpOrb, spawnPos);

            if (orb != null)
            {
                var exp = orb.GetComponent<ExpOrb>();
                if (exp != null)
                {
                    exp.Initialize(expPerOrb);

                    Vector3 horizontal = Random.insideUnitCircle.normalized;

                    Vector3 dir = new Vector3(
                        horizontal.x,
                        Random.Range(0.7f, 1f),
                        horizontal.y
                    );

                    float force = Random.Range(1f, 3f);

                    exp.Launch(dir, force);
                }
            }
        }
    }

    private void OnDestroy()
    {
        DropLoot();
    }
}