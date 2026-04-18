using DG.Tweening;
using UnityEngine;


public class Health : MonoBehaviour, IDamageable
{
    public enum HealthType { Player, Enemy, Neutral }
    public HealthType healthType;
    public GameObject deathParticles;
    private float maxHealth;
    private float currentHealth;
    private Animator animator;
    private StatsController stats;
    private HealthUI healthUI;
    private bool isImmortality;
    private SoundData dieClip;

    public void Initialize()
    {
        dieClip = Resources.Load<SoundData>("Audio/SFX/Die");

        animator = GetComponent<Animator>();
        stats = GetComponentInChildren<StatsController>();

        stats.OnStatsChanged += UpdateStats;
        UpdateStats();

        healthUI = GetComponentInChildren<HealthUI>();
        healthUI?.Initialize(maxHealth);
        currentHealth = maxHealth;
    }

    private void UpdateStats()
    {
        maxHealth = stats.GetStat(StatType.MaxHealth);
        if (currentHealth > maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
        healthUI?.UpdateHealth(currentHealth, maxHealth);
    }
    public void TakeDamage(float damage)
    {
        if (isImmortality)
        {
            animator?.SetTrigger("Receive");
            return;
        }

        if (HealthType.Enemy == healthType) PlayerService.DamageDelta(damage);
        if (HealthType.Player == healthType) PlayerService.DamageTaken(damage);

        currentHealth -= damage;
        healthUI?.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator?.SetTrigger("Receive");
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    public void ToggleImmortal(bool enbale)
    {
        isImmortality = enabled;
    }

    public void Kill()
    {
        Die();
    }
    private void Die()
    {
        GetComponentInChildren<Collider>().enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        G.AudioManager?.Play(dieClip);
        Instantiate(deathParticles, transform.position + new Vector3(0, 2f, 0), Quaternion.identity);
        if (HealthType.Enemy == healthType) PlayerService.Kill();

        transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
    }


    private void OnDestroy()
    {
        if (stats != null) stats.OnStatsChanged -= UpdateStats;
    }
}