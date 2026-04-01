using UnityEngine;
using UnityEngine.UI;

public class GameMenu : UIWindow
{
    [SerializeField] private string sceneName = "MainMenu";
    [SerializeField] private SettingManager settingMenuPrefab;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitMenuButton;
    [SerializeField] private GameObject gameMenuPanel;
    [SerializeField] private SettingManager settingManager;
    private StatsControllerUI statsUI;

    public void Initialize()
    {
        statsUI = GetComponentInChildren<StatsControllerUI>();

        continueButton.onClick.AddListener(() => UIManager.Instance.Close(this));
        settingButton.onClick.AddListener(OpenSettings);
        exitMenuButton.onClick.AddListener(Exit);

        GameEvents.OnPlayerSpawned += SetComponent;

        settingManager.Initialize();

        gameMenuPanel.SetActive(false);
    }

    private void OpenSettings()
    {
        UIManager.Instance.Open(settingManager);
    }

    private void Exit()
    {
        UIManager.Instance.Close(this);
        GameEvents.GameStart(sceneName);
    }

    private void SetComponent(PlayerManager playerManager)
    {
        statsUI.Initialize(playerManager.GetComponentInChildren<StatsController>());
    }

    public override void Show()
    {
        base.Show();
        gameMenuPanel.SetActive(true);
        statsUI?.UpdateUI();
    }

    public override void Hide()
    {
        base.Hide();
        gameMenuPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        continueButton.onClick.RemoveAllListeners();
        settingButton.onClick.RemoveAllListeners();
        exitMenuButton.onClick.RemoveAllListeners();

        GameEvents.OnPlayerSpawned -= SetComponent;
    }
}