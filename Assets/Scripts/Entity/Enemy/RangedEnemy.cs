using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangedEnemy : EnemyAI
{
    [Header("Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    private float retreatRange = 30f;
    private float arcHeight = 8f;
    private float explosionRadius = 3f;
    private EnemyVFX vfx;
    private PlayerRandomSFX sfx;
    private Queue<GameObject> _projectilePool = new Queue<GameObject>();
    private Transform _poolParent;

    public float testValue;

    public override void Initialize()
    {
        base.Initialize();

        attackRange = stats.ShootRange;
        vfx = GetComponent<EnemyVFX>();
        sfx = GetComponentInChildren<PlayerRandomSFX>();
        sfx?.Initialize();

        _poolParent = new GameObject("ProjectilePool").transform;
        _poolParent.SetParent(transform);
    }

    protected override void EnemyMove(float distance)
    {
        if (distance > stats.ShootRange)
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
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab not assigned!");
            return;
        }

        Vector3 lookDirection = (enemyManager.player.position - transform.position).normalized;
        lookDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        animator?.SetTrigger("Attack");

        Vector3 landingPoint = GetLandingPoint();

        // Берём снаряд из пула
        GameObject projectile = GetProjectile();
        projectile.transform.position = shootPoint.position;
        projectile.SetActive(true);

        StartCoroutine(MoveProjectile(projectile, landingPoint));
    }

    private GameObject GetProjectile()
    {
        var proj = _projectilePool.Count > 0 ? _projectilePool.Dequeue() : Instantiate(projectilePrefab, _poolParent);

        return proj;
    }

    private Vector3 GetLandingPoint()
    {
        if (enemyManager.player == null) return Vector3.zero;

        Vector3 playerVel = enemyManager.player.GetComponent<PlayerMovement>()?.Rb.linearVelocity ?? Vector3.zero;

        // Фиксированное время предсказания (подбирается экспериментально)
        float predictionTime = testValue;

        Vector3 target = enemyManager.player.position + playerVel * predictionTime;
        target.x += Random.Range(-2.5f, 2.5f);
        target.z += Random.Range(-2.5f, 2.5f);

        if (Physics.Raycast(target + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            target = hit.point;

        return target;
    }


    private IEnumerator MoveProjectile(GameObject projectile, Vector3 target)
    {
        Vector3 startPos = projectile.transform.position;
        Vector3 targetPos = target;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / projectileSpeed;
        float elapsed = 0;

        while (elapsed < duration)
        {
            if (enemyManager.IsStoped)
            {
                yield return null;
                continue;
            }
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Движение по дуге
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // Параболическая высота
            float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
            currentPos.y += arc;

            projectile.transform.position = currentPos;

            yield return null;
        }

        projectile.transform.position = targetPos;

        // Небольшая задержка перед взрывом
        yield return new WaitForSeconds(0.1f);

        Explode(projectile.transform.position);

        sfx?.PlayRandomSound();
        vfx?.PlayImpact(projectile.transform.position);

        ReturnProjectile(projectile);
    }

    private void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        _projectilePool.Enqueue(projectile);
    }

    private void Explode(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                var damageable = hit.GetComponent<IDamageable>();
                damageable?.TakeDamage(stats.AttackDamage);
            }
        }
    }
}