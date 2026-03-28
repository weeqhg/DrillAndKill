using DG.Tweening;
using UnityEngine;


public class Health : MonoBehaviour, IDamageable
{
    public GameObject deathParticles;
    private float currentHealth;
    private Animator animator;
    private EventSFX characterEventSFX;
    private EntityStats stats;

    public void Initialize()
    {
        animator = GetComponent<Animator>();
        characterEventSFX = GetComponent<EventSFX>();
        stats = GetComponent<EntityStats>();
        currentHealth = stats.MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Receive");
        }
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
}