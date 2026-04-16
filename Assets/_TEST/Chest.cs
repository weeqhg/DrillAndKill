using UnityEngine;

public class Chest : BaseInteractable
{

    [Header("Visual")]
    [SerializeField] private GameObject closedModel;
    [SerializeField] private GameObject openedModel;

    [Header("Settings")]
    [SerializeField] private int baseCost = 25;
    [SerializeField] private AudioClip openSound;

    private LootDropper lootDropper;
    private Collider objectCollider;

    private int currentCost;

    protected override void SetupDerived()
    {
        lootDropper = GetComponent<LootDropper>();
        objectCollider = GetComponent<Collider>();

        closedModel.SetActive(true);
        openedModel.SetActive(false);

        CalculateCost();
    }

    private void CalculateCost()
    {
        currentCost = baseCost * Mathf.RoundToInt(1 + G.DifficultyManager.timeDifficulty / 60f);
    }

    public override void Interact(PlayerInteractor playerInteractor)
    {
        if (isUsed) return;

        if (!TryPay(playerInteractor))
            return;

        StatsController statsController = playerInteractor.GetComponentInChildren<StatsController>();
        float luck = statsController.GetStat(StatType.Luck);
        ItemData item = G.LootSystem.GetItem(luck);

        OpenChest(item);
    }

    private bool TryPay(PlayerInteractor playerInteractor)
    {
        var wallet = playerInteractor.GetComponent<MoneyManager>();

        if (wallet == null) return false;

        if (!wallet.SpendCoin(currentCost))
        {
            ConsoleEvents.ConsoleMessage("Not enough coins");

            //Audio Effect
            return false;
        }

        return true;
    }

    private void OpenChest(ItemData item)
    {
        isUsed = true;

        closedModel.SetActive(false);
        openedModel.SetActive(true);

        objectCollider.enabled = false;

        G.AudioManager?.PlayAudio3DSFX(openSound, transform.position);

        lootDropper.DropLootItem(item);
    }
}
