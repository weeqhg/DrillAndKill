using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [SerializeField] private GameObject itemPickupPrefab;
    [SerializeField] private int orbCount = 3;
    [SerializeField] private int coinCount = 5;
    [SerializeField] private Transform spawnPoint;

    private int exp;
    private int coin;

    public void SetReward(int expAmount, int coinAmount)
    {
        exp = expAmount;
        coin = coinAmount;
    }

    public void DropLootItem(ItemData item)
    {
        Vector3 spawnPos = spawnPoint.position + Vector3.up * 2f;

        GameObject obj = Instantiate(itemPickupPrefab, spawnPos, Quaternion.identity);

        Vector2 horizontal = Random.insideUnitCircle.normalized;

        Vector3 dir = new Vector3(
            horizontal.x,
            Random.Range(0.7f, 1f),
            horizontal.y
        );

        float force = Random.Range(2f, 4f);

        if (obj.TryGetComponent<ItemPickup>(out var pickup))
        {
            pickup.Initialize(item);
            pickup.Launch(dir, force);
        }
    }
    public void DropLootEXP()
    {
        DropOrbs(
        PoolId.ExpOrb,
        exp,
        orbCount,
        3f,
        1f,
        3f);

    }

    public void DropLootCOIN()
    {
        DropOrbs(
        PoolId.CoinOrb,
        coin,
        coinCount,
        2f,
        1.5f,
        3.5f);
    }

    private void DropOrbs(PoolId poolId, int totalAmount, int count, float yOffset, float minForce, float maxForce)
    {
        int amountPerOrb = Mathf.Max(1, totalAmount / count);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = spawnPoint.position + Vector3.up * yOffset;

            GameObject obj = G.PoolManager?.Get(poolId, spawnPos);

            if (obj == null) continue;

            Vector2 horizontal = Random.insideUnitCircle.normalized;

            Vector3 dir = new Vector3(
                horizontal.x,
                Random.Range(0.6f, 1f),
                horizontal.y
            );

            float force = Random.Range(minForce, maxForce);

            if (obj.TryGetComponent<ILootOrb>(out var orb))
            {
                orb.Initialize(amountPerOrb);
                orb.Launch(dir, force);
            }
        }
    }

    private void OnDestroy()
    {
        DropLootEXP();
        DropLootCOIN();
    }
}