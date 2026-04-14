using UnityEngine;
using UnityEngine.AI;


public abstract class EnemyAI : MonoBehaviour
{
    protected EnemyManager enemyManager;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected StatsController stats;
    protected float distance;
    protected float attackShootRange;
    protected float attackMeeleRange;
    protected float attackRate;
    protected float damage;
    private float lastAttackTime;
    private bool lastStopState;

    public virtual void Initialize()
    {
        enemyManager = GetComponent<EnemyManager>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stats = GetComponentInChildren<StatsController>();
        stats.OnStatsChanged += UpdateStats;

        UpdateStats();
    }

    private void UpdateStats()
    {
        attackRate = stats.GetStat(StatType.AttackRate);
        attackShootRange = stats.GetStat(StatType.ShootRange);
        attackMeeleRange = stats.GetStat(StatType.MeleeRange);
        damage = stats.GetStat(StatType.Damage);
    }

    private void Update()
    {
        bool isStopped = enemyManager.IsStoped;

        // Проверяем, изменилось ли состояние
        if (isStopped != lastStopState)
        {
            if (isStopped)
            {
                agent.isStopped = true;
                animator.enabled = false;
            }
            else
            {
                agent.isStopped = false;
                animator.enabled = true;
            }

            lastStopState = isStopped;
        }

        if (enemyManager.player == null) return;

        distance = Vector3.Distance(transform.position, enemyManager.player.position);

        EnemyMove();
    }

    protected abstract void EnemyMove();
    protected abstract void EnemyAttack();

    protected virtual bool CanAttack()
    {
        if (enemyManager.IsStoped) return false;

        float cooldown = 1f / attackRate;

        if (Time.time >= lastAttackTime + cooldown)
        {
            lastAttackTime = Time.time; // 🔥 ВАЖНО
            return true;
        }

        return false;
    }



    protected Vector3 GetFlankPosition(float minDistance = 2f, float maxDistance = 5f)
    {
        Vector3 playerPos = enemyManager.player.position;
        Vector3 direction = (transform.position - playerPos).normalized;

        // Выбираем случайный угол обхода (влево или вправо)
        float angle = Random.Range(-90f, 90f);
        Quaternion rot = Quaternion.Euler(0, angle, 0);

        Vector3 flankDir = rot * direction;

        float distance = Random.Range(minDistance, maxDistance);
        Vector3 targetPos = playerPos + flankDir * distance;

        // Привязка к NavMesh
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return hit.position;

        return transform.position; // если не нашли, остаёмся на месте
    }

    protected Vector3 GetRandomNearbyPosition(float radius = 3f)
    {
        Vector3 randomOffset = Random.insideUnitSphere * radius;
        randomOffset.y = 0;
        Vector3 targetPos = enemyManager.player.position + randomOffset;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    protected Vector3 GetBehindPlayer(float distance = 2f)
    {
        Vector3 playerForward = enemyManager.player.forward;
        Vector3 behindPos = enemyManager.player.position - playerForward * distance;

        if (NavMesh.SamplePosition(behindPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }


    private void OnDestroy()
    {
        stats.OnStatsChanged -= UpdateStats;
    }
}