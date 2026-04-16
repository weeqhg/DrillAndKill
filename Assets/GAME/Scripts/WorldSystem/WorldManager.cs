using System;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    private SpawnObjectWorld spawnObjectWorld;
    private EnemySpawner enemySpawner;
    private GameDirector gameDirector;
    private PlayerSpawner playerSpawner;
    private LaunchWorld launchWorld;
    public event Action OnBossDefeated;
    public void Initialize()
    {
        spawnObjectWorld = gameObject.GetComponentInChildren<SpawnObjectWorld>();
        spawnObjectWorld?.Initialize();

        gameDirector = gameObject.GetComponentInChildren<GameDirector>();
        gameDirector?.Initialize();

        playerSpawner = gameObject.GetComponentInChildren<PlayerSpawner>();
        playerSpawner?.Initialize();

        launchWorld = gameObject.GetComponentInChildren<LaunchWorld>();
        launchWorld?.Initialize();
    }

    public void CallBoerNextWorld()
    {
        launchWorld?.CallBoer();
    }

    public void CallBossWorld()
    {
        gameDirector?.SpawnBoss();
    }

    public void BossDefeated()
    {
        gameDirector?.BossEnd();
        launchWorld?.CallBoer();
        OnBossDefeated?.Invoke();
    }
}
