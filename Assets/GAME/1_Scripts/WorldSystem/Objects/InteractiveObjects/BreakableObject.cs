using UnityEngine;
using UnityEngine.Localization;

public class BreakableObject : BaseInteractable, IDamageable
{
    [SerializeField] private GameObject intactModel;  // Целая модель
    [SerializeField] private GameObject destroyedModel; // Разрушенная модель
    [SerializeField] private LocalizedString localizedHint;
    private Collider objectCollider;
    private LootDropper lootDropper;
    private int levelMultiplier = 1;
    private SoundData breakSound;



    protected override void SetupDerived()
    {
        levelMultiplier = G.GameFlow?.DifficultyLevel ?? 1;
        objectCollider = GetComponent<Collider>();
        intactModel.SetActive(true);
        destroyedModel.SetActive(false);

        lootDropper = GetComponent<LootDropper>();
        CalculateReward();

        breakSound = Resources.Load<SoundData>("Audio/SFX/Break");
    }

    public override string GetHint()
    {
        if (localizedHint != null) return localizedHint.GetLocalizedString();
        else return "";
    }


    private void CalculateReward()
    {
        int expReward = Random.Range(10, 50) * levelMultiplier;
        int coinReward = Random.Range(10, 50) * levelMultiplier;
        lootDropper.SetReward(expReward, coinReward);
    }

    public override void Interact(PlayerInteractor playerInteractor)
    {
        if (isUsed) return;
        BreakObj();
    }

    public void TakeDamage(float damage)
    {
        if (isUsed) return;
        BreakObj();
    }

    private void BreakObj()
    {
        isUsed = true;

        // Переключаем модели
        intactModel.SetActive(false);
        destroyedModel.SetActive(true);

        // Отключаем коллайдер (не нужно больше)
        objectCollider.enabled = false;

        G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Default, transform.position, 1f);

        G.AudioManager?.Play(breakSound, transform.position);

        if (Random.value < 0.5f)
        {
            lootDropper.DropLootEXP();
        }
        else
        {
            lootDropper.DropLootCOIN();
        }
    }
}
