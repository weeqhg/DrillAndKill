using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using WekenDev.InputSystem;
using WekenDev.Settings;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private string sceneName = "MainMenu";
    [SerializeField] private SettingManager settingMenuPrefab;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitMenuButton;
    private bool isConsole = false;
    private bool isSetting = false;
    private CanvasGroup canvasGroupGameMenu;
    private SettingManager settingManager;
    private EntityStatsUI statsUI;
    public void Initialize()
    {
        statsUI = GetComponentInChildren<EntityStatsUI>();
        canvasGroupGameMenu = GetComponentInChildren<CanvasGroup>();
        GameEvents.OnPlayerSpawned += (PlayerManager playerManager) =>
        {
            statsUI.UpdateComponent(playerManager);
        };
        GameEvents.OnConsole += value => isConsole = value;
        InputManager.Instance.Actions.Player.Pause.performed += OpenGameMenu;
        InputManager.Instance.Actions.UI.Cancel.performed += Cancel;

        continueButton.onClick.AddListener(() =>
        {
            ToggleGameMenu(false);
        });

        settingButton.onClick.AddListener(() =>
        {
            HideGameMenu();
            settingManager.Show();
            isSetting = true;
        });

        settingManager = Instantiate(settingMenuPrefab, transform);
        if (settingManager != null)
        {
            settingManager.Initialize();
            settingManager.OnCloseSetting += () =>
            {
                isSetting = false;
                ShowGameMenu();
            };
        }

        exitMenuButton.onClick.AddListener(() =>
        {
            GameEvents.GameStart(sceneName);
        });


        HideGameMenu();
    }
    private void OpenGameMenu(InputAction.CallbackContext contex)
    {
        ToggleGameMenu(true);
    }
    private void Cancel(InputAction.CallbackContext contex)
    {
        if (isSetting) return;

        ToggleGameMenu(false);
    }

    private void HideGameMenu()
    {
        canvasGroupGameMenu.alpha = 0f;
        canvasGroupGameMenu.interactable = false;
        canvasGroupGameMenu.blocksRaycasts = false;
    }

    private void ShowGameMenu()
    {
        canvasGroupGameMenu.alpha = 1f;
        canvasGroupGameMenu.interactable = true;
        canvasGroupGameMenu.blocksRaycasts = true;
    }
    private void ToggleGameMenu(bool enable)
    {
        if (isConsole) return;

        if (enable) InputManager.Instance.ChangeInputType(InputType.UI);
        else InputManager.Instance.ChangeInputType(InputType.Player);

        GameEvents.GameMenu(enable);

        if (enable) ShowGameMenu();
        else HideGameMenu();

        if (enable) statsUI.UpdateUI();
    }

    private void OnDestroy()
    {
        InputManager.Instance.Actions.Player.Pause.performed -= OpenGameMenu;
        InputManager.Instance.Actions.UI.Cancel.performed -= Cancel;
        GameEvents.OnConsole -= value => isConsole = value;
        GameEvents.OnPlayerSpawned -= (PlayerManager playerManager) =>
        {
            statsUI.UpdateComponent(playerManager);
        };
        settingManager.OnCloseSetting -= () => isSetting = false;
    }
}
