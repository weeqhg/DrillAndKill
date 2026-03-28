using System;
using NUnit.Framework;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public event Action<EnemyManager> OnEnemyDied;
    public Transform player { get; private set; }
    public bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;
    public void Initialize(Transform player)
    {
        GameEvents.OnPlayerSpawned += OnPlayerSpawnedHandler;

        this.player = player;
        EnemyAI ai = GetComponent<EnemyAI>();
        Health health = GetComponent<Health>();

        health.Initialize();
        ai.Initialize();
    }

    private void OnPlayerSpawnedHandler(PlayerManager player)
    {
        this.player = player.transform;
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerSpawned -= OnPlayerSpawnedHandler;

        OnEnemyDied?.Invoke(this);

        OnEnemyDied = null;
    }
}
