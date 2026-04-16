using UnityEngine;

public interface ICollectable
{
    bool IsMoving { get; }
    void StartMovingToPlayer(Transform player, float speed);
}

public class PlayerCollector : MonoBehaviour
{
    [SerializeField] private LayerMask collectableLayer;
    [SerializeField] private int maxItemsPerFrame = 5;
    private float moveSpeed = 10f;
    private float pickupRadius = 5f;

    private Transform playerTransform;
    private Collider[] hitBuffer = new Collider[50];
    private StatsController stats;

    public void Initialize()
    {
        playerTransform = transform;
        stats = GetComponentInChildren<StatsController>();

        stats.OnStatsChanged += UpdateStats;
        UpdateStats();
    }

    private void UpdateStats()
    {
       pickupRadius = stats.GetStat(StatType.PickupRadius);
       moveSpeed = stats.GetStat(StatType.PickingSpeed);
    }

    private void Update()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, pickupRadius, hitBuffer, collectableLayer);

        int processed = 0;
        for (int i = 0; i < hitCount && processed < maxItemsPerFrame; i++)
        {
            ICollectable item = hitBuffer[i].GetComponent<ICollectable>();
            if (item != null && !item.IsMoving)
            {
                item.StartMovingToPlayer(playerTransform, moveSpeed);
                processed++;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}