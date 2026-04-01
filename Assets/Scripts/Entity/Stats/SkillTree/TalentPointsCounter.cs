using System;
using UnityEngine;

public class TalentPointsCounter : MonoBehaviour
{
    public int Points { get; private set; } = 0;

    public void Initialize()
    {
        LoadProgress();
        GameEvents.OnCommandTalentPoints += AddPoints;
    }

    public void AddPoints(int amount)
    {
        if (amount == 0) return;

        Points = Mathf.Max(0, Points + amount);;

        SaveProgress();
        OnPointsChanged?.Invoke(Points);
    }

    public void RemovePoints(int amount)
    {
        Points = Mathf.Max(0, Points - amount);

        SaveProgress();
        OnPointsChanged?.Invoke(Points);
    }

    public event Action<int> OnPointsChanged;

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(PlayerPrefsKeys.SkillTreePoints, Points);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        Points = PlayerPrefs.GetInt(PlayerPrefsKeys.SkillTreePoints, 0);

        OnPointsChanged?.Invoke(Points);
    }

    private void OnDestroy()
    {
        GameEvents.OnCommandTalentPoints -= AddPoints;
    }
}
