using System;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public event Action<EnemyManager> OnEnemyDied;
    public Transform player { get; private set; }
    public CameraShake cameraShake { get; private set; }
    public bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;
    public void Initialize(int levelDifficulty, int expReward, int coinReward)
    {
        StatsController statsController = GetComponentInChildren<StatsController>();
        EnemyAI ai = GetComponent<EnemyAI>();
        Health health = GetComponent<Health>();

        LevelStats levelStats = GetComponentInChildren<LevelStats>();
        levelStats.SetLevel(levelDifficulty);

        LootDropper lootDropper = GetComponentInChildren<LootDropper>();
        lootDropper.Initialize(expReward, coinReward);

        statsController.Initialize();
        health.Initialize();
        ai.Initialize();

        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            OnPlayerReady(GameManager.Instance.Player);
        }
        GameManager.Instance.OnPlayerSpawned += OnPlayerReady;
    }

    private void OnPlayerReady(GameObject player)
    {
        this.player = player.transform;
        cameraShake = player.GetComponentInChildren<CameraShake>();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerSpawned -= OnPlayerReady;

        OnEnemyDied?.Invoke(this);

        OnEnemyDied = null;
    }
}
