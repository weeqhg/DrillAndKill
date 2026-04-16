using System.Collections.Generic;
using UnityEngine;

public class LootSystem : MonoBehaviour
{
    [SerializeField] private List<ItemData> allItems;

    public void Initialize()
    {
        if (G.LootSystem != null && G.LootSystem != this)
        {
            Destroy(gameObject);
            return;
        }

        G.LootSystem = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public ItemData GetItem(float luck)
    {
        ItemRarity rarity = RollRarity(luck);

        return GetRandomItem(rarity);
    }

    public ItemRarity RollRarity(float luck)
    {
        int rolls = 1 + Mathf.FloorToInt(luck);

        ItemRarity best = ItemRarity.Common;

        for (int i = 0; i < rolls; i++)
        {
            var roll = BaseRoll();

            if (roll > best)
                best = roll;
        }

        if (Random.value < luck * 0.15f)
        {
            best = Upgrade(best);
        }

        return best;
    }

    private ItemRarity BaseRoll()
    {
        float roll = Random.value;

        if (roll < 0.01f) return ItemRarity.Legendary;
        if (roll < 0.08f) return ItemRarity.Rare;
        if (roll < 0.25f) return ItemRarity.Uncommon;
        return ItemRarity.Common;
    }

    private ItemRarity Upgrade(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => ItemRarity.Uncommon,
            ItemRarity.Uncommon => ItemRarity.Rare,
            ItemRarity.Rare => ItemRarity.Legendary,
            _ => rarity
        };
    }

    private ItemData GetRandomItem(ItemRarity rarity)
    {
        var list = allItems.FindAll(i => i.rarity == rarity);

        if (list.Count == 0) return null;

        return list[Random.Range(0, list.Count)];
    }
}