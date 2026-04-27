using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "SkillTree/TalentNodeData")]
public class TalentNodeData : ScriptableObject
{
    public string id;
    public Vector2 position;
    public List<string> connections;
    public Sprite icon;
    public LocalizedString nodeName;
    public LocalizedString description;

    public StatType statType;
    public ModifierType modifierType;
    public float statValue;
    public ItemEffect itemEffect;
}