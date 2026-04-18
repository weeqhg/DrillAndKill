using UnityEngine;

public class Chest : BaseInteractable
{

    [Header("Visual")]
    [SerializeField] private GameObject closedModel;
    [SerializeField] private GameObject openedModel;

    [Header("Settings")]
    [SerializeField] private int baseCost = 25;


    private SoundData openSound;
    private LootDropper lootDropper;

    private int currentCost;

    protected override void SetupDerived()
    {
        openSound = Resources.Load<SoundData>("Audio/SFX/Open");

        lootDropper = GetComponent<LootDropper>();

        closedModel.SetActive(true);
        openedModel.SetActive(false);

        CalculateCost();
    }

    public override string GetHint()
    {
        var wallet = PlayerService.Player.GetComponent<MoneyManager>();
        bool canAfford = wallet.Money >= currentCost;
        string color = canAfford ? "white" : "red";
        return $"<color={color}>{currentCost} $</color>";
    }
    private void CalculateCost()
    {
        currentCost = baseCost * Mathf.RoundToInt(1 + G.GameFlow.GameTIME / 60f);
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

        G.AudioManager?.Play(openSound, transform.position);

        lootDropper.DropLootItem(item);
    }
}
