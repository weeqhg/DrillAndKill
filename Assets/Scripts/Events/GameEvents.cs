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
    public static event Action<bool> OnGameMenu;
    public static event Action<bool> OnConsole;
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
    public static void GameMenu(bool enable)
    {
        OnGameMenu?.Invoke(enable);
    }
    public static void Console(bool enable)
    {
        OnConsole?.Invoke(enable);
    }

}