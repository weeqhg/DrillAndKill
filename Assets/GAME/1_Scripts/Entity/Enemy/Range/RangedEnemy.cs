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
    private float accuracy = 0.85f;
    private Vector3 currentTarget;

    private void Start()
    {
        agent.updateRotation = false;
    }

    protected override void EnemyMove()
    {
        if (player == null) return;
        if (IsStoped) return;

        if (distance > attackRange) // подход к игроку
        {
            currentTarget = posPlayer;

            Vector3 target = currentTarget;
            target.y = transform.position.y;

            transform.LookAt(target);
        }
        else if (distance < retreatRange) // отступ
        {
            Vector3 retreatDir = (transform.position - posPlayer).normalized;
            currentTarget = transform.position + retreatDir * retreatRange;

            Vector3 target = currentTarget;
            target.y = transform.position.y;

            transform.LookAt(target);
        }
        else
        {
            // В зоне атаки - стоим
            currentTarget = transform.position;
        }

        if (NavMesh.SamplePosition(currentTarget, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            currentTarget = navHit.position;

        SetDestinationSmart(currentTarget);


        if (distance >= retreatRange && distance <= attackRange)
        {
            Vector3 lookDirection = (posPlayer - transform.position).normalized;
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
        if (shootPoint == null || player == null) return;

        Vector3 lookDirection = (posPlayer - transform.position).normalized;
        lookDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        animator?.SetTrigger("Attack");

        Vector3 landingPoint = GetLandingPoint();

        GameObject projectile = G.PoolManager?.Get(PoolId.Projectile, shootPoint.position);

        projectile.GetComponent<Projectile>().Init(G.PoolManager, landingPoint, projectileSpeed, arcHeight, damage, explosionRadius);

    }


    private Vector3 GetLandingPoint()
    {
        if (player == null) return Vector3.zero;

        Vector3 playerVel = player.Velocity;

        Vector3 from = shootPoint.position;

        Vector3 predicted = posPlayer;
        predicted = Vector3.Lerp(posPlayer, predicted, accuracy);

        for (int i = 0; i < 3; i++)
        {
            float distance = Vector3.Distance(from, predicted);
            float time = distance / projectileSpeed * 1.1f;

            predicted = posPlayer + playerVel * time;
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
        if (player == null || shootPoint == null) return false;

        Vector3 direction = posPlayer + Vector3.up * 1f - shootPoint.position; // цель на уровне головы
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