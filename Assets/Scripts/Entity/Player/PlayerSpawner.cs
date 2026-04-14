using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] playerPrefab;
    [SerializeField] private float radius = 15f;
    [SerializeField] private LayerMask groundLayer;
    private GameObject player;

    public event Action<GameObject> OnPlayerSpawn;

    public void Initialize()
    {
        GameEvents.OnStartGame += OnStartGameHandler;
        GameEvents.OnCommandPlayerSpawn += SpawnPlayerCommand;
        GameEvents.OnCommandKillPlayer += KillPlayer;
        GameEvents.OnImmortalPlayer += ImmortalPlayer;
        GameEvents.OnEndGame += KillPlayer;
    }

    private void OnStartGameHandler()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SpawnFisrtStart();
    }

    public void SpawnFisrtStart()
    {
        KillPlayer();

        SpawnPlayer(PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCharacter));

        OnPlayerSpawn?.Invoke(player);
    }

    private void SpawnPlayerCommand(int id = 0)
    {
        KillPlayer();

        SpawnPlayer(id);

        OnPlayerSpawn?.Invoke(player);
    }

    private void SpawnPlayer(int id = 0)
    {
        if (playerPrefab == null || id < 0 || id >= playerPrefab.Length || playerPrefab[id] == null)
        {
            GameEvents.ConsoleMessage($"Invalid player prefab: id={id}");

        }

        player = Instantiate(playerPrefab[id], FindAndSpawn(), Quaternion.identity);

        if (player.TryGetComponent<PlayerManager>(out var playerManager))
        {
            playerManager.Initialize();
            GameEvents.ConsoleMessage($"Spawn player with id={id}");
        }
    }

    private Vector3 FindAndSpawn()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = transform.position + UnityEngine.Random.insideUnitSphere * radius;
            randomPos.y += 200f;

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 500f, groundLayer))
            {
                Vector3 spawnPos = hit.point;
                return spawnPos;
            }
        }

        return Vector3.zero;
    }


    //---Команды для игрока---

    private void KillPlayer()
    {
        if (player == null) return;

        Health health = player.GetComponentInChildren<Health>();

        if (health != null) health.Kill();
        else Destroy(player);
    }

    private void ImmortalPlayer(bool enable)
    {
        Health health = player.GetComponentInChildren<Health>();
        if (health != null) health.ToggleImmortal(enable);
        else GameEvents.ConsoleMessage("Helath not found");
    }


    private void OnDestroy()
    {
        GameEvents.OnStartGame -= OnStartGameHandler;
        GameEvents.OnCommandPlayerSpawn -= SpawnPlayerCommand;
        GameEvents.OnCommandKillPlayer -= KillPlayer;
        GameEvents.OnImmortalPlayer -= ImmortalPlayer;
        GameEvents.OnEndGame -= KillPlayer;
    }
}