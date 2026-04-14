using System;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public LevelTree LevelTree;
    public SceneLoader SceneLoader;
    public PlayerSpawner PlayerSpawner;
    public DifficultyManager difficultyManager;
    public GameObject Player { get; private set; }
    public event Action<GameObject> OnPlayerSpawned;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    private void Initialize()
    {
        PlayerSpawner.Initialize();
        PlayerSpawner.OnPlayerSpawn += OnSpawnPlayerHandler;

        LevelTree.Initialize();
        SceneLoader.Initialize();
        difficultyManager.Initialize();
    }

    private void OnSpawnPlayerHandler(GameObject player)
    {
        Player = player;
        OnPlayerSpawned?.Invoke(Player);
    }

    private void OnDestroy()
    {
        PlayerSpawner.OnPlayerSpawn -= OnSpawnPlayerHandler;
    }
}
