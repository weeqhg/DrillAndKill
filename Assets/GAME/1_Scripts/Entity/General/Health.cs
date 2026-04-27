using DG.Tweening;
using UnityEngine;


public abstract class Health : MonoBehaviour, IDamageable
{
    public GameObject deathParticles;
    protected float currentHealth;
    [SerializeField] protected float maxHealth;
    protected Animator animator;
    protected StatsController stats;
    private SoundData dieClip;



    public void Initialize()
    {
        dieClip = Resources.Load<SoundData>("Audio/SFX/Die");
        animator = GetComponent<Animator>();
        stats = GetComponentInChildren<StatsController>();
        stats.OnStatsChanged += UpdateStats;

        SetDerrived();
    }

    public abstract void SetDerrived();

    public abstract void UpdateStats();

    public abstract void TakeDamage(float damage);

    public abstract void Heal(float amount);

    public void Kill()
    {
        Die();
    }

    public virtual void Die()
    {
        GetComponentInChildren<Collider>().enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        G.AudioManager?.Play(dieClip);
        Instantiate(deathParticles, transform.position + new Vector3(0, 2f, 0), Quaternion.identity);

        transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        if (stats != null) stats.OnStatsChanged -= UpdateStats;
    }
}