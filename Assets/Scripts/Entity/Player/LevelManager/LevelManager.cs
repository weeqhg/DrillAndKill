using UnityEngine;
using System;

public class LevelManager : MonoBehaviour
{
    private int currentLevel = 1;
    private int totalExp = 0;
    private int expRequired = 100;
    public event Action<int> OnLevelChanged;
    public event Action<int, int> OnExpChanged;
    private LevelStats levelStats;
    private LevelUI levelUI;
    private EventSFX eventSFX;

    public void Initialize()
    {
        levelStats = GetComponentInChildren<LevelStats>();
        levelUI = GetComponentInChildren<LevelUI>();
        eventSFX = GetComponentInChildren<EventSFX>();
        GameEvents.OnCommandExp += AddExp;
        RecalculateLevel();
    }

    public void AddExp(int amount)
    {
        if (amount == 0) return;

        totalExp = Mathf.Max(0, totalExp + amount);
        if (amount > 0)
            eventSFX?.PlayExpPickup();
        RecalculateLevel();
    }

    public bool SpendExp(int amount)
    {
        amount = Mathf.Abs(amount);
        if (totalExp < amount) return false;

        totalExp -= amount;
        RecalculateLevel();
        return true;
    }

    private void RecalculateLevel()
    {
        int newLevel = 1;
        int expToNext = expRequired;
        int accumulatedExp = 0;

        while (totalExp >= accumulatedExp + expToNext)
        {
            accumulatedExp += expToNext;
            newLevel++;
            expToNext = GetExpRequiredForLevel(newLevel);
        }

        int currentLevelExp = totalExp - accumulatedExp;

        // Проверяем изменение уровня
        bool levelChanged = (newLevel != currentLevel);

        if (levelChanged)
        {
            currentLevel = newLevel;
            levelStats?.SetLevel(currentLevel);
            levelUI?.LevelChanged(currentLevel);
            OnLevelChanged?.Invoke(currentLevel);
        }

        // Exp события всегда вызываем
        levelUI?.ExpChanged(currentLevelExp, expToNext);
        OnExpChanged?.Invoke(currentLevelExp, expToNext);
    }

    private int GetExpRequiredForLevel(int level)
    {
        float baseExp = 50f;     // базовая сложность
        float linear = 25f;      // линейный рост
        float amplitude = 20f;   // сила "волны"
        float frequency = 0.5f;  // частота волны

        float wave = (Mathf.Sin(level * frequency) + 1f) * 0.5f;
        float value = baseExp + linear * level + amplitude * wave;

        return Mathf.Max(1, Mathf.RoundToInt(value));
    }

    private void OnDestroy()
    {
        GameEvents.OnCommandExp -= AddExp;
    }
}