using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemsStats : MonoBehaviour
{

    [SerializeField] private ItemsStatsUI UI;
    // 👉 сколько предметов каждого типа
    private Dictionary<string, int> _itemStacks = new();

    // 👉 модификаторы
    private Dictionary<StatType, List<StatModifier>> _modifiers = new();

    // 👉 эффекты
    private Dictionary<string, ItemEffect> _activeEffects = new();
    private Dictionary<string, int> _effectStacks = new();

    public event Action OnStatsChanged;


    // =========================
    // 🔥 ADD ITEM
    // =========================
    public void AddItem(ItemData item)
    {
        string id = item.ID;

        if (!_itemStacks.ContainsKey(id)) _itemStacks[id] = 0;

        _itemStacks[id]++;

        AddModifier(item);
        AddEffect(item);

        UI.AddItem(item);
        OnStatsChanged?.Invoke();

        //AudioEffect
    }

    // =========================
    // 🔥 REMOVE ITEM
    // =========================
    public void RemoveItem(ItemData item)
    {
        string id = item.ID;

        if (!_itemStacks.ContainsKey(id)) return;

        _itemStacks[id]--;

        RemoveModifier(item);
        RemoveEffect(item);

        if (_itemStacks[id] <= 0)
            _itemStacks.Remove(id);

        UI.RemoveItem(item);
        OnStatsChanged?.Invoke();
    }

    // =========================
    // 🔥 STATS
    // =========================
    private void AddModifier(ItemData item)
    {
        if (item.statModule.type == StatType.None) return;

        var stat = item.statModule;

        if (!_modifiers.TryGetValue(stat.type, out var list))
        {
            list = new List<StatModifier>();
            _modifiers[stat.type] = list;
        }

        list.Add(new StatModifier(stat.value, stat.modifierType));
    }

    private void RemoveModifier(ItemData item)
    {
        if (item.statModule.type == StatType.None) return;

        var stat = item.statModule;

        if (!_modifiers.TryGetValue(stat.type, out var list))
            return;

        if (list.Count > 0)
            list.RemoveAt(list.Count - 1);

        if (list.Count == 0)
            _modifiers.Remove(stat.type);
    }

    // =========================
    // 🔥 EFFECTS
    // =========================
    private void AddEffect(ItemData item)
    {
        if (item.itemEffect == null) return;

        string id = item.ID;

        ItemEffect effect;

        // 👉 создаём эффект если его нет
        if (!_activeEffects.TryGetValue(id, out effect))
        {
            var obj = Instantiate(item.itemEffect, transform);

            effect = obj.GetComponent<ItemEffect>();
            effect.OnApply(transform.parent.gameObject);

            _activeEffects[id] = effect;
            _effectStacks[id] = 0;
        }

        // 👉 увеличиваем stack
        _effectStacks[id]++;

        // 👉 безопасный вызов
        effect.OnStackChanged(_effectStacks[id]);
    }

    private void RemoveEffect(ItemData item)
    {
        string id = item.ID;

        if (!_activeEffects.TryGetValue(id, out var effect))
            return;

        if (!_effectStacks.ContainsKey(id))
            return;

        _effectStacks[id]--;

        if (_effectStacks[id] <= 0)
        {
            effect.OnRemove(transform.parent.gameObject);

            Destroy(effect.gameObject);

            _activeEffects.Remove(id);
            _effectStacks.Remove(id);
            return;
        }

        effect.OnStackChanged(_effectStacks[id]);
    }

    // =========================
    // 🔥 StatController
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

        return (baseValue + flat) * (1 + increased / 100) * more;
    }
}