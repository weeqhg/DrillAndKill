using UnityEngine;
using UnityEngine.UI;
using System;


public class MainMenuUI : MonoBehaviour
{
    [Header("Version")]
    [SerializeField] private Text version;
    [Header("CanvasGroup")]
    [SerializeField] private CanvasGroup _mainMenu;
    [SerializeField] private CanvasGroup _authorMenu;

    [Header("Buttons")]
    [SerializeField] private Button _startGame;
    [SerializeField] private Button _settingGame;
    [SerializeField] private Button _authorGame;
    [SerializeField] private Button _quitGame;

    public event Action OnSettingsActiveUI;
    public event Action OnStartGame;

    public void Init()
    {
        ShowMainMenu();

        version.text = "v" + Application.version;

        _startGame.onClick.AddListener(StartGame);
        _authorGame.onClick.AddListener(ShowAuthor);
        _settingGame.onClick.AddListener(ShowSetting);
        _quitGame.onClick.AddListener(QuitGame);
    }


    public void ShowMainMenu()
    {
        HideAll();

        _mainMenu.alpha = 1f;
        _mainMenu.interactable = true;
        _mainMenu.blocksRaycasts = true;
    }

    private void StartGame()
    {
        OnStartGame?.Invoke();
        HideAll();
    }


    private void ShowAuthor()
    {
        HideAll();

        _authorMenu.alpha = 1f;
        _authorMenu.interactable = true;
        _authorMenu.blocksRaycasts = true;
    }

    private void ShowSetting()
    {
        HideAll();

        OnSettingsActiveUI?.Invoke();
    }

    private void HideAll()
    {
        _mainMenu.alpha = 0f;
        _mainMenu.interactable = false;
        _mainMenu.blocksRaycasts = false;

        _authorMenu.alpha = 0f;
        _authorMenu.interactable = false;
        _authorMenu.blocksRaycasts = false;
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

