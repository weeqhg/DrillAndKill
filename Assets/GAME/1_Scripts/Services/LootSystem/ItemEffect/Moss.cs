using UnityEngine;

public class Moss : ItemEffect
{
    public ItemData item;
    private PlayerHealth health;
    private float mossTime;



    public override void OnApply(GameObject owner)
    {
        health = owner.GetComponentInChildren<PlayerHealth>();
        health.ToggleImmortal(true);
    }

    public override void OnRemove(GameObject owner)
    {
        health = null;
        health.ToggleImmortal(false);
    }

    public override void OnStackChanged(int stack)
    {
        mossTime = item.statModule.value * stack;
        health.SetRate(mossTime);
    }
}


