using System;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public event Action<EnemyManager> OnEnemyDied;
    public void Initialize(int levelDifficulty, int expReward, int coinReward)
    {
        StatsController statsController = GetComponentInChildren<StatsController>();
        EnemyAI ai = GetComponent<EnemyAI>();
        Health health = GetComponent<Health>();

        LevelStats levelStats = GetComponentInChildren<LevelStats>();
        levelStats.SetLevel(levelDifficulty);

        LootDropper lootDropper = GetComponentInChildren<LootDropper>();
        lootDropper.SetReward(expReward, coinReward);

        statsController.Initialize();
        health.Initialize();
        ai.Initialize();
    }

    

    private void OnDestroy()
    {
        OnEnemyDied?.Invoke(this);

        OnEnemyDied = null;
    }
}
