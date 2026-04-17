using System.Collections.Generic;
using UnityEngine;

public class ItemsStatsUI : MonoBehaviour
{
    [SerializeField] private ItemSlotUI itemSlotPrefab;
    [SerializeField] private Transform container;

    private Dictionary<string, int> itemCounts = new();
    private Dictionary<string, ItemSlotUI> slots = new();

    public void AddItem(ItemData item)
    {
        string key = item.ID;

        if (itemCounts.ContainsKey(key))
            itemCounts[key]++;
        else
            itemCounts[key] = 1;

        if (slots.TryGetValue(key, out var slot))
        {
            slot.SetCount(itemCounts[key]);
            return;
        }

        var newSlot = Instantiate(itemSlotPrefab, container);
        newSlot.Initialize(item.icon);
        newSlot.SetCount(itemCounts[key]);

        slots[key] = newSlot;
    }

    public void RemoveItem(ItemData item)
    {
        string key = item.ID;

        if (!itemCounts.ContainsKey(key))
            return;

        itemCounts[key]--;

        if (itemCounts[key] <= 0)
        {
            itemCounts.Remove(key);

            if (slots.TryGetValue(key, out var slot))
            {
                Destroy(slot.gameObject);
                slots.Remove(key);
            }

            return;
        }

        if (slots.TryGetValue(key, out var ui))
        {
            ui.SetCount(itemCounts[key]);
        }
    }
}