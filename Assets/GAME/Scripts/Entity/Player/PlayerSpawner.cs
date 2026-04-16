using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] playerPrefab;
    [SerializeField] private float radius = 15f;
    [SerializeField] private LayerMask groundLayer;
    private GameObject playerObj;

    public void Initialize()
    {
        if (G.GameFlow.IsFirstScene) SpawnFirstStart();
        G.GameFlow.OnEndGame += KillPlayer;

        ConsoleEvents.OnCommandPlayerSpawn += SpawnPlayerCommand;
        ConsoleEvents.OnCommandKillPlayer += KillPlayer;
        ConsoleEvents.OnCommandImmortalPlayer += ImmortalPlayer;
    }

    private void SpawnFirstStart()
    {
        KillPlayer();

        SpawnPlayer(PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCharacter));
    }


    private void SpawnPlayerCommand(int id = 0)
    {
        KillPlayer();

        SpawnPlayer(id);
    }

    private void SpawnPlayer(int id = 0)
    {
        if (playerPrefab == null || id < 0 || id >= playerPrefab.Length || playerPrefab[id] == null)
        {
            ConsoleEvents.ConsoleMessage($"Invalid player prefab: id={id}");

        }

        playerObj = Instantiate(playerPrefab[id], FindAndSpawn(), Quaternion.identity);

        if (playerObj.TryGetComponent<PlayerManager>(out var playerManager))
        {
            playerManager.Initialize();
            ConsoleEvents.ConsoleMessage($"Spawn player with id={id}");
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
        if (playerObj == null) return;

        Health health = playerObj.GetComponentInChildren<Health>();

        if (health != null) health.Kill();
        else Destroy(playerObj);
    }

    private void ImmortalPlayer(bool enable)
    {
        Health health = playerObj.GetComponentInChildren<Health>();
        if (health != null) health.ToggleImmortal(enable);
        else ConsoleEvents.ConsoleMessage("Helath not found");
    }


    private void OnDestroy()
    {
        G.GameFlow.OnEndGame -= KillPlayer;

        ConsoleEvents.OnCommandPlayerSpawn -= SpawnPlayerCommand;
        ConsoleEvents.OnCommandKillPlayer -= KillPlayer;
        ConsoleEvents.OnCommandImmortalPlayer -= ImmortalPlayer;
    }
}