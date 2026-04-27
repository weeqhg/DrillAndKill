using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemInformationUI : MonoBehaviour
{
    public ItemTooltip tooltip;
    [SerializeField] private ItemSlotUI itemSlotPrefab;
    [SerializeField] private Transform container;

    private Dictionary<string, int> itemCounts = new();
    private Dictionary<string, ItemSlotUI> slots = new();


    public void Initialize(ItemsStatsUI items)
    {
        items.OnAddItem += AddItem;
        items.OnRemoveItem += RemoveItem;
        items.OnRemoveAll += ClearInfo;

        HideTooltip();
    }

    private void ShowTooltip(ItemData item)
    {
        if (tooltip != null)
            tooltip.ShowTooltip(item);
    }

    private void HideTooltip()
    {
        if (tooltip != null)
            tooltip.HideTooltip();
    }

    private void AddItem(ItemData item)
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
        newSlot.Initialize(item);
        newSlot.SetCount(itemCounts[key]);

        slots[key] = newSlot;

        AddTooltipEvents(newSlot.gameObject, item);
    }

    private void AddTooltipEvents(GameObject obj, ItemData item)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = obj.AddComponent<EventTrigger>();

        if (trigger.triggers == null)
            trigger.triggers = new List<EventTrigger.Entry>();

        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener(_ => ShowTooltip(item));

        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener(_ => HideTooltip());

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    private void RemoveItem(ItemData item)
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

    private void ClearInfo()
    {
        foreach (var slot in slots.Values)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        slots.Clear();
    }
}
