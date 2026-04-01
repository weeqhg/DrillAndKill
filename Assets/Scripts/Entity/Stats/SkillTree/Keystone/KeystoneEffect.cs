using UnityEngine;
using UnityEngine.Localization;

public abstract class KeystoneEffect : ScriptableObject
{
    [Header("UI")]
    public LocalizedString title;
    public LocalizedString description;
    public abstract void Apply(SkillTreeStats stats);
}