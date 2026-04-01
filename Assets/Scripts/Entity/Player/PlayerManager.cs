using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public void Initialize()
    {
        StatsController statsController = GetComponentInChildren<StatsController>();
        CameraShake cameraShake = GetComponentInChildren<CameraShake>();
        CameraContorller cameraContorller = GetComponentInChildren<CameraContorller>();
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        IKTargetFollower ikFollower = GetComponentInChildren<IKTargetFollower>();
        DualGun dualGun = GetComponentInChildren<DualGun>();
        EventSFX eventSFX = GetComponent<EventSFX>();
        Health health = GetComponent<Health>();
        PlayerHUD playerHUD = GetComponentInChildren<PlayerHUD>();
        LevelManager levelManager = GetComponentInChildren<LevelManager>();
        PlayerCollector playerCollector = GetComponent<PlayerCollector>();
        SkillTreeStats skillTreeStats = GetComponentInChildren<SkillTreeStats>();
        SkillTreeUI skillTreeUI = GetComponentInChildren<SkillTreeUI>();
        AutoPopup treePopup = GetComponentInChildren<AutoPopup>();

        statsController.Initialize();
        skillTreeStats.Initialize();
        treePopup.Initialize();
        skillTreeUI.Initialize(skillTreeStats);
        playerHUD.Initialize();
        cameraContorller.Initialize();
        playerMovement.Initialize(statsController);
        ikFollower.Initialize();
        dualGun.Initialize(cameraShake, statsController);
        health?.Initialize();
        levelManager.Initialize();
        playerCollector.Initialize();
    }
}
