using System;

public static class GameEvents
{
    public static event Action<string> OnGameStart;
    public static event Action<int> OnCommandPlayerSpawn;
    public static event Action<int, int> OnCommandEnemySpawn;
    public static event Action<PlayerManager> OnPlayerSpawned;
    public static event Action OnCommandKillAllEnemy;
    public static event Action<string> OnConsoleMessage;
    public static event Action<float> OnSensitivityChanged;
    public static event Action<bool> OnCommandPlayerFly;
    public static event Action<bool> OnTogglePause;
    public static event Action<int> OnCommandExp;
    public static event Action OnCommandResetTree;
    public static event Action<int> OnCommandTalentPoints;
    public static event Action OnTriggerTreePanel;
    public static void GameStart(string sceneName)
    {
        OnGameStart?.Invoke(sceneName);
    }
    public static void CommandPlayerSpawn(int id = 0)
    {
        OnCommandPlayerSpawn?.Invoke(id);
    }
    public static void CommandEnemySpawn(int id = 0, int count = 0)
    {
        OnCommandEnemySpawn?.Invoke(id, count);
    }
    public static void PlayerSpawned(PlayerManager player)
    {
        OnPlayerSpawned?.Invoke(player);
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
    public static void TriggerTree()
    {
        OnTriggerTreePanel?.Invoke();
    }

}