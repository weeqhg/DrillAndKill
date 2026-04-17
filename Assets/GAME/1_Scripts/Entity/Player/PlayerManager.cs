using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private Rigidbody rb;
    private CameraShake cameraShake;
    private IDamageable damageable;
    private StatsController statsController;
    public Transform Transform => transform;
    public CameraShake CameraShake => cameraShake;
    public IDamageable Damageable => damageable;
    public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;
    public StatsController StatsController => statsController;

    public void Initialize()
    {
        statsController = GetComponentInChildren<StatsController>(true);
        cameraShake = GetComponentInChildren<CameraShake>(true);
        damageable = GetComponent<IDamageable>();
        rb = GetComponent<Rigidbody>();

        CameraContorller cameraContorller = GetComponentInChildren<CameraContorller>();
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        IKTargetFollower ikFollower = GetComponentInChildren<IKTargetFollower>();
        DualGun dualGun = GetComponentInChildren<DualGun>();
        Sword sword = GetComponentInChildren<Sword>();
        EventSFX eventSFX = GetComponent<EventSFX>();
        Health health = GetComponent<Health>();
        PlayerHUD playerHUD = GetComponentInChildren<PlayerHUD>();
        LevelManager levelManager = GetComponentInChildren<LevelManager>();
        PlayerCollector playerCollector = GetComponent<PlayerCollector>();
        SkillTreeStats skillTreeStats = GetComponentInChildren<SkillTreeStats>();
        SkillTreeUI skillTreeUI = GetComponentInChildren<SkillTreeUI>();
        AutoPopup treePopup = GetComponentInChildren<AutoPopup>();
        PlayerInteractor playerInteractor = GetComponent<PlayerInteractor>();
        MoneyManager moneyManager = GetComponentInChildren<MoneyManager>();

        statsController.Initialize();
        skillTreeStats.Initialize();
        treePopup.Initialize();
        skillTreeUI.Initialize(skillTreeStats);
        playerHUD.Initialize();
        cameraContorller.Initialize();
        playerMovement.Initialize(statsController);
        ikFollower?.Initialize();
        dualGun?.Initialize(cameraShake, statsController);
        sword?.Initialize(cameraShake, statsController);
        health?.Initialize();
        levelManager.Initialize();
        playerCollector.Initialize();
        playerInteractor.Initialized();
        moneyManager.Initialize();

        G.GameFlow.OnEndGame += KillPlayer;
        
        ConsoleEvents.OnCommandKillPlayer += KillPlayer;
        ConsoleEvents.OnCommandImmortalPlayer += ImmortalPlayer;

        PlayerService.SetPlayer(this);
        DontDestroyOnLoad(gameObject);
    }

    public void HidePlayer()
    {
        gameObject.SetActive(false);
    }

    public void ShowPlayer()
    {
        gameObject.SetActive(true);
    }

    public void TeleportPlayer(Vector3 position)
    {
        gameObject.transform.position = position;
    }

    //---Команды для игрока---

    private void KillPlayer()
    {
        Health health = gameObject.GetComponentInChildren<Health>();

        if (health != null) health.Kill();
        else Destroy(gameObject);
    }

    private void ImmortalPlayer(bool enable)
    {
        Health health = gameObject.GetComponentInChildren<Health>();
        if (health != null) health.ToggleImmortal(enable);
        else ConsoleEvents.ConsoleMessage("Helath not found");
    }



    private void OnDestroy()
    {
        PlayerService.ClearPlayer();

        G.GameFlow.OnEndGame -= KillPlayer;

        ConsoleEvents.OnCommandKillPlayer -= KillPlayer;
        ConsoleEvents.OnCommandImmortalPlayer -= ImmortalPlayer;
    }
}
