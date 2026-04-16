using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class BossEnemy : EnemyAI
{
    [SerializeField] private Transform body;
    [Header("Attack Types")]
    private float downAttackCooldown = 3f;
    private float superAttackCooldown = 20f;

    [Header("Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private LayerMask obstacleLayer;
    private float retreatRange = 20f;
    private float arcHeight = 8f;
    private float explosionRadius = 3f;
    private float accuracy = 0.85f;
    private Vector3 currentTarget;
    private Vector3 lastDestination;
    private float lastSuperAttack;
    private float lastDownAttack;
    private float superAttackShootInterval = 0.2f;
    private float lastTickAttackTime;
    private Sequence currentSequence;

    private bool isAttacking = false;

    protected override void EnemyMove()
    {
        if (player == null) return;
        if (IsStoped)
        {
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
            }
            if (currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying())
            {
                currentSequence.Pause();
            }
            return;
        }
        else
        {
            if (currentSequence != null && currentSequence.IsActive() && !currentSequence.IsPlaying())
            {
                currentSequence.Play();
            }
        }

        if (isAttacking)
        {
            agent.isStopped = true;
            return;
        }

        distance = Vector3.Distance(transform.position, posPlayer);
        Vector3 newTarget = currentTarget;

        if (distance > attackShootRange) // подход к игроку
        {
            currentTarget = posPlayer;
        }
        else if (distance < retreatRange) // отступ
        {
            Vector3 retreatDir = (transform.position - posPlayer).normalized;
            currentTarget = transform.position + retreatDir * retreatRange;
        }
        else
        {
            // В зоне атаки - стоим
            currentTarget = transform.position;
        }

        // NavMesh проверка
        if (NavMesh.SamplePosition(currentTarget, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            currentTarget = navHit.position;

        if (Vector3.Distance(newTarget, lastDestination) > 0.5f)
        {
            lastDestination = newTarget;
            agent.SetDestination(newTarget);
        }


        float time = Time.time;
        // Атака (отдельно от движения)
        if (distance >= retreatRange && distance <= attackShootRange)
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
        else if (distance < retreatRange && CanAttack())
        {
            if (time - lastSuperAttack >= superAttackCooldown)
            {
                SuperAttack();
                lastSuperAttack = time;
            }
            else if (time - lastDownAttack >= downAttackCooldown)
            {
                DownAttack();
                lastDownAttack = time;
            }
        }
    }


    protected override void EnemyAttack()
    {
        if (isAttacking) return;
        if (shootPoint == null || player == null) return;


        float time = Time.time;

        // Приоритет атак: круговая → тройная → одиночная
        if (time - lastSuperAttack >= superAttackCooldown)
        {
            SuperAttack();
            lastSuperAttack = time;
        }
        else
        {
            BaseAttack();
        }
    }

    private void BaseAttack()
    {
        // 1. Определяем количество снарядов (2-5)
        int projectileCount = Random.Range(2, 6);

        Vector3 landingPoint = GetLandingPoint();

        for (int i = 0; i < projectileCount; i++)
        {
            if (i > 0)
            {
                landingPoint += new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            }

            G.PoolManager?.CallWithAutoReturn(PoolId.Indicator, landingPoint, 3f, 10f);

            GameObject projectile = G.PoolManager?.Get(PoolId.Projectile, shootPoint.position);

            projectile.GetComponent<Projectile>().Init(G.PoolManager, landingPoint, projectileSpeed, arcHeight, damage, explosionRadius);
        }
    }

    private void SuperAttack()
    {
        if (isAttacking) return;
        isAttacking = true;

        currentSequence?.Kill(false);
        DOTween.Kill(body);
        ResetBody();
        currentSequence = DOTween.Sequence();

        // 1. подготовка (прыжок/замах)
        currentSequence.Append(body.DOLocalMoveY(5f, 1f).SetEase(Ease.OutQuad));

        currentSequence.Append(body.DOLocalMoveY(-10f, 0.5f).SetEase(Ease.InQuad));

        currentSequence.AppendCallback(() =>
        {
            Damage();
            G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Land, new Vector3(body.position.x, body.position.y + 20f, body.position.z), 5f, 10f);
            G.AudioManager?.PlayAudioSFX(TypeSFX.LandObject);
            player.CameraShake.ShakeLight(10f);
        });

        currentSequence.Append(
        body.DOLocalRotate(new Vector3(0, 1440f, 0), 10f, RotateMode.FastBeyond360)
        .SetRelative()
        .SetEase(Ease.Linear)
        .OnUpdate(() =>
        {
            float time = Time.time;

            if (time - lastTickAttackTime >= superAttackShootInterval)
            {
                SuperAttackShoot();
                Damage();

                lastTickAttackTime = time;
            }
        })
        );

        // 4. восстановление
        currentSequence.Append(body.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.OutQuad));

        currentSequence.OnComplete(() =>
        {
            isAttacking = false;
        });
    }

    private void SuperAttackShoot()
    {
        if (IsStoped) return;
        if (player == null) return;

        Vector3 landingPoint = transform.position - body.up * 40f;

        for (int i = 0; i < 10; i++)
        {
            landingPoint += new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));

            if (Physics.Raycast(landingPoint + Vector3.up * 20f, Vector3.down, out RaycastHit hit, 50f, obstacleLayer))
            {
                landingPoint = hit.point;
            }

            G.PoolManager?.CallWithAutoReturn(PoolId.Indicator, landingPoint, 1f, 10f);

            GameObject projectile = G.PoolManager?.Get(PoolId.Projectile, shootPoint.position);

            projectile.GetComponent<Projectile>().Init(G.PoolManager, landingPoint, projectileSpeed, arcHeight, damage, explosionRadius);
        }
    }

    private void DownAttack()
    {
        if (isAttacking) return;
        isAttacking = true;

        currentSequence?.Kill(false);
        DOTween.Kill(body);
        ResetBody();
        currentSequence = DOTween.Sequence();

        // 1. подготовка (прыжок/замах)
        currentSequence.Append(body.DOLocalMoveY(5f, 1f).SetEase(Ease.OutQuad));

        // 3. удар в землю
        currentSequence.Append(body.DOLocalMoveY(-10f, 0.5f).SetEase(Ease.InQuad));

        currentSequence.AppendCallback(() =>
        {
            Damage();
            G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Land, new Vector3(body.position.x, body.position.y + 20f, body.position.z), 5f, 10f);
            G.AudioManager?.PlayAudioSFX(TypeSFX.LandObject);
            player.CameraShake.ShakeLight(10f);
        });

        currentSequence.AppendInterval(1f);
        // 4. восстановление
        currentSequence.Append(body.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad));

        currentSequence.OnComplete(() =>
        {
            isAttacking = false;
        });
    }


    private void Damage()
    {
        if (distance <= attackMeeleRange)
        {
            var damageable = player?.Damageable;
            damageable?.TakeDamage(damage);
        }
    }

    private void ResetBody()
    {
        body.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        body.localPosition = Vector3.zero;
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

        Vector3 direction = posPlayer + Vector3.up * 1 - shootPoint.position; // цель на уровне головы
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

    private void OnDestroy()
    {
        currentSequence?.Kill(false);
        DOTween.Kill(body);
    }
}

