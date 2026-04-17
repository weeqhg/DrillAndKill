using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum TypeEnemy
{
    All,
    Default,
    Elite,
    Boss
}

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] allEnemyPrefabs;
    public GameObject[] enemyDefaultPrefabs;
    public GameObject[] enemyElitePrefabs;
    public GameObject[] enemyBossPrefabs;

    [SerializeField] private float safeRadius = 15f;
    [SerializeField] private float radius = 15f;
    [SerializeField] private LayerMask groundLayer;
    private PlayerManager player;
    private List<EnemyManager> _allEnemyManagers = new List<EnemyManager>();
    private int currentLevel = 1;
    private int expReward = 0;
    private int coinRewar = 0;

    private List<EnemyManager> _bosses = new List<EnemyManager>();

    public void Initialize()
    {
        ConsoleEvents.OnCommandEnemySpawn += SpawnEnemiesId;
        ConsoleEvents.OnCommandKillAllEnemy += KillAllEnemies;

        if (PlayerService.Player != null) SetPlayer(PlayerService.Player);
        PlayerService.OnPlayerChanged += SetPlayer;
    }

    private void SetPlayer(PlayerManager player)
    {
        this.player = player;
    }

    private void SpawnEnemiesId(int id, int count = 0)
    {
        GameObject enemy = allEnemyPrefabs[id];

        if (enemy == null)
        {
            ConsoleEvents.ConsoleMessage($"Invalid enemy prefab id {id}");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(TypeEnemy.All, enemy);
        }
    }

    public void SpawnEnemies(int count, TypeEnemy typeEnemy, int levelDiffculty = 1, int exp = 0, int coins = 0)
    {
        currentLevel = levelDiffculty;
        expReward = exp;
        coinRewar = coins;
        SpawnEnemyType(typeEnemy, count);
    }

    private void SpawnEnemyType(TypeEnemy typeEnemy, int count)
    {
        GameObject prefab = typeEnemy switch
        {
            TypeEnemy.Default => enemyDefaultPrefabs[Random.Range(0, enemyDefaultPrefabs.Length)],
            TypeEnemy.Elite => enemyElitePrefabs[Random.Range(0, enemyElitePrefabs.Length)],
            TypeEnemy.Boss => enemyBossPrefabs[Random.Range(0, enemyBossPrefabs.Length)],
            _ => null
        };

        SpawnEnemiesType(typeEnemy, prefab, count);
    }

    private void SpawnEnemiesType(TypeEnemy type, GameObject enemyPrefab, int count = 0)
    {
        if (enemyPrefab == null)
        {
            ConsoleEvents.ConsoleMessage($"Invalid enemy prefab");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(type, enemyPrefab);
        }
    }


    private void SpawnEnemy(TypeEnemy type, GameObject enemyPrefab)
    {
        float enemyRadius = 0.5f;
        float enemyHeight = 2f;

        if (enemyPrefab.TryGetComponent<CapsuleCollider>(out var capsule))
        {
            enemyRadius = capsule.radius;
            enemyHeight = capsule.height;
        }

        if (!TryGetSpawnPosition(out Vector3 spawnPos, enemyRadius: 0.5f, enemyHeight: 2f)) return;

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        if (enemy.TryGetComponent<EnemyManager>(out var enemyManager))
        {
            enemyManager.Initialize(currentLevel, expReward, coinRewar);
            enemyManager.OnEnemyDied += RemoveEnemy;
            _allEnemyManagers.Add(enemyManager);
        }

        if (type == TypeEnemy.Boss)
        {
            _bosses.Add(enemyManager);
        }
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition, float enemyRadius = 0.5f, float enemyHeight = 2f)
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            float distance = Random.Range(safeRadius, radius);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            Vector3 randomPos;

            if (player != null)
            {
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                randomPos = player.Transform.position + offset + Vector3.up * 10f;
            }
            else
            {
                randomPos = transform.position + Random.insideUnitSphere * radius;
            }

            if (Physics.Raycast(randomPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 30f, groundLayer))
            {
                // Привязка к NavMesh
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    Vector3 candidatePos = navHit.position;

                    // Проверка коллайдеров, чтобы не спавниться внутри объектов
                    if (Physics.CheckCapsule(candidatePos, candidatePos + Vector3.up * enemyHeight, enemyRadius, ~groundLayer, QueryTriggerInteraction.Ignore))
                        continue; // место занято

                    // Поднимаем объект на половину высоты (чтобы не проваливался)
                    spawnPosition = candidatePos + Vector3.up * (enemyHeight / 2f);
                    return true;
                }
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    public void RemoveEnemy(EnemyManager enemyManager)
    {
        if (enemyManager == null) return;

        enemyManager.OnEnemyDied -= RemoveEnemy;

        _allEnemyManagers.Remove(enemyManager);

        if (_bosses.Contains(enemyManager))
        {
            _bosses.Remove(enemyManager);

            if (_bosses.Count == 0)
            {
                G.WorldManager?.BossDefeated();
            }
        }

        PlayerService.Kill();
    }

    private void KillAllEnemies()
    {
        for (int i = _allEnemyManagers.Count - 1; i >= 0; i--)
        {
            EnemyManager enemyManager = _allEnemyManagers[i];
            if (enemyManager != null)
            {
                Health health = enemyManager.GetComponent<Health>();
                enemyManager.OnEnemyDied -= RemoveEnemy;
                if (health != null) health.Kill();
                else Destroy(enemyManager.gameObject);
            }
        }

        _allEnemyManagers.Clear();
    }

    private void OnDestroy()
    {
        PlayerService.OnPlayerChanged -= SetPlayer;

        ConsoleEvents.OnCommandEnemySpawn -= SpawnEnemiesId;
        ConsoleEvents.OnCommandKillAllEnemy -= KillAllEnemies;

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
