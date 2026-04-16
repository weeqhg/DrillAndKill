using System;
using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

[System.Serializable]
public class ItemStat
{
    public StatType type;
    public float value;
    public ModifierType modifierType;
}

[CreateAssetMenu(menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemRarity rarity;

    public ItemStat stats;
    //Эффекты TODO
}