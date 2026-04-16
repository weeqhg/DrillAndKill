using System;

public class PlayerService
{
    private static PlayerManager _player;

    public static PlayerManager Player
    {
        get => _player;
        set
        {
            _player = value;
            OnPlayerChanged?.Invoke(value);
        }
    }

    public static event Action<PlayerManager> OnPlayerChanged;
}
