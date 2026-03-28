using UnityEngine;
using UnityEngine.AI;


public abstract class EnemyAI : MonoBehaviour
{
    protected EnemyManager enemyManager;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected EntityStats stats;
    protected float lastAttackTime;
    protected float attackRange;
    private bool _lastStopState;

    public virtual void Initialize()
    {
        enemyManager = GetComponent<EnemyManager>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();
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

        return Time.time >= lastAttackTime + stats.AttackSpeed;
    }


}