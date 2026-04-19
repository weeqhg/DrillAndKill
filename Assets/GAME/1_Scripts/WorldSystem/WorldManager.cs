using System;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public SceneType sceneType = SceneType.Arena;
    private GameDirector gameDirector;
    private LaunchWorld launchWorld;
    public event Action OnBossDefeated;



    public void Initialize()
    {
        gameDirector = SystemInitializer.InitializeSystem<GameDirector>(transform);
        launchWorld = SystemInitializer.InitializeSystem<LaunchWorld>(transform);

        SystemInitializer.InitializeSystem<SpawnObjectWorld>(transform);
        SystemInitializer.InitializeSystem<PlayerSpawner>(transform);

        StartWorld();
    }

    public void StartWorld()
    {
        launchWorld?.LaunchPlayerInWorld(sceneType);
    }

    public void CallBoerNextWorld()
    {
        launchWorld?.CallPepelats();
    }

    public void CallBossWorld()
    {
        gameDirector?.SpawnBoss();
    }

    public void BossDefeated()
    {
        gameDirector?.BossEnd();
        launchWorld?.CallPepelats();
        OnBossDefeated?.Invoke();
    }
}