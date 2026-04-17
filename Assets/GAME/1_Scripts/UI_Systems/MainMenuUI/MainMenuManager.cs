using UnityEngine;
using Unity.Cinemachine;


public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private SettingManager _settingManagerPrefab;
    [SerializeField] private CharacterSelector _characterSelectorPrefab;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private SettingManager _settingManager;
    private CharacterSelector _characterSelector;
    private AuthorUI _authorUI;
    private MainMenuUI _mainMenuUI;

    public void Initialize()
    {
        _settingManager = Instantiate(_settingManagerPrefab);
        _settingManager.Initialize();

        _characterSelector = Instantiate(_characterSelectorPrefab);
        _characterSelector.Initialize();
        _characterSelector.OnPlayerReady += StartGame;

        _authorUI = GetComponentInChildren<AuthorUI>();
        _authorUI.Init();

        _mainMenuUI = GetComponentInChildren<MainMenuUI>();
        _mainMenuUI.Initialize(_settingManager, _characterSelector, _authorUI, cinemachineCamera);

        G.UIManager.Open(_mainMenuUI);
    }

    private void StartGame()
    {
        G.GameFlow?.StartGamePlay();
        G.UIManager.CloseAll();
    }
}

