using UnityEngine;

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
    [SerializeField] private PoolManager poolManagerPrefab;
    [SerializeField] private GameInput gameInputPrefab;
    [SerializeField] private UIManager uiManagerPrefab;

    private MainMenuManager _mainMenuManager;
    private PlayerSpawner _playerSpawner;
    private EnemySpawner _enemySpawner;
    private Console _console;
    private PauseManager _pauseManager;
    private SceneLoader _sceneLoader;
    private GameInput _gameInput;

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

        if (PoolManager.Instance == null)
        {
            if (poolManagerPrefab != null)
            {
                Instantiate(poolManagerPrefab);
            }
        }

        if (UIManager.Instance == null)
        {
            if (uiManagerPrefab != null)
            {
                Instantiate(uiManagerPrefab);
            }
        }

        if (gameInputPrefab != null) _gameInput = Instantiate(gameInputPrefab);
        if (mainMenuManagerPrefab != null) _mainMenuManager = Instantiate(mainMenuManagerPrefab);
        if (consolePrefab != null) _console = Instantiate(consolePrefab);
        if (playerSpawnerPrefab != null) _playerSpawner = Instantiate(playerSpawnerPrefab);
        if (enemySpawnerPrefab != null) _enemySpawner = Instantiate(enemySpawnerPrefab);
        if (pauseManagerPrefab != null) _pauseManager = Instantiate(pauseManagerPrefab);
        if (sceneLoaderPrefab != null) _sceneLoader = Instantiate(sceneLoaderPrefab);

        Initialized();
    }

    private void Initialized()
    {
        AudioManager.Instance?.Initialize();
        PoolManager.Instance?.Initialize();
        UIManager.Instance?.Initialize();
        
        _enemySpawner?.Initialize();
        _mainMenuManager?.Initialize();
        _pauseManager?.Initialize();
        _playerSpawner?.Initialize();
        _console?.Initialize();
        _sceneLoader?.Initialize();
        _gameInput.Initialize(_console,_pauseManager);
    }
}
