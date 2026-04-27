using UnityEngine;
using Unity.Collections;
using UnityEngine.Localization;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif


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
    [Header("Identification")]
    [SerializeField, ReadOnly] private string _id;
    [HideInInspector] public string ID => _id;
    public LocalizedString itemName;
    public LocalizedString description;
    public Sprite icon;
    public ItemRarity rarity;
    public StatModule statModule;
    public GameObject itemEffect;

#if UNITY_EDITOR
    private void OnValidate()
    {
        string path = AssetDatabase.GetAssetPath(this);
        string assetGuid = AssetDatabase.AssetPathToGUID(path);

        if (_id != assetGuid)
        {
            _id = assetGuid;
            EditorUtility.SetDirty(this);
        }
    }
#endif
}

[Serializable]
public class StatModule
{
    public StatType type;
    public float value;
    public ModifierType modifierType;
}