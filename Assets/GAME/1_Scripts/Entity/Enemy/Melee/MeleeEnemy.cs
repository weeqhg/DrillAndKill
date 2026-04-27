using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : EnemyAI
{
    private float nextDecisionTime;
    private Vector3 currentTarget;

    protected override void EnemyMove()
    {
        if (player == null) return;
        if (IsStoped) return;

        // 1️⃣ Двигаемся к игроку или обходим его
        if (distance > attackRange)
        {
            // 30% вероятности: случайная позиция рядом с игроком (обход/фланг)
            if (Time.time > nextDecisionTime)
            {
                if (Random.value < 0.3f)
                    currentTarget = GetRandomNearbyPosition(3f);
                else
                    currentTarget = posPlayer;

                nextDecisionTime = Time.time + 1f;
            }

            // Привязываем точку к NavMesh, чтобы враг не застрял в стенах
            if (NavMesh.SamplePosition(currentTarget, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                currentTarget = navHit.position;

            agent.isStopped = false;
            SetDestinationSmart(currentTarget);
            animator?.SetBool("IsMoving", true);
        }
        else
        {
            // 2️⃣ В пределах досягаемости атаки: идём вокруг игрока и атакуем
            agent.isStopped = false;
            animator?.SetBool("IsMoving", true);

            // Двигаемся вокруг игрока случайным образом
            Vector3 strafePos = GetRandomNearbyPosition(2f); // небольшое движение вокруг игрока
            if (NavMesh.SamplePosition(strafePos, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                SetDestinationSmart(navHit.position);

            // 3️⃣ Атака, если готов
            if (CanAttack())
            {
                EnemyAttack();
            }
        }
    }

    protected override void EnemyAttack()
    {
        animator?.SetTrigger("Attack");
    }

    public void OnAttackTrigger()
    {
        if (distance <= attackRange)
        {
            var damageable = player?.Damageable;
            damageable?.TakeDamage(damage);
        }
    }
}