using UnityEngine;

public class BaseStats : MonoBehaviour
{
    [Header("Base Stats")]
    [Header("Выживание")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _regenerationHealth;
    [SerializeField] private float _armor;
    [SerializeField] private float _evasion;

    [Header("Атака")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _сritСhance;
    [SerializeField] private float _сritMultiplayer = 1f;
    [SerializeField] private float _attackRate = 1f;
    [SerializeField] private float _attackRange = 2f;

    [Header("Мобильность")]
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private int _maxJump = 1;

    [Header("Утилиты")]
    [SerializeField] private float _luck = 0f;
    [SerializeField] private float _pickupRadius = 5;

    [Header("Специальные")]
    [SerializeField] private float _shield;
    [SerializeField] private float _thorns;
    [SerializeField] private float _stealingLife;


    public float GetStat(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth: return _maxHealth;
            case StatType.Regeneration: return _regenerationHealth;
            case StatType.Shield: return _shield;
            case StatType.Armor: return _armor;
            case StatType.Evasion: return _evasion;
            case StatType.StealingLife: return _stealingLife;
            case StatType.Thorns: return _thorns;
            case StatType.Damage: return _damage;
            case StatType.CritСhance: return _сritСhance;
            case StatType.CritMultiplayer: return _сritMultiplayer;
            case StatType.AttackRate: return _attackRate;
            case StatType.MoveSpeed: return _moveSpeed;
            case StatType.MaxJump: return _maxJump;
            case StatType.Luck: return _luck;
            case StatType.AttackRange: return _attackRange;
            case StatType.PickupRadius: return _pickupRadius;
            default: return 0f;
        }
    }
}
