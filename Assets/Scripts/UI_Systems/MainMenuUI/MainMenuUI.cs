using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;

public class MainMenuUI : UIWindow, ICloseBlocker
{
    [Header("Version")]
    [SerializeField] private Text version;

    [Header("Buttons")]
    [SerializeField] private Button _startGame;
    [SerializeField] private Button _settingGame;
    [SerializeField] private Button _authorGame;
    [SerializeField] private Button _quitGame;

    private SettingManager _settingManager;
    private CharacterSelectorUI _selectorUI;
    private AuthorUI _authorUI;
    private CinemachineCamera _cinemachineCameraMain;
    private CinemachineCamera _cinemachineCameraSelector;

    public void Initialize(SettingManager settingManager, CharacterSelector characterSelector, AuthorUI authorUI, CinemachineCamera cinemachineCamera)
    {
        _settingManager = settingManager;
        _selectorUI = characterSelector.gameObject.GetComponentInChildren<CharacterSelectorUI>();
        _authorUI = authorUI;
        _cinemachineCameraMain = cinemachineCamera;

        version.text = "v" + Application.version;

        _startGame.onClick.AddListener(OpenCharacterSelector);
        _authorGame.onClick.AddListener(OpenAuthorUI);
        _settingGame.onClick.AddListener(OpenSettings);
        _quitGame.onClick.AddListener(QuitGame);

        _cinemachineCameraSelector = characterSelector.GetComponentInChildren<CinemachineCamera>();
    }
    public override void Show()
    {
        base.Show();
        _cinemachineCameraMain.enabled = true;
        _cinemachineCameraSelector.enabled = false;
    }

    public override void Hide()
    {
        base.Hide();
    }

    private void OpenCharacterSelector()
    {
        UIManager.Instance.Open(_selectorUI);
        _cinemachineCameraMain.enabled = false;
        _cinemachineCameraSelector.enabled = true;
    }

    private void OpenAuthorUI()
    {
        UIManager.Instance.Open(_authorUI);
    }

    private void OpenSettings()
    {
        UIManager.Instance.Open(_settingManager);
    }

    private void OnDestroy()
    {
        _startGame.onClick.RemoveAllListeners();
        _authorGame.onClick.RemoveAllListeners();
        _settingGame.onClick.RemoveAllListeners();
        _quitGame.onClick.RemoveAllListeners();
    }

    private void QuitGame()
    {
        Debug.Log("Выход из игры...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
