using UnityEngine;

public class EntityStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _attackSpeed = 1f;
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _jumpHeight = 8f;
    [SerializeField] private int _maxJump = 1;
    [SerializeField] private float _meleeRange = 2f;
    [SerializeField] private float _shootRange = 50f;

    public float MaxHealth => _maxHealth;
    public float MoveSpeed => _moveSpeed;
    public float AttackSpeed => _attackSpeed;
    public float AttackDamage => _attackDamage;
    public float JumpHeight => _jumpHeight;
    public int MaxJump => _maxJump;
    public float MeleeRange => _meleeRange;
    public float ShootRange => _shootRange;
}
