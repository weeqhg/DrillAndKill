using UnityEngine;

public class ItemPickup : Collectable
{
    private SpriteRenderer iconItem;
    private ItemData item;
    private Camera cam;
    public void Initialize(ItemData itemData)
    {
        iconItem = GetComponentInChildren<SpriteRenderer>();
        cam = Camera.main;
        item = itemData;
        iconItem.sprite = itemData.icon;
    }

    private void LateUpdate()
    {
        transform.forward = cam.transform.forward;
    }

    protected override void Collect()
    {
        if (item == null || targetPlayer == null) return;

        ItemsStats itemsStats = targetPlayer.GetComponentInChildren<ItemsStats>();
        itemsStats?.AddItem(item);


        Destroy(gameObject);
    }
}
