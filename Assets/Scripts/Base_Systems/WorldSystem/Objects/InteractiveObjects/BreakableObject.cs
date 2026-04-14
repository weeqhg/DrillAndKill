using UnityEngine;

public class BreakableObject : BaseInteractable, IDamageable
{
    [SerializeField] private GameObject intactModel;  // Целая модель
    [SerializeField] private GameObject destroyedModel; // Разрушенная модель
    [SerializeField] private AudioClip breakObj;
    private Collider objectCollider;
    private LootDropper lootDropper;
    private int levelMultiplier = 1;

    protected override void SetupDerived()
    {
        levelMultiplier = Difficulty.level;
        objectCollider = GetComponent<Collider>();
        intactModel.SetActive(true);
        destroyedModel.SetActive(false);

        lootDropper = GetComponent<LootDropper>();
        CalculateReward();
    }

    private void CalculateReward()
    {
        int expReward = Random.Range(10, 50) * levelMultiplier;
        int coinReward = Random.Range(10, 50) * levelMultiplier;
        lootDropper.Initialize(expReward, coinReward);
    }

    public override void Interact(PlayerInteractor playerInteractor)
    {
        if (isUsed) return;
        isUsed = true;

        intactModel.SetActive(false);
        destroyedModel.SetActive(true);

        objectCollider.enabled = false;

        poolManager.CallWithAutoReturn(PoolId.Dust_Default, transform.position, 1f);

        AudioManager.Instance.PlayAudio3DSFX(breakObj, transform.position);

        lootDropper.DropLoot();
    }

    public void TakeDamage(float damage)
    {
        if (isUsed) return;
        isUsed = true;

        // Переключаем модели
        intactModel.SetActive(false);
        destroyedModel.SetActive(true);

        // Отключаем коллайдер (не нужно больше)
        objectCollider.enabled = false;

        poolManager.CallWithAutoReturn(PoolId.Dust_Default, transform.position, 1f);

        AudioManager.Instance.PlayAudio3DSFX(breakObj, transform.position);

        lootDropper.DropLoot();
    }
}
