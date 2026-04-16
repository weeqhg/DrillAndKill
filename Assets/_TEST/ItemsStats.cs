using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemsStats : MonoBehaviour
{
    // 👉 сколько предметов каждого типа
    private Dictionary<ItemData, int> _itemStacks = new();

    // 👉 модификаторы
    private Dictionary<StatType, List<StatModifier>> _modifiers = new();

    public event Action OnStatsChanged;

    // =========================
    // 🔥 ADD ITEM
    // =========================
    public void AddItem(ItemData itemData)
    {
        if (!_itemStacks.ContainsKey(itemData))
        {
            _itemStacks[itemData] = 0;

            // 👉 первый раз — вешаем эффекты
            ApplyEffects(itemData);
        }

        _itemStacks[itemData]++;

        ApplyStats(itemData);

        AddItemVisualUI(itemData);

        OnStatsChanged?.Invoke();
    }

    // =========================
    // 🔥 REMOVE ITEM
    // =========================
    public void RemoveItem(ItemData itemData)
    {
        if (!_itemStacks.ContainsKey(itemData)) return;

        _itemStacks[itemData]--;

        RemoveStats(itemData);

        if (_itemStacks[itemData] <= 0)
        {
            _itemStacks.Remove(itemData);

            // 👉 если стак закончился — убираем эффекты
            RemoveEffects(itemData);
        }

        RemoveItemVisualUI(itemData);

        OnStatsChanged?.Invoke();
    }

    // =========================
    // 🔥 STATS
    // =========================
    private void ApplyStats(ItemData itemData)
    {
        Debug.Log("X");
        if (!_modifiers.ContainsKey(itemData.stats.type))
            _modifiers[itemData.stats.type] = new List<StatModifier>();

        _modifiers[itemData.stats.type].Add(new StatModifier(itemData.stats.value, itemData.stats.modifierType));
    }

    private void RemoveStats(ItemData itemData)
    {
        if (!_modifiers.ContainsKey(itemData.stats.type)) return;

        var list = _modifiers[itemData.stats.type];

        var mod = list.Find(m => m.value == itemData.stats.value && m.type == itemData.stats.modifierType);

        list.Remove(mod);
    }

    // =========================
    // 🔥 EFFECTS
    // =========================
    private void ApplyEffects(ItemData itemData)
    {
        //itemData.effects.OnApply(gameObject);
    }

    private void RemoveEffects(ItemData itemData)
    {
        //itemData.effects.OnRemove(gameObject);
    }

    // =========================
    // 🔥 APPLY FINAL STATS
    // =========================
    public float Apply(StatType type, float baseValue)
    {
        if (!_modifiers.TryGetValue(type, out var modifiers))
            return baseValue;

        float flat = 0f;
        float increased = 0f;
        float more = 1f;

        foreach (var mod in modifiers)
        {
            switch (mod.type)
            {
                case ModifierType.Flat: flat += mod.value; break;
                case ModifierType.Increased: increased += mod.value; break;
                case ModifierType.More: more *= (1 + mod.value); break;
            }
        }

        return (baseValue + flat) * (1 + increased) * more;
    }

    // =========================
    // 🔥 UI
    // =========================
    private void AddItemVisualUI(ItemData item)
    {
        // TODO
    }

    private void RemoveItemVisualUI(ItemData item)
    {
        // TODO
    }
}