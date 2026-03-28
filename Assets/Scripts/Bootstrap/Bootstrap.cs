using UnityEngine;
using WekenDev.InputSystem;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private MainMenuManager mainMenuManagerPrefab;
    [SerializeField] private SceneLoader sceneLoaderPrefab;
    [SerializeField] private PlayerSpawner playerSpawnerPrefab;
    [SerializeField] private InputManager inputManagerPrefab;
    [SerializeField] private AudioManager audioManagerPrefab;
    [SerializeField] private Console consolePrefab;
    [SerializeField] private EnemySpawner enemySpawnerPrefab;
    [SerializeField] private PauseManager pauseManagerPrefab;
    [SerializeField] private GameMenu gameMenuPrefab;

    private MainMenuManager _mainMenuManager;
    private PlayerSpawner _playerSpawner;
    private EnemySpawner _enemySpawner;
    private Console _console;
    private PauseManager _pauseManager;
    private GameMenu _gameMenu;
    private SceneLoader _sceneLoader;

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            if (inputManagerPrefab != null)
            {
                Instantiate(inputManagerPrefab);
            }
        }

        if (AudioManager.Instance == null)
        {
            if (audioManagerPrefab != null)
            {
                Instantiate(audioManagerPrefab);
            }
        }

        if (mainMenuManagerPrefab != null) _mainMenuManager = Instantiate(mainMenuManagerPrefab);
        if (consolePrefab != null) _console = Instantiate(consolePrefab);
        if (playerSpawnerPrefab != null) _playerSpawner = Instantiate(playerSpawnerPrefab);
        if (enemySpawnerPrefab != null) _enemySpawner = Instantiate(enemySpawnerPrefab);
        if (pauseManagerPrefab != null) _pauseManager = Instantiate(pauseManagerPrefab);
        if (gameMenuPrefab != null) _gameMenu = Instantiate(gameMenuPrefab);
        if (sceneLoaderPrefab != null) _sceneLoader = Instantiate(sceneLoaderPrefab);

        Initialized();
    }

    private void Initialized()
    {
        _mainMenuManager?.Initialize();
        _gameMenu?.Initialize();
        _pauseManager?.Initialize();
        _playerSpawner?.Initialize();
        _enemySpawner?.Initialize();
        _console?.Initialize();
        _sceneLoader?.Initialize();
        AudioManager.Instance?.Initialize();
    }
}
