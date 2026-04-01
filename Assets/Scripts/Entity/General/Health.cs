using DG.Tweening;
using UnityEngine;


public class Health : MonoBehaviour, IDamageable
{
    public GameObject deathParticles;
    private float maxHealth;
    private float currentHealth;
    private Animator animator;
    private EventSFX characterEventSFX;
    private StatsController stats;
    private HealthUI healthUI;
    private ExpDropper expDropper;

    public void Initialize()
    {
        animator = GetComponent<Animator>();
        characterEventSFX = GetComponent<EventSFX>();
        expDropper = GetComponentInChildren<ExpDropper>();
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
        currentHealth -= damage;
        healthUI?.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Receive");
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    private void Die()
    {
        GetComponent<Collider>().enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        characterEventSFX?.PlayDieSound();
        Instantiate(deathParticles, transform.position + new Vector3(0, 2f, 0), Quaternion.identity);

        transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        if (stats != null) stats.OnStatsChanged -= UpdateStats;
        if (expDropper != null) expDropper.DropExp();
    }
}