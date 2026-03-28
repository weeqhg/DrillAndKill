using System;

public static class GamePause
{
    public static event Action<bool> OnPauseGame;
    public static event Action<bool> OnFrozenGame;
    public static bool IsGamePaused { get; private set; }
    public static bool IsGameFrozen { get; private set; }

    public static void SetPaused(bool paused)
    {
        IsGamePaused = paused;
        OnPauseGame?.Invoke(paused);
    }

    public static void SetFrozen(bool frozen)
    {
        IsGameFrozen = frozen;
        OnFrozenGame?.Invoke(frozen);
    }
}
