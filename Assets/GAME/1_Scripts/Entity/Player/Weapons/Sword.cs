using System.Collections;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [Header("Shoot Points")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private TrailRenderer swordTrail;
    private EventSFX sfx;
    private float attackRadius = 0f;
    private bool hasHit;

    private bool isAttackingInput;
    private CameraShake cameraShake;
    private AimAnimation aimAnimation;
    private StatsController stats;
    private float _nextAttackTime;
    private int attackIndex;
    private bool isAttacking;
    private float attackRate;
    private float damage;
    private float chancheCrit;
    private float critMultiplayer;
    private SoundData swordData;
    public void Initialize(CameraShake cameraShake, StatsController statsController)
    {
        stats = statsController;
        stats.OnStatsChanged += UpdateStats;
        UpdateStats();

        sfx = GetComponent<EventSFX>();
        swordData = Resources.Load<SoundData>("Audio/SFX/SwordAttack");

        aimAnimation = GetComponentInChildren<AimAnimation>();
        this.cameraShake = cameraShake;

        var input = G.InputManager;
        input.Actions.Player.Shoot.started += ctx => isAttackingInput = true;
        input.Actions.Player.Shoot.canceled += ctx => isAttackingInput = false;
    }

    private void UpdateStats()
    {
        attackRate = stats.GetStat(StatType.AttackRate);
        damage = stats.GetStat(StatType.Damage);
        chancheCrit = stats.GetStat(StatType.CritСhance) / 100f;
        critMultiplayer = stats.GetStat(StatType.CritMultiplayer);
        attackRadius = stats.GetStat(StatType.AttackRange);
    }

    private void Update()
    {
        if (isAttackingInput && Time.time >= _nextAttackTime)
        {
            float cooldown = 1f / attackRate;
            _nextAttackTime = Time.time + cooldown;
            Attack();
        }
    }

    private void Attack()
    {
        if (isAttacking) return;

        attackIndex = Random.Range(0, 7);

        animator.SetFloat("AttackRate", attackRate);

        swordTrail?.Clear();

        animator.SetFloat("AttackRate", attackRate);
        animator.SetFloat("MeleeAttack", attackIndex + 1);

        StartCoroutine(Swordtrail());
        StartCoroutine(AttackFailSafe());
    }

    private IEnumerator Swordtrail()
    {
        yield return new WaitForSeconds(0.3f);
        swordTrail.emitting = true;
    }
    private IEnumerator AttackFailSafe()
    {
        float maxDuration = 1f / attackRate + 0.2f; // небольшой запас

        yield return new WaitForSeconds(maxDuration);

        if (isAttacking)
        {
            ForceStopAttack();
        }
    }

    private void ForceStopAttack()
    {
        if (animator != null)
        {
            animator.SetFloat("MeleeAttack", 0);
        }
        swordTrail.emitting = false;
        isAttacking = false;
    }

    public void OnAttackStart()
    {
        isAttacking = true;
    }

    public void OnAttackEnd()
    {
        if (isActiveAndEnabled) ForceStopAttack();
    }

    public void OnHitEvent()
    {
        if (hasHit) return;

        hasHit = true;
        PerformHit();
    }

    private void PerformHit()
    {
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayer);

        foreach (var hit in hits)
        {
            var damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                float finalDamage = CalculateHitDamage();
                damageable.TakeDamage(finalDamage);
            }
        }

        aimAnimation.PlayScaleAnimation();
        G.AudioManager?.Play(swordData);
        cameraShake?.ShakeLight(0.5f);

        hasHit = false;
    }

    private float CalculateHitDamage()
    {
        bool isCrit = Random.value < chancheCrit;

        float finalDamage = damage;

        if (isCrit)
            finalDamage *= critMultiplayer;

        return finalDamage;
    }

    private void OnDisable()
    {
        OnAttackEnd();
    }

    private void OnDestroy()
    {
        if (G.InputManager != null)
        {
            var input = G.InputManager;
            input.Actions.Player.Shoot.started -= ctx => isAttackingInput = true;
            input.Actions.Player.Shoot.canceled -= ctx => isAttackingInput = false;
        }
        if (stats != null) stats.OnStatsChanged -= UpdateStats;
    }
}
