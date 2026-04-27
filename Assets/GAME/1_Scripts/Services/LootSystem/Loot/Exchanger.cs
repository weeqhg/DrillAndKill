using UnityEngine;

public enum ExchangerType { Item, Talant }

public class Exchanger : BaseInteractable
{
    public ItemData item; [Tooltip("Установить если нужно обменивать предмет, стандартно обменивает очки таланта")]
    private SpriteRenderer sprite;
    private ExchangerType type;
    private SoundData interactSound;
    private LootDropper lootDropper;
    private int currentCost;
    private int baseCost = 25;
    private Camera cam;



    private void Start()
    {
        SetupDerived();
    }

    private void LateUpdate()
    {
        if (sprite != null) sprite.transform.forward = cam.transform.forward;
    }

    protected override void SetupDerived()
    {
        cam = Camera.main;
        outLine = GetComponentInChildren<OutLine>();
        outLine.SetActive(false);
        
        sprite = GetComponentInChildren<SpriteRenderer>();

        if (item != null)
        {
            sprite.sprite = item.icon;
            type = ExchangerType.Item;
        }
        else
        {
            type = ExchangerType.Talant;
        }

        interactSound = Resources.Load<SoundData>("Audio/SFX/Open");
        lootDropper = GetComponent<LootDropper>();

        CalculateCost();
    }

    public override string GetHint()
    {
        switch (type)
        {
            case ExchangerType.Item:
                var wallet = PlayerService.Player.GetComponent<MoneyManager>();
                bool canAffordItem = wallet.Money >= currentCost;
                string colorItem = canAffordItem ? "white" : "red";
                return $"<color={colorItem}>{currentCost} $</color>";

            case ExchangerType.Talant:
                var exp = PlayerService.Player.GetComponent<LevelManager>();
                bool canAffordTalent = exp.Exp >= currentCost;
                string colorTalent = canAffordTalent ? "white" : "red";
                return $"<color={colorTalent}>{currentCost} EXP</color>";

            default:
                return $"<color=white>{currentCost} </color>";
        }

    }

    private void CalculateCost()
    {
        currentCost = baseCost * Mathf.RoundToInt(1 + G.GameFlow.GameTIME / 60f);
    }

    public override void Interact(PlayerInteractor playerInteractor)
    {
        if (isUsed) return;

        switch (type)
        {
            case ExchangerType.Item:
                if (!TryPayCoin(playerInteractor))
                    return;
                ItemDrop(item);

                break;
            case ExchangerType.Talant:
                if (!TryPayExp(playerInteractor))
                    return;
                SkillPointDrop();

                break;
        }

        currentCost *= 2;

    }

    private bool TryPayExp(PlayerInteractor playerInteractor)
    {
        var levelManager = playerInteractor.GetComponent<LevelManager>();

        if (levelManager == null) return false;

        if (!levelManager.SpendExp(currentCost))
        {
            ConsoleEvents.ConsoleMessage("Not enough coins");

            //Audio Effect
            return false;
        }

        return true;
    }

    private bool TryPayCoin(PlayerInteractor playerInteractor)
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

    private void SkillPointDrop()
    {
        G.AudioManager?.Play(interactSound, transform.position);

        lootDropper.DropLootTalanted();

        isUsed = false;
    }

    private void ItemDrop(ItemData item)
    {
        G.AudioManager?.Play(interactSound, transform.position);

        lootDropper.DropLootItem(item);

        isUsed = false;
    }
}
