using UnityEngine;

[CreateAssetMenu(menuName = "SkillTree/Keystone/GlassCannon")]
public class GlassCannon : KeystoneEffect
{
    public override void Apply(SkillTreeStats stats)
    {
        stats.AddModifier(StatType.Damage, 1.0f, ModifierType.More);
        stats.AddModifier(StatType.MaxHealth, -0.5f, ModifierType.More);
    }
}