using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;


public class FlyEnemy : EnemyAI
{
    private enum State
    {
        Follow,
        PrepareDash
    }
    [SerializeField] private Transform body;
    private float hoverHeight = 5f;
    private float hoverSpeed = 2f;
    private float hoverTime;
    private State currentState;
    private bool prepareStarted;

    private float dashSpeedMultiplier = 3f;
    private float dashDuration = 3f;
    private bool hasHit;
    private Tween rotateTween;
    private Sequence prepareSequence;
    private Vector3 baseLocalPos;
    private Vector3 dashDirection;
    private Coroutine dashCoroutine;




    private void Start()
    {
        baseLocalPos = body.localPosition;
        agent.updateRotation = false;
    }

    protected override void EnemyMove()
    {
        if (player == null) return;

        switch (currentState)
        {
            case State.Follow:
                FollowPlayer();
                break;

            case State.PrepareDash:
                PrepareDash();
                break;
        }

        Hover();
    }

    protected override void EnemyAttack()
    {
        // не нужен — атака встроена в Dash
    }

    private void Hover()
    {
        hoverTime += Time.deltaTime;

        float offset = Mathf.Sin(hoverTime * hoverSpeed) * hoverHeight;
        float noise = Mathf.PerlinNoise(Time.time, 0f) * 0.1f;

        Vector3 pos = baseLocalPos;
        pos.y += offset + noise;

        body.localPosition = pos;
    }

    // =========================
    // STATES
    // =========================

    private void FollowPlayer()
    {
        agent.speed = moveSpeed;

        body.LookAt(posPlayer);
        if (agent.enabled) SetDestinationSmart(posPlayer);

        // если близко → начинаем атаку
        if (distance < attackRange * 2f && CanAttack())
        {
            currentState = State.PrepareDash;
            prepareStarted = false;
            agent.isStopped = true;
        }
    }

    private void PrepareDash()
    {
        if (!prepareStarted)
        {
            prepareStarted = true;

            prepareSequence?.Kill();

            Vector3 dir = (posPlayer - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir);

            prepareSequence = DOTween.Sequence();

            prepareSequence.Append(
                body.DOLocalRotate(new Vector3(90f, 0f, 0f), 1f)
                    .SetEase(Ease.OutBack)
            );

            prepareSequence.AppendInterval(0.1f);

            prepareSequence.Append(
                body.DOLocalRotate(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack)
            );

            prepareSequence.OnComplete(() =>
            {
                StartDash();
            });
        }

    }

    private void StartDash()
    {
        hasHit = false;

        Vector3 target = posPlayer + Vector3.up * 1f;
        dashDirection = (target - transform.position).normalized;

        // отключаем NavMesh
        agent.ResetPath();
        agent.isStopped = true;
        agent.enabled = false;

        // 🔥 вращение (если надо)
        rotateTween?.Kill();

        rotateTween = body.DOLocalRotate(new Vector3(0f, 360f, 0), 0.3f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);

        // 🔥 запускаем корутину
        dashCoroutine = StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        float elapsed = 0f;
        float speed = moveSpeed * dashSpeedMultiplier;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            transform.position += dashDirection * speed * Time.deltaTime;

            // hit
            if (!hasHit && distance < 5f)
            {
                hasHit = true;
                player?.Damageable?.TakeDamage(damage);
            }

            yield return null;
        }

        EndDash();
        currentState = State.Follow;
    }

    private void EndDash()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }

        // остановка вращения
        rotateTween?.Kill();
        body.DOKill(); // на всякий
        body.localRotation = Quaternion.identity;

        agent.enabled = true;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        hasHit = false;
    }

    private void OnDestroy()
    {
        prepareSequence?.Kill();
        rotateTween?.Kill();
    }
}