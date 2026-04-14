using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : EnemyAI
{
    [Header("Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private LayerMask obstacleLayer;
    private float retreatRange = 20f;
    private float arcHeight = 8f;
    private float explosionRadius = 3f;
    private PoolManager poolManager;
    private float accuracy = 0.85f;
    private Vector3 currentTarget;

    public override void Initialize()
    {
        base.Initialize();

        poolManager = PoolManager.Instance;
    }

    protected override void EnemyMove()
    {
        if (enemyManager.player == null) return;
        if (enemyManager.IsStoped) return;

        distance = Vector3.Distance(transform.position, enemyManager.player.position);

        if (distance > attackShootRange) // подход к игроку
        {
            currentTarget = enemyManager.player.position;
        }
        else if (distance < retreatRange) // отступ
        {
            Vector3 retreatDir = (transform.position - enemyManager.player.position).normalized;
            currentTarget = transform.position + retreatDir * retreatRange;
        }
        else
        {
            // В зоне атаки - стоим
            currentTarget = transform.position;
        }

        // NavMesh проверка
        if (NavMesh.SamplePosition(currentTarget, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            currentTarget = navHit.position;

        agent.SetDestination(currentTarget);


        if (distance >= retreatRange && distance <= attackShootRange)
        {
            Vector3 lookDirection = (enemyManager.player.position - transform.position).normalized;
            lookDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            if (CanAttack() && CanSeePlayer())
            {
                EnemyAttack();
            }
        }

        agent.isStopped = false;
        animator?.SetBool("IsMoving", true);
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

    /// <summary>
    /// Проверка видимости игрока через стены
    /// </summary>
    private bool CanSeePlayer()
    {
        if (enemyManager.player == null || shootPoint == null) return false;

        Vector3 direction = (enemyManager.player.position + Vector3.up * 1f) - shootPoint.position; // цель на уровне головы
        float distance = direction.magnitude;
        direction.Normalize();

        // Raycast к игроку
        if (Physics.Raycast(shootPoint.position, direction, out RaycastHit hit, distance, obstacleLayer))
        {
            // Если луч попал в препятствие до игрока
            return false;
        }

        return true;
    }

}