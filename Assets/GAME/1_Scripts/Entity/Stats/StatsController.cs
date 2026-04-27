using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum StatType
{
    None,
    // Выживание
    MaxHealth, //Макс.Здоровье
    Regeneration, //Регенерация здоровья в секунду
    Armor, //Уменьшает получаемый урон
    Evasion, //Уворот (шанс не получить урон)

    // Атака
    Damage, //Чистый урон
    CritСhance, //фомрула critСhance / 100
    CritMultiplayer, //Множитель крита
    AttackRate, //Скорость атаки фомрула 1 / attackRate
    AttackRange, //Радиус атаки

    // Мобильность
    MoveSpeed, //Скорость передвжиения
    MaxJump, //Кол-во прыжков

    // Утилиты
    Luck, //Удача
    PickupRadius, //Радиус подбора

    // Специальные
    Shield, //Энергетический щит
    Thorns, //Шипы
    StealingLife, //Вампиризм
}

[Serializable]
public struct StatModifier
{
    public float value;
    public ModifierType type;
    public StatModifier(float value, ModifierType type)
    {
        this.value = value;
        this.type = type;
    }
}

public enum ModifierType { Flat, Increased, More }

public class StatsController : MonoBehaviour
{
    [SerializeField] private BaseStats baseStats;
    [SerializeField] private SkillTreeStats skillTreeStats;
    [SerializeField] private LevelStats levelStats;
    [SerializeField] private ItemsStats itemsStats;
    

    //сюда добавляем предметы ещё

    public event Action OnStatsChanged;

    private void OnEnable()
    {
        G.InputManager.Actions.Player.Test.performed += TestInput;

        if (levelStats != null)
            levelStats.OnStatsUpdated += HandleStatsChanged;

        if (skillTreeStats != null)
            skillTreeStats.OnStatBonusTree += HandleStatsChanged;

        if (itemsStats != null)
            itemsStats.OnStatsChanged += HandleStatsChanged;
    }

    private void OnDisable()
    {
        if (G.InputManager != null) G.InputManager.Actions.Player.Test.performed -= TestInput;

        if (levelStats != null)
            levelStats.OnStatsUpdated -= HandleStatsChanged;

        if (skillTreeStats != null)
            skillTreeStats.OnStatBonusTree -= HandleStatsChanged;

        if (itemsStats != null)
            itemsStats.OnStatsChanged -= HandleStatsChanged;
    }

    private void HandleStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    public void Initialize()
    {
        if (levelStats != null)
        {
            levelStats.Initialize();
        }
    }

    public float GetStat(StatType type)
    {
        float value = baseStats.GetStat(type);

        if (levelStats != null)
            value = levelStats.Apply(type, value);

        if (skillTreeStats != null)
            value = skillTreeStats.Apply(type, value);

        if (itemsStats != null)
            value = itemsStats.Apply(type, value);

        return value;
    }

    private void TestInput(InputAction.CallbackContext ctx)
    {
        TestStat(StatType.Regeneration);
    }
    private void TestStat(StatType type)
    {
        float value = baseStats.GetStat(type);
        Debug.Log($"BaseValue {value}");

        if (levelStats != null)
        {
            float l = levelStats.Apply(type, value);
            Debug.Log($"Level {l}");
        }

        if (skillTreeStats != null)
        {
            float s = skillTreeStats.Apply(type, value);
            Debug.Log($"Skill {s}");
        }

        if (itemsStats != null)
        {
            float i = itemsStats.Apply(type, value);
            Debug.Log($"Item {i}");
        }
    }
}