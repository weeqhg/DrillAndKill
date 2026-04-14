using System.Collections;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [Header("Обязательные системы")]
    [SerializeField] private InputManager inputManagerPrefab;
    [SerializeField] private UIManager uiManagerPrefab;
    [SerializeField] private PoolManager poolManagerPrefab;
    [SerializeField] private AudioManager audioManagerPrefab;
    [SerializeField] private GameManager gameManagerPrefab;

    [Header("В зависимости от сцены")]
    [SerializeField] private MainMenuManager mainMenuManagerPrefab;
    [SerializeField] private GameMenu gameMenuPrefab;
    [SerializeField] private LaunchLevel launchLevelPrefab;
    [SerializeField] private WorldManager _world;

    [Header("Доп. ситсемы для тестов")]
    [SerializeField] private Console consolePrefab;

    private SceneLoader _sceneLoader;
    private MainMenuManager _mainMenuManager;
    private Console _console;
    private GameMenu _gameMenu;
    private LaunchLevel _launchLevel;


    private void Start()
    {
        StartCoroutine(BootstrapRoutine());
    }

    private IEnumerator BootstrapRoutine()
    {
        if (GameManager.Instance == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }

        _sceneLoader = GameManager.Instance.SceneLoader;

        _sceneLoader?.Show();
        yield return null;

        float progress = 0f;

        // --- Singleton'ы ---
        if (InputManager.Instance == null && inputManagerPrefab != null)
            Instantiate(inputManagerPrefab);

        progress += 0.1f;
        _sceneLoader?.SetProgress(progress);
        yield return null;

        if (AudioManager.Instance == null && audioManagerPrefab != null)
            Instantiate(audioManagerPrefab);

        progress += 0.1f;
        _sceneLoader?.SetProgress(progress);
        yield return null;

        if (PoolManager.Instance == null && poolManagerPrefab != null)
            Instantiate(poolManagerPrefab);

        progress += 0.1f;
        _sceneLoader?.SetProgress(progress);
        yield return null;

        if (UIManager.Instance == null && uiManagerPrefab != null)
            Instantiate(uiManagerPrefab);

        progress += 0.1f;
        _sceneLoader?.SetProgress(progress);
        yield return null;

        // --- Остальные системы ---
        if (mainMenuManagerPrefab != null) _mainMenuManager = Instantiate(mainMenuManagerPrefab);
        if (consolePrefab != null) _console = Instantiate(consolePrefab);
        if (gameMenuPrefab != null) _gameMenu = Instantiate(gameMenuPrefab);
        if (launchLevelPrefab != null) _launchLevel = Instantiate(launchLevelPrefab);

        progress += 0.3f;
        _sceneLoader?.SetProgress(progress);
        yield return null;

        Initialized();

        progress = 1f;
        _sceneLoader?.SetProgress(progress);

        yield return new WaitForSeconds(0.5f); // чуть задержки для UX

        _sceneLoader?.Hide();
    }

    private void Initialized()
    {
        _mainMenuManager?.Initialize();
        _gameMenu?.Initialize();
        _console?.Initialize();
        _world?.Initialize();
        _launchLevel?.Initialize();
    }
}
