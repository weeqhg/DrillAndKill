using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string sceneName = "TestRoom";
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

        UIManager.Instance.Open(_mainMenuUI);
    }

    private void StartGame()
    {
        GameEvents.GameStart(sceneName);
        UIManager.Instance.CloseAll();
    }


}

