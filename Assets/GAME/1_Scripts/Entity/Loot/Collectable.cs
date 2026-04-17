using UnityEngine;

public abstract class Collectable : MonoBehaviour, ICollectable
{
    [Header("Base Settings")]
    [SerializeField] protected float chainRadius = 5f;
    [SerializeField] protected float acceleration = 20f;
    [SerializeField] protected float groundCheckDistance = 0.05f;
    [SerializeField] protected LayerMask groundLayer;

    protected Transform targetPlayer;
    protected float moveSpeed;
    protected bool isMoving = false;
    protected bool isGrounded = false;

    protected Vector3 velocity;
    protected float gravity = 10f;

    protected bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;

    public bool IsMoving => isMoving;


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


    protected virtual void Update()
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
            moveSpeed += acceleration * Time.deltaTime;

            Vector3 targetPosition = targetPlayer.position + Vector3.up * 1.8f;
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;

            AttractNearbyOrbs();
        }
    }

    protected virtual void AttractNearbyOrbs()
    {
        if (Time.frameCount % 5 != 0) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, chainRadius);

        foreach (var hit in hits)
        {
            Collectable orb = hit.GetComponent<Collectable>();
            if (orb != null && !orb.isMoving)
            {
                orb.StartMovingToPlayer(targetPlayer, moveSpeed * 1.8f);
            }
        }
    }

    protected abstract void Collect();

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
}