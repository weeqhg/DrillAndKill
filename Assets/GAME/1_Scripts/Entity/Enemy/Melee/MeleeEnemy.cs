using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : EnemyAI
{
    protected override void EnemyMove()
    {
        if (player == null) return;
        if (IsStoped) return;

        distance = Vector3.Distance(transform.position, posPlayer);

        // 1️⃣ Двигаемся к игроку или обходим его
        if (distance > attackRange)
        {
            Vector3 target;

            // 30% вероятности: случайная позиция рядом с игроком (обход/фланг)
            if (Random.value < 0.3f)
                target = GetRandomNearbyPosition(3f); // радиус обхода
            else
                target = posPlayer;

            // Привязываем точку к NavMesh, чтобы враг не застрял в стенах
            if (NavMesh.SamplePosition(target, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                target = navHit.position;

            agent.isStopped = false;
            agent.SetDestination(target);
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
                agent.SetDestination(navHit.position);

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