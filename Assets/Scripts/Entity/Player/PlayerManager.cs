using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public void Initialize()
    {
        EntityStats stats = GetComponent<EntityStats>();
        CameraShake cameraShake = GetComponentInChildren<CameraShake>();
        CameraContorller cameraContorller = GetComponentInChildren<CameraContorller>();
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        IKTargetFollower ikFollower = GetComponentInChildren<IKTargetFollower>();
        DualGun dualGun = GetComponentInChildren<DualGun>();
        EventSFX eventSFX = GetComponent<EventSFX>();
        Health health = GetComponent<Health>();
        PlayerHUD playerHUD = GetComponentInChildren<PlayerHUD>();

        playerHUD.Initialize();
        cameraContorller.Initialize();
        playerMovement.Initialize();
        ikFollower.Initialize();
        dualGun.Initialize(cameraShake);
        eventSFX.Initialize();
        health?.Initialize();
    }
}
