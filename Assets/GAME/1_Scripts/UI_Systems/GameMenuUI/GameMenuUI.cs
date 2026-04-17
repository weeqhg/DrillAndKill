using UnityEngine;
using UnityEngine.UI;

public class GameMenuUI : UIWindow
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitMenuButton;
    [SerializeField] private GameObject gameMenuPanel;
    [SerializeField] private SettingManager settingManager;

    public void Initialize()
    {
        continueButton.onClick.AddListener(() => G.UIManager.Close(this));
        settingButton.onClick.AddListener(OpenSettings);
        exitMenuButton.onClick.AddListener(Exit);

        settingManager.Initialize();

        gameMenuPanel.SetActive(false);
    }

    private void OpenSettings()
    {
        G.UIManager.Open(settingManager);
    }

    private void Exit()
    {
        G.UIManager.Close(this);
        G.GameFlow?.EndHandler();
    }

    public override void Show()
    {
        base.Show();
        gameMenuPanel.SetActive(true);
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
    }
}