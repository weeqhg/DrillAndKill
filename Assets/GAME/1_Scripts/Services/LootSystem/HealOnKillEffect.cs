using UnityEngine;

public class HealOnKillEffect : ItemEffect
{
    [SerializeField] private float healPerStack = 1f;

    private GameObject owner;
    private float currentHeal;

    public override void OnApply(GameObject owner)
    {
        this.owner = owner;
        PlayerService.OnKill += HandleKill;
    }

    public override void OnRemove(GameObject owner)
    {
        PlayerService.OnKill -= HandleKill;
    }

    public override void OnStackChanged(int stack)
    {
        currentHeal = healPerStack * stack;
    }

    private void HandleKill()
    {
        var health = owner.GetComponent<Health>();
        health?.Heal(currentHeal);
    }

    private void OnDestroy()
    {
        PlayerService.OnKill -= HandleKill;
    }
}