using System;

public static class PlayerService
{
    public static PlayerManager Player { get; private set; }

    public static event Action<PlayerManager> OnPlayerChanged;
    public static event Action OnKill;
    public static event Action<float> OnDamageTaken;
    public static event Action<float> OnDamageDelta;
    public static event Action OnHit;

    public static void SetPlayer(PlayerManager player)
    {
        Player = player;
        OnPlayerChanged?.Invoke(player);
    }

    public static void ClearPlayer()
    {
        Player = null;
    }

    public static void Kill() => OnKill?.Invoke();
    public static void DamageTaken(float dmg) => OnDamageTaken?.Invoke(dmg);
    public static void DamageDelta(float dmg) => OnDamageDelta?.Invoke(dmg);
    public static void Hit() => OnHit?.Invoke();
}
