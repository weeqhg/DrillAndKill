using UnityEngine;

public class MeleeEnemy : EnemyAI
{
    public override void Initialize()
    {
        base.Initialize();

        attackRange = stats.MeleeRange;
    }

    protected override void EnemyMove(float distance)
    {
        if (distance > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(enemyManager.player.position);
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
        animator?.SetTrigger("Attack");
        var damageable = enemyManager.player?.GetComponent<IDamageable>();
        damageable?.TakeDamage(stats.AttackDamage);
    }
}