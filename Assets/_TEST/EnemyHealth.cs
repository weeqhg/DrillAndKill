using UnityEngine;

public class EnemyHealth : Health
{

    public override void SetDerrived()
    {
        UpdateStats();
        currentHealth = maxHealth;
    }

    public override void UpdateStats()
    {
        maxHealth = stats.GetStat(StatType.MaxHealth);

        if (currentHealth > maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }

    public override void TakeDamage(float damage)
    {
        PlayerService.DamageDelta(damage);

        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator?.SetTrigger("Receive");
        }
    }

    public override void Heal(float amount)
    {
        float valueHealth = currentHealth + amount;

        if (valueHealth > maxHealth)
        {
            amount = maxHealth - currentHealth;
        }

        currentHealth += amount;
    }
}
