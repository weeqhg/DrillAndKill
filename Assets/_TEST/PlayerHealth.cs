using System.Collections;
using UnityEngine;

public class PlayerHealth : Health
{
    private HealthUI healthUI;
    private float _lastDisplayedHealth;
    private float temporaryShield;
    private Coroutine _shieldDecayCoroutine;
    private bool isImmortality;
    private float regeneration;
    private Coroutine _regenerationCoroutine;
    private bool isMossActive = false;
    private float rateMoss = 0f;



    public override void SetDerrived()
    {
        maxHealth = stats.GetStat(StatType.MaxHealth);
        regeneration = stats.GetStat(StatType.Regeneration);

        currentHealth = maxHealth;
        
        healthUI = GetComponentInChildren<HealthUI>();
        healthUI?.Initialize(maxHealth);
    }

    public override void UpdateStats()
    {
        maxHealth = stats.GetStat(StatType.MaxHealth);
        regeneration = stats.GetStat(StatType.Regeneration);

        if (currentHealth > maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        healthUI?.UpdateHealth(currentHealth, maxHealth);

        if (regeneration > 0) RestartRegeneration();
    }

    public void ToggleImmortal(bool enable)
    {
        isImmortality = enable;
    }

    public void SetRate(float time)
    {
        rateMoss = time;
    }

    public void ToggleMoss(bool enable)
    {
        isMossActive = enable;
    }

    public override void TakeDamage(float damage)
    {
        PlayerService.DamageTaken(damage);

        if (isImmortality)
        {
            animator?.SetTrigger("Receive");
            return;
        }

        if (temporaryShield > 0)
        {
            float absorbed = Mathf.Min(temporaryShield, damage);
            temporaryShield -= absorbed;
            damage -= absorbed;
        }

        if (damage <= 0)
        {
            UpdateHealthUI();
            return;
        }

        currentHealth -= damage;
        UpdateHealthUI();

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
        float overflow = 0f;

        float valueHealth = currentHealth + amount;

        if (valueHealth > maxHealth)
        {
            overflow = valueHealth - maxHealth;
            amount = maxHealth - currentHealth;
        }

        currentHealth += amount;

        if (overflow > 0 && isMossActive)
            AddTemporaryShield(overflow);

        UpdateHealthUI();
    }

    public override void Die()
    {
        base.Die();

        PlayerService.Kill();
    }

    private void AddTemporaryShield(float amount)
    {
        temporaryShield += amount;

        if (_shieldDecayCoroutine == null)
            _shieldDecayCoroutine = StartCoroutine(ShieldDecayRoutine());

        UpdateHealthUI();
    }

    private IEnumerator ShieldDecayRoutine()
    {
        while (temporaryShield > 0)
        {
            yield return new WaitForSeconds(rateMoss);

            temporaryShield = Mathf.Max(0, temporaryShield - 1f);

            UpdateHealthUI();

            if (temporaryShield <= 0)
            {
                _shieldDecayCoroutine = null;
                yield break;
            }
        }
    }

    private void UpdateHealthUI()
    {
        int currentInt = Mathf.FloorToInt(currentHealth);
        int lastInt = Mathf.FloorToInt(_lastDisplayedHealth);

        if (currentInt != lastInt)
        {
            healthUI?.UpdateHealth(currentHealth, maxHealth);
            _lastDisplayedHealth = currentHealth;
        }
    }

    private void RestartRegeneration()
    {
        if (_regenerationCoroutine != null)
            StopCoroutine(_regenerationCoroutine);

        if (regeneration > 0) _regenerationCoroutine = StartCoroutine(RegenerationRoutine());
    }

    private IEnumerator RegenerationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Регенерация каждую секунду
            if (currentHealth >= maxHealth) continue;

            float healAmount = regeneration;

            // Не превышаем максимальное здоровье
            if (currentHealth + healAmount > maxHealth)
                healAmount = maxHealth - currentHealth;

            Heal(healAmount);
        }
    }

    private void OnDestroy()
    {
        if (_regenerationCoroutine != null) StopCoroutine(_regenerationCoroutine);
    }
}
