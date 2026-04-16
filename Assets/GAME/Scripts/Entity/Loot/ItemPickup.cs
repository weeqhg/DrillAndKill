using UnityEngine;

public class ItemPickup : Collectable
{
    [SerializeField] private SpriteRenderer iconItem;
    private ItemData item;
    private bool isInit = false;
    public void Initialize(ItemData itemData)
    {
        item = itemData;
        iconItem.sprite = itemData.icon;
        isInit = true;
    }
    protected override void Collect()
    {
        if (isInit == false) return;
        if (targetPlayer == null) return;

        ItemsStats itemsStats = targetPlayer.GetComponentInChildren<ItemsStats>();
        itemsStats?.AddItem(item);


        Destroy(gameObject);
    }
}
