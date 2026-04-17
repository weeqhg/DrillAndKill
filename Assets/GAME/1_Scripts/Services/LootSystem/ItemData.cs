using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

[CreateAssetMenu(menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string ID;
    public string itemName;
    public Sprite icon;
    public ItemRarity rarity;

    public StatModule statModule;

    public GameObject itemEffect;
}

[CreateAssetMenu(menuName = "Items/Modules/Stat")]
public class StatModule : ScriptableObject
{
    public StatType type;
    public float value;
    public ModifierType modifierType;
}