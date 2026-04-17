using System;

public static class ConsoleEvents
{
    public static event Action<string> OnConsoleMessage;
    public static event Action<int> OnCommandPlayerSpawn;
    public static event Action<int, int> OnCommandEnemySpawn;
    public static event Action OnCommandLaunchPepelats;
    public static event Action<int, int> OnCommandObjectSpawn;
    public static event Action OnCommandKillPlayer;
    public static event Action OnCommandKillAllEnemy;
    public static event Action<bool> OnCommandPlayerFly;
    public static event Action<bool> OnCommandFreezeGame;
    public static event Action<bool> OnCommandToggleDifficultyScaler;
    public static event Action<int> OnCommandExp;
    public static event Action<int> OnCommandCoin;
    public static event Action OnCommandResetSkillTree;
    public static event Action<int> OnCommandTalentPoints;
    public static event Action OnCommandToggleSkillTree;
    public static event Action OnCommandToggleLevelTree;
    public static event Action<bool> OnCommandImmortalPlayer;
    public static event Action<float> OnSensitivityChanged;

    public static void ConsoleMessage(string message)
    {
        OnConsoleMessage?.Invoke(message);
    }

    public static void CommandPlayerSpawn(int id = 0)
    {
        OnCommandPlayerSpawn?.Invoke(id);
    }

    public static void CommandEnemySpawn(int id = 0, int count = 0)
    {
        OnCommandEnemySpawn?.Invoke(id, count);
    }

    public static void CommandLaunchPepelats()
    {
        OnCommandLaunchPepelats?.Invoke();
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

    public static void CommandPlayerFly(bool value)
    {
        OnCommandPlayerFly?.Invoke(value);
    }

    public static void CommandFreezeGame(bool enable)
    {
        OnCommandFreezeGame?.Invoke(enable);
    }

    public static void CommandToggleDifficultyScaler(bool value)
    {
        OnCommandToggleDifficultyScaler?.Invoke(value);
    }

    public static void CommandExp(int value)
    {
        OnCommandExp?.Invoke(value);
    }
    public static void CommandCoin(int value)
    {
        OnCommandCoin?.Invoke(value);
    }
    public static void CommandResetSkillTree()
    {
        OnCommandResetSkillTree?.Invoke();
    }
    public static void CommandTalentPoints(int amount)
    {
        OnCommandTalentPoints?.Invoke(amount);
    }
    public static void CommandToggleSkillTree()
    {
        OnCommandToggleSkillTree?.Invoke();
    }
    public static void CommandToggleLevelTree()
    {
        OnCommandToggleLevelTree?.Invoke();
    }
    public static void CommandImmortalPlayer(bool value)
    {
        OnCommandImmortalPlayer?.Invoke(value);
    }
    public static void CommandSensitivityChanged(float value)
    {
        OnSensitivityChanged?.Invoke(value);
    }
}