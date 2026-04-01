using UnityEngine;

public class RangedEnemy : EnemyAI
{
    [Header("Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 8f;
    private float retreatRange = 30f;
    private float arcHeight = 8f;
    private float explosionRadius = 3f;
    private PoolManager poolManager;
    private float accuracy = 0.85f;
    public override void Initialize()
    {
        base.Initialize();

        poolManager = PoolManager.Instance;
    }

    protected override void EnemyMove(float distance)
    {
        if (distance > attackShootRange)
        {
            agent.isStopped = false;
            agent.SetDestination(enemyManager.player.position);
            animator?.SetBool("IsMoving", true);
        }
        else if (distance < retreatRange)
        {
            agent.isStopped = false;
            Vector3 retreatDir = (transform.position - enemyManager.player.position).normalized;
            Vector3 retreatPos = transform.position + retreatDir * retreatRange;
            agent.SetDestination(retreatPos);
            animator?.SetBool("IsMoving", true);
        }
        else
        {
            agent.isStopped = true;
            animator?.SetBool("IsMoving", false);

            if (CanAttack())
            {
                EnemyAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    protected override void EnemyAttack()
    {
        if (shootPoint == null || enemyManager.player == null) return;

        Vector3 lookDirection = (enemyManager.player.position - transform.position).normalized;
        lookDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        animator?.SetTrigger("Attack");

        Vector3 landingPoint = GetLandingPoint();

        GameObject projectile = poolManager.Get(PoolId.Projectile, shootPoint.position);

        projectile.GetComponent<Projectile>()
    .Init(poolManager, landingPoint, projectileSpeed, arcHeight, damage, explosionRadius);
    }


    private Vector3 GetLandingPoint()
    {
        if (enemyManager.player == null) return Vector3.zero;

        Vector3 playerPos = enemyManager.player.position;
        Vector3 playerVel = enemyManager.player.GetComponent<PlayerMovement>()?.Rb.linearVelocity ?? Vector3.zero;

        Vector3 from = shootPoint.position;

        Vector3 predicted = playerPos;
        predicted = Vector3.Lerp(playerPos, predicted, accuracy);

        for (int i = 0; i < 3; i++)
        {
            float distance = Vector3.Distance(from, predicted);
            float time = distance / projectileSpeed * 1.1f;

            predicted = playerPos + playerVel * time;
        }

        predicted.x += Random.Range(-1.5f, 1.5f);
        predicted.z += Random.Range(-1.5f, 1.5f);

        if (Physics.Raycast(predicted + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            predicted = hit.point;

        return predicted;
    }

}