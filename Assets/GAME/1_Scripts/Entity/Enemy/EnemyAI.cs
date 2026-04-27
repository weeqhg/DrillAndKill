using UnityEngine;
using UnityEngine.AI;


public abstract class EnemyAI : MonoBehaviour
{
    protected PlayerManager player;
    protected bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected StatsController stats;
    protected float distance;
    protected float attackRange;
    protected float attackRate;
    protected float damage;
    protected float moveSpeed;
    private float lastAttackTime;
    private bool lastStopState;
    protected Vector3 posPlayer => PlayerService.Player != null ? player.Transform.position : Vector3.zero;
    private float farDistance = 200f;
    private float nextPathUpdate;
    private float pathUpdateRate = 0.2f;
    private float disableDistance = 1000f;
    private float fullDisableDistance = 150f;
    private SkinnedMeshRenderer[] renderers;
    private int updateFrameOffset;

    public void Initialize()
    {
        updateFrameOffset = Random.Range(0, 10);
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stats = GetComponentInChildren<StatsController>();
        stats.OnStatsChanged += UpdateStats;

        if (PlayerService.Player != null) SetPlayer(PlayerService.Player);
        PlayerService.OnPlayerChanged += SetPlayer;

        UpdateStats();
    }

    private void SetPlayer(PlayerManager player)
    {
        this.player = player;
    }

    private void UpdateStats()
    {
        attackRate = 1f / stats.GetStat(StatType.AttackRate);
        attackRange = stats.GetStat(StatType.AttackRange);
        damage = stats.GetStat(StatType.Damage);
        moveSpeed = stats.GetStat(StatType.MoveSpeed);
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (Time.frameCount % 10 != updateFrameOffset) return;

        // Проверяем, изменилось ли состояние
        if (IsStoped != lastStopState)
        {
            if (IsStoped)
            {
                if (agent.enabled) agent.isStopped = true;
                animator.enabled = false;
            }
            else
            {
                if (agent.enabled) agent.isStopped = false;
                animator.enabled = true;
            }

            lastStopState = IsStoped;
        }

        if (IsStoped) return;

        distance = Vector3.Distance(transform.position, posPlayer);

        UpdateSpeedByDistance();

        EnemyMove();

        UpdateLOD();
    }

    private void UpdateLOD()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, posPlayer);

        // 🔴 Очень далеко — выключаем всё
        if (dist > fullDisableDistance)
        {
            animator.enabled = false;

            foreach (var r in renderers)
                r.enabled = false;

            return;
        }

        // 🟡 Средняя дистанция — отключаем анимации
        if (dist > disableDistance)
        {
            animator.enabled = false;

            foreach (var r in renderers)
                r.enabled = true;

            return;
        }

        animator.enabled = true;

        foreach (var r in renderers)
            r.enabled = true;
    }

    private void UpdateSpeedByDistance()
    {
        bool isFar = distance > farDistance;

        if (isFar && player != null)
        {
            Vector3 behindPos = posPlayer - player.Transform.forward * 200f;

            if (NavMesh.SamplePosition(behindPos, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
    }

    protected abstract void EnemyMove();
    protected abstract void EnemyAttack();
    protected virtual bool CanAttack()
    {
        if (IsStoped) return false;

        float cooldown = attackRate;

        if (Time.time >= lastAttackTime + cooldown)
        {
            lastAttackTime = Time.time; // 🔥 ВАЖНО
            return true;
        }

        return false;
    }

    protected void SetDestinationSmart(Vector3 target)
    {
        if (Time.time < nextPathUpdate) return;

        agent.SetDestination(target);
        nextPathUpdate = Time.time + pathUpdateRate;
    }

    protected Vector3 GetFlankPosition(float minDistance = 2f, float maxDistance = 5f)
    {
        Vector3 direction = (transform.position - posPlayer).normalized;

        // Выбираем случайный угол обхода (влево или вправо)
        float angle = Random.Range(-90f, 90f);
        Quaternion rot = Quaternion.Euler(0, angle, 0);

        Vector3 flankDir = rot * direction;

        float distance = Random.Range(minDistance, maxDistance);
        Vector3 targetPos = posPlayer + flankDir * distance;

        // Привязка к NavMesh
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return hit.position;

        return transform.position; // если не нашли, остаёмся на месте
    }

    protected Vector3 GetRandomNearbyPosition(float radius = 3f)
    {
        Vector3 randomOffset = Random.insideUnitSphere * radius;
        randomOffset.y = 0;
        Vector3 targetPos = posPlayer + randomOffset;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    protected Vector3 GetBehindPlayer(float distance = 2f)
    {
        Vector3 playerForward = player.Transform.forward;
        Vector3 behindPos = posPlayer - playerForward * distance;

        if (NavMesh.SamplePosition(behindPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }


    private void OnDestroy()
    {
        PlayerService.OnPlayerChanged -= SetPlayer;
        stats.OnStatsChanged -= UpdateStats;
    }
}