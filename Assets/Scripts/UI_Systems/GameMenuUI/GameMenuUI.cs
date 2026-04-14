using UnityEngine;
using UnityEngine.UI;

public class GameMenuUI : UIWindow
{
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

        settingManager.Initialize();

        gameMenuPanel.SetActive(false);

        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            OnPlayerReady(GameManager.Instance.Player);
        }
        GameManager.Instance.OnPlayerSpawned += OnPlayerReady;
    }

    private void OnPlayerReady(GameObject player)
    {
        StatsController statsController = player.GetComponentInChildren<StatsController>();
        statsUI.Initialize(statsController);
    }

    private void OpenSettings()
    {
        UIManager.Instance.Open(settingManager);
    }

    private void Exit()
    {
        UIManager.Instance.Close(this);
        GameEvents.EndGame();
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
        GameManager.Instance.OnPlayerSpawned -= OnPlayerReady;

        continueButton.onClick.RemoveAllListeners();
        settingButton.onClick.RemoveAllListeners();
        exitMenuButton.onClick.RemoveAllListeners();
    }
}