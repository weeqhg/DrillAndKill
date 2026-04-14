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

    public void Initialize()
    {
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

        if (HealthType.Enemy == healthType) GameEvents.DamageDealt(damage);
        if (HealthType.Player == healthType) GameEvents.DamageTaken(damage);

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

        AudioManager.Instance.PlayAudioSFX(TypeSFX.Die);
        Instantiate(deathParticles, transform.position + new Vector3(0, 2f, 0), Quaternion.identity);
        if (HealthType.Enemy == healthType) GameEvents.EntityDie();

        transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        if (stats != null) stats.OnStatsChanged -= UpdateStats;
    }
}