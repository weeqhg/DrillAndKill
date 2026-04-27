
using UnityEngine;

public class GlassCannon : ItemEffect
{
    public override void OnApply(GameObject owner)
    {
        SkillTreeStats stats = owner.GetComponentInChildren<SkillTreeStats>();
        
        stats.AddModifier(StatType.Damage, 1.0f, ModifierType.More);
        stats.AddModifier(StatType.MaxHealth, -0.5f, ModifierType.More);
    }

    public override void OnRemove(GameObject owner) {}
}