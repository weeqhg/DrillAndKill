using UnityEngine;
using WekenDev.MainMenu.UI;
using UnityEngine.InputSystem;
using WekenDev.InputSystem;
using WekenDev.Settings;
using Unity.Cinemachine;


public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string sceneName = "TestRoom";
    [SerializeField] private SettingManager _settingManagerPrefab;
    [SerializeField] private CharacterSelector _characterSelectorPrefab;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    private SettingManager _settingManager;
    private CharacterSelector _characterSelector;
    private MainMenuUI _menuUI;
    private AuthorUI _authorUI;
    public void Initialize()
    {
        InputManager.Instance.ChangeInputType(InputType.UI);

        _characterSelector = Instantiate(_characterSelectorPrefab);
        _characterSelector.Initialize();
        _characterSelector.OnPlayerReady += () => GameEvents.GameStart(sceneName);
        _characterSelector.OnCloseSelector += Show;

        _settingManager = Instantiate(_settingManagerPrefab);
        _settingManager.Initialize();
        _settingManager.OnCloseSetting += Show;

        _authorUI = GetComponentInChildren<AuthorUI>();
        _authorUI.Init();
        _authorUI.OnCloseAuthorUI += Show;

        _menuUI = GetComponent<MainMenuUI>();
        _menuUI.Init();
        _menuUI.OnSettingsActiveUI += HandleShowSetting;
        _menuUI.OnStartGame += HandleShowStartMenu;

        Show();
    }

    private void Show()
    {
        cinemachineCamera.enabled = true;
        _menuUI.ShowMainMenu();
    }

    private void HandleShowSetting()
    {
        _settingManager.Show();
    }

    private void HandleShowStartMenu()
    {
        cinemachineCamera.enabled = false;
        _characterSelector.ShowMenu();
    }

    private void OnDestroy()
    {
        _characterSelector.OnPlayerReady -= () => GameEvents.GameStart(sceneName);
        _menuUI.OnSettingsActiveUI -= HandleShowSetting;
        _menuUI.OnStartGame -= HandleShowStartMenu;
        _characterSelector.OnCloseSelector -= Show;
        _settingManager.OnCloseSetting -= Show;
        _authorUI.OnCloseAuthorUI -= Show;
    }
}

