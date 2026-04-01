using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "SkillTree/TalentNodeData")]
public class TalentNodeData : ScriptableObject
{
    public string id;
    public Sprite icon;
    public LocalizedString nodeName;
    public List<string> connections;

    public StatType statType;
    public ModifierType modifierType;
    public KeystoneEffect keystoneEffect;
    public float statValue;

    [Header("Hybrid Options")]
    public bool isBridgeNode = false;
    public Vector2 customPosition = Vector2.zero;
}