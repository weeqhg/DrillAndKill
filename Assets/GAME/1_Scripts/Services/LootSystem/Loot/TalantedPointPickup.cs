using UnityEngine;

public class TalantedPointPickup : Collectable
{
    private Camera cam;



    public void Initialize()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        transform.forward = cam.transform.forward;
    }

    protected override void Collect()
    {
        if (targetPlayer == null) return;

        TalentPointsCounter talantedPoint = targetPlayer.GetComponentInChildren<TalentPointsCounter>();
        talantedPoint?.AddPoints(1);


        Destroy(gameObject);
    }
}
