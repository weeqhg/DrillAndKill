using UnityEngine;

public class PlayerSpawner : MonoBehaviour, IInitializable
{
    [SerializeField] private GameObject[] playerPrefab;
    [SerializeField] private float radius = 15f;



    public void Initialize()
    {
        ConsoleEvents.OnCommandPlayerSpawn += SpawnPlayerCommand;
        if (G.GameFlow.IsFirstScene) SpawnFirstStart();
        else RespawnPlayer();
    }

    private void SpawnFirstStart()
    {
        SpawnPlayer(PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCharacter));
    }

    private void SpawnPlayerCommand(int id = 0)
    {
        ConsoleEvents.CommandKillPlayer();
        SpawnPlayer(id);
    }

    private void RespawnPlayer()
    {
        Vector3 pos = SystemGet.GetGroundPosition(transform.position, radius);
        PlayerService.Player?.TeleportPlayer(pos);
    }

    private void SpawnPlayer(int id = 0)
    {
        if (playerPrefab == null || id < 0 || id >= playerPrefab.Length || playerPrefab[id] == null)
        {
            ConsoleEvents.ConsoleMessage($"Invalid player prefab: id={id}");
        }

        GameObject playerObj = Instantiate(playerPrefab[id], SystemGet.GetGroundPosition(transform.position, radius), Quaternion.identity);

        if (playerObj.TryGetComponent<PlayerManager>(out var playerManager))
        {
            playerManager.Initialize();
            ConsoleEvents.ConsoleMessage($"Spawn player with id={id}");
        }
    }

    private void OnDestroy()
    {
        ConsoleEvents.OnCommandPlayerSpawn -= SpawnPlayerCommand;
    }
}