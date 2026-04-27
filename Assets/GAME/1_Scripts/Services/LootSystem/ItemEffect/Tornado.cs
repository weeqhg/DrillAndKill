using UnityEngine;

public class Tornado : ItemEffect
{
    public ItemData item;
    private PlayerMovement playerMovement;
    private DualGun dualGun;
    private Sword sword;
    private float damageIncrease;
    private bool isIncrease = false;



    public override void OnApply(GameObject owner)
    {
        playerMovement = owner.GetComponentInChildren<PlayerMovement>();

        dualGun = playerMovement.gameObject.GetComponentInChildren<DualGun>();
        sword = playerMovement.gameObject.GetComponentInChildren<Sword>();
    }

    private void Update()
    {
        if (playerMovement == null) return;

        if (!playerMovement.IsGrounded && !isIncrease)
        {
            IncreaseDamage();
        }
        else if (playerMovement.IsGrounded && isIncrease)
        {
            DowngradeDamage();
        }
    }

    public override void OnRemove(GameObject owner)
    {
        playerMovement = null;
    }

    public override void OnStackChanged(int stack)
    {
        damageIncrease = item.statModule.value * stack;
    }

    private void IncreaseDamage()
    {
        if (dualGun != null) dualGun.IncreaseDamage(damageIncrease);
        if (sword != null) sword.IncreaseDamage(damageIncrease);

        isIncrease = true;
    }

    private void DowngradeDamage()
    {
        if (dualGun != null) dualGun.ResetDamage();
        if (sword != null) sword.ResetDamage();

        isIncrease = false;
    }
}
