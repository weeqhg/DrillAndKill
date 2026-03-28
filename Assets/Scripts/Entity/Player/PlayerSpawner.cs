using System;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] playerPrefab;
    [SerializeField] private float radius = 15f;
    [SerializeField] private LayerMask groundLayer;
    public bool isSpawnPlayer = true;
    public void Initialize()
    {
        GameEvents.OnCommandPlayerSpawn += SpawnPlayer;

        if (isSpawnPlayer) SpawnPlayer(PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCharacter));
    }

    private void SpawnPlayer(int id = 0)
    {
        if (playerPrefab == null || id < 0 || id >= playerPrefab.Length || playerPrefab[id] == null)
        {
            GameEvents.ConsoleMessage($"Invalid player prefab: id={id}");
            return;
        }

        GameObject gameObject = Instantiate(playerPrefab[id], FindAndSpawn(), Quaternion.identity);

        if (gameObject.TryGetComponent<PlayerManager>(out var playerManager))
        {
            playerManager.Initialize();
            GameEvents.ConsoleMessage($"Spawn player with id={id}");
            GameEvents.PlayerSpawned(playerManager);
        }
        else
        {
            GameEvents.ConsoleMessage("PlayerManager missing!");
            Destroy(gameObject);
        }
    }

    private Vector3 FindAndSpawn()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = transform.position + UnityEngine.Random.insideUnitSphere * radius;
            randomPos.y += 10f;

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 30f, groundLayer))
            {
                return hit.point;
            }
        }

        return Vector3.zero;
    }

    private void OnDestroy()
    {
        GameEvents.OnCommandPlayerSpawn -= SpawnPlayer;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}