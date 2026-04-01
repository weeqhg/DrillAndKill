using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;

    [SerializeField] private float radius = 15f;
    [SerializeField] private LayerMask groundLayer;
    private Transform _player;
    private List<EnemyManager> _allEnemyManagers = new List<EnemyManager>();

    public void Initialize()
    {
        GameEvents.OnPlayerSpawned += OnPlayerSpawnedHandler;
        GameEvents.OnCommandEnemySpawn += SpawnEnemies;
        GameEvents.OnCommandKillAllEnemy += KillAllEnemies;
    }

    private void OnPlayerSpawnedHandler(PlayerManager player)
    {
        _player = player.transform;
    }

    public void SpawnEnemies(int id = 0, int count = 0)
    {
        if (enemyPrefabs == null || id < 0 || id >= enemyPrefabs.Length || enemyPrefabs[id] == null)
        {
            GameEvents.ConsoleMessage($"Invalid enemy prefab: id={id}");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(id);
        }

    }
    private void SpawnEnemy(int id = 0)
    {
        if (!TryGetSpawnPosition(out Vector3 spawnPosition)) return;

        GameObject enemy = Instantiate(enemyPrefabs[id], spawnPosition, Quaternion.identity);

        if (enemy.TryGetComponent<EnemyManager>(out var enemyManager))
        {
            enemyManager.Initialize(_player);
            enemyManager.OnEnemyDied += RemoveEnemy;
            _allEnemyManagers.Add(enemyManager);
        }
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * radius;

            if (Physics.Raycast(randomPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 30f, groundLayer) &&
                NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                spawnPosition = navHit.position;
                return true;
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    public void RemoveEnemy(EnemyManager enemyManager)
    {
        if (enemyManager != null && _allEnemyManagers.Contains(enemyManager))
        {
            enemyManager.OnEnemyDied -= RemoveEnemy; // Отписываемся
            _allEnemyManagers.Remove(enemyManager);
        }
    }

    private void KillAllEnemies()
    {
        for (int i = _allEnemyManagers.Count - 1; i >= 0; i--)
        {
            EnemyManager enemyManager = _allEnemyManagers[i];
            if (enemyManager != null)
            {
                enemyManager.OnEnemyDied -= RemoveEnemy;
                Destroy(enemyManager.gameObject);
            }
        }

        _allEnemyManagers.Clear();
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerSpawned -= OnPlayerSpawnedHandler;
        GameEvents.OnCommandEnemySpawn -= SpawnEnemies;

        foreach (EnemyManager enemyManager in _allEnemyManagers)
        {
            if (enemyManager != null)
            {
                enemyManager.OnEnemyDied -= RemoveEnemy;
            }
        }

        _allEnemyManagers.Clear();
    }
}
