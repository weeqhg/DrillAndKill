using UnityEngine;

public class ExpOrb : MonoBehaviour, ICollectable
{
    [SerializeField] private float chainRadius = 5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] private int expAmount = 10;
    [SerializeField] private LayerMask groundLayer;

    private Transform targetPlayer;
    private float moveSpeed;
    private bool isMoving = false;
    private bool isGrounded = false;

    public bool IsMoving => isMoving;

    private Vector3 velocity;
    private float gravity = 10f;

    private bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;

    public void Initialize(int amount)
    {
        expAmount = amount;
        isMoving = false;
        isGrounded = false;
    }

    public void Launch(Vector3 direction, float force)
    {
        velocity = direction * force;
        isGrounded = false;
        isMoving = false;
    }

    public void StartMovingToPlayer(Transform player, float speed)
    {
        if (isMoving) return;

        targetPlayer = player;
        moveSpeed = speed;
        isMoving = true;
    }

    private void Update()
    {
        if (!isGrounded && !isMoving && !IsStoped)
        {
            
            velocity += Vector3.down * gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;


            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                transform.position = hit.point;
                isGrounded = true;
                velocity = Vector3.zero;
            }
        }

        if (isMoving && targetPlayer != null && !IsStoped)
        {
            moveSpeed += acceleration * Time.deltaTime; // 🔥 ускорение

            Vector3 targetPosition = targetPlayer.position + Vector3.up * 1.8f; // 🔥 цель выше игрока
            Vector3 direction = (targetPosition - transform.position).normalized;

            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;

            // 🔥 цепная реакция
            AttractNearbyOrbs();
        }
    }

    private void AttractNearbyOrbs()
    {
        if (Time.frameCount % 5 != 0) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, chainRadius);

        foreach (var hit in hits)
        {
            ExpOrb orb = hit.GetComponent<ExpOrb>();

            if (orb != null && !orb.isMoving)
            {
                orb.StartMovingToPlayer(targetPlayer, moveSpeed * 1.8f);
            }
        }
    }

    private void Collect()
    {
        if (targetPlayer == null) return;

        LevelManager levelManager = targetPlayer.GetComponent<LevelManager>();
        levelManager.AddExp(expAmount);

        PoolManager.Instance.Return(PoolId.ExpOrb, gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();  
        }
    }
}