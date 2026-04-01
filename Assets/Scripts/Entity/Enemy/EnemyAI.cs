using UnityEngine;
using UnityEngine.AI;


public abstract class EnemyAI : MonoBehaviour
{
    protected EnemyManager enemyManager;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected StatsController stats;
    protected float lastAttackTime;
    protected float attackShootRange;
    protected float attackMeeleRange;
    protected float attackRate;
    protected float damage;
    private bool _lastStopState;

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
        if (isStopped != _lastStopState)
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

            _lastStopState = isStopped;
        }

        if (isStopped) return;

        if (enemyManager.player == null) return;

        float distance = Vector3.Distance(transform.position, enemyManager.player.position);
        EnemyMove(distance);
    }

    protected abstract void EnemyMove(float distance);
    protected abstract void EnemyAttack();

    protected virtual bool CanAttack()
    {
        if (enemyManager.IsStoped) return false;
        
        float cooldown = 1f / attackRate;
        return Time.time >= lastAttackTime + cooldown;
    }


    private void OnDestroy()
    {
        stats.OnStatsChanged -= UpdateStats;
    }
}