using UnityEngine;
using System.Collections.Generic;

public class LevelStats : MonoBehaviour
{
    [Header("Sine Wave Growth")]
    [SerializeField] private float sineAmplitude = 10f;    // Амплитуда волны
    [SerializeField] private float sineFrequency = 0.5f;   // Частота волны

    [Header("Growth Rates")]
    [SerializeField] private float healthPerLevel = 3f;
    [SerializeField] private float damagePerLevel = 1.5f;
    [SerializeField] private float armorPerLevel = 1f;
    [SerializeField] private float attackRatePerLevel = 0.1f;
    private int level = 1;

    public System.Action OnStatsUpdated;

    private Dictionary<StatType, float> levelGrowth = new();

    public void Initialize()
    {
        InitializeGrowth();
    }

    public void SetLevel(int level)
    {
        this.level = level;

        OnStatsUpdated?.Invoke();
    }

    private void InitializeGrowth()
    {
        levelGrowth[StatType.MaxHealth] = 0f;
        levelGrowth[StatType.Damage] = 0f;
        levelGrowth[StatType.Armor] = 0f;
        levelGrowth[StatType.Damage] = 0f;
        levelGrowth[StatType.MoveSpeed] = 0f;
        levelGrowth[StatType.AttackRate] = 0f;
    }


    public float Apply(StatType type, float baseValue)
    {
        if (level <= 1)
            return baseValue;

        float growth = GetGrowth(type);

        float linear = GetLinearGrowth(type) * growth;

        float sine = Mathf.Sin((level - 1) * sineFrequency);
        float amplitude = sineAmplitude * GetSineAmplitudeMultiplier(type);

        float decay = 1f / (1f + level * 0.1f);
        float sineBonus = sine * amplitude * 0.1f * decay;

        float result = baseValue + linear + sineBonus;

        return ApplyCaps(type, result);
    }
    private float GetGrowth(StatType type)
    {
        return type switch
        {
            StatType.AttackRate => Mathf.Sqrt(level - 1) * 0.5f,
            StatType.CritСhance => Mathf.Log(level + 1),
            StatType.Armor => Mathf.Log(level + 1),
            _ => Mathf.Sqrt(level - 1)
        };
    }
    private float ApplyCaps(StatType type, float value)
    {
        return type switch
        {
            StatType.AttackRate => Mathf.Min(value, 4f),     // максимум 4 атаки/сек
            StatType.MoveSpeed => Mathf.Min(value, 10f),
            StatType.CritСhance => Mathf.Min(value, 0.6f),   // 60% максимум
            _ => value
        };
    }

    private float GetLinearGrowth(StatType type)
    {
        return type switch
        {
            StatType.MaxHealth => healthPerLevel,
            StatType.Damage => damagePerLevel,
            StatType.Armor => armorPerLevel,
            StatType.MoveSpeed => 0.1f,
            StatType.AttackRate => attackRatePerLevel,
            _ => 0f
        };
    }

    private float GetSineAmplitudeMultiplier(StatType type)
    {
        return type switch
        {
            StatType.MaxHealth => 2f,
            StatType.Damage => 1.5f,
            StatType.Armor => 1.2f,
            StatType.CritСhance => 1f,
            StatType.MoveSpeed => 0.8f,
            _ => 0f
        };
    }
}