using System;

public static class GameEvents
{
    public static event Action OnStartGame;
    public static event Action<int> OnCommandPlayerSpawn;
    public static event Action<int, int> OnCommandEnemySpawn;
    public static event Action<TypeEnemy, int> OnEnemySpawnWithType;
    public static event Action OnLaunchBoer;
    public static event Action<int, int> OnCommandObjectSpawn;
    public static event Action OnCommandKillPlayer;
    public static event Action OnCommandKillAllEnemy;
    public static event Action<string> OnConsoleMessage;
    public static event Action<float> OnSensitivityChanged;
    public static event Action<bool> OnCommandPlayerFly;
    public static event Action<bool> OnTogglePause;
    public static event Action<bool> OnDifficultyScalerCommand;
    public static event Action<int> OnCommandExp;
    public static event Action<int> OnCommandCoin;
    public static event Action OnCommandResetTree;
    public static event Action<int> OnCommandTalentPoints;
    public static event Action OnTriggerSkillTree;
    public static event Action OnTriggerLevelTree;
    public static event Action<SceneType> OnNextLevel;
    public static event Action<bool> OnImmortalPlayer;
    public static event Action OnBossStartFight;
    public static event Action OnBossDefeated;
    public static event Action OnEndGame;
    public static event Action OnEntityDie;
    public static event Action<float> OnDamageDealt;
    public static event Action<float> OnDamageTaken;
    public static void StartGame()
    {
        OnStartGame?.Invoke();
    }
    public static void CommandPlayerSpawn(int id = 0)
    {
        OnCommandPlayerSpawn?.Invoke(id);
    }
    public static void CommandEnemySpawn(int id = 0, int count = 0)
    {
        OnCommandEnemySpawn?.Invoke(id, count);
    }
    public static void EnemySpawnWithType(TypeEnemy typeEnemy, int count)
    {
        OnEnemySpawnWithType?.Invoke(typeEnemy, count);
    }
    public static void BoerLaunch()
    {
        OnLaunchBoer?.Invoke();
    }
    public static void CommandObjectSpawn(int id = 0, int count = 0)
    {
        OnCommandObjectSpawn?.Invoke(id, count);
    }
    public static void CommandKillPlayer()
    {
        OnCommandKillPlayer?.Invoke();
    }

    public static void CommandKillAllEnemy()
    {
        OnCommandKillAllEnemy?.Invoke();
    }

    public static void ConsoleMessage(string message)
    {
        OnConsoleMessage?.Invoke(message);
    }

    public static void SensitivityChanged(float value)
    {
        OnSensitivityChanged?.Invoke(value);
    }

    public static void CommandPlayerFly(bool value)
    {
        OnCommandPlayerFly?.Invoke(value);
    }
    public static void CommandExp(int value)
    {
        OnCommandExp?.Invoke(value);
    }
    public static void CommandCoin(int value)
    {
        OnCommandCoin?.Invoke(value);
    }
    public static void CommandResetTree()
    {
        OnCommandResetTree?.Invoke();
    }
    public static void CommandTalentPoints(int amount)
    {
        OnCommandTalentPoints?.Invoke(amount);
    }
    public static void TogglePause(bool enable)
    {
        OnTogglePause?.Invoke(enable);
    }
    public static void TriggerSkillTree()
    {
        OnTriggerSkillTree?.Invoke();
    }
    public static void TriggerLevelTree()
    {
        OnTriggerLevelTree?.Invoke();
    }
    public static void ImmortalPlayer(bool value)
    {
        OnImmortalPlayer?.Invoke(value);
    }
    public static void BossStartFight()
    {
        OnBossStartFight?.Invoke();
    }
    public static void BossDefeated()
    {
        OnBossDefeated?.Invoke();
    }
    public static void NextLevel(SceneType sceneType)
    {
        OnNextLevel?.Invoke(sceneType);
    }
    public static void EndGame()
    {
        OnEndGame?.Invoke();
    }
    public static void EntityDie()
    {
        OnEntityDie?.Invoke();
    }
    public static void DamageDealt(float value)
    {
        OnDamageDealt?.Invoke(value);
    }
    public static void DamageTaken(float value)
    {
        OnDamageTaken?.Invoke(value);
    }
    public static void DifficultyScalerCommand(bool value)
    {
        OnDifficultyScalerCommand?.Invoke(value);
    }

}