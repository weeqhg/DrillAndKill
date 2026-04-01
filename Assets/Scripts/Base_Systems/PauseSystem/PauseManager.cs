using UnityEngine;


public class PauseManager : MonoBehaviour
{
    public void Initialize()
    {
        GameEvents.OnTogglePause += TogglePauseHandler;


        GameMenu gameMenu = GetComponentInChildren<GameMenu>();
        gameMenu.Initialize();
        SettingManager settingManager = GetComponentInChildren<SettingManager>();
        settingManager.Initialize();
    }

    private void TogglePauseHandler(bool enable)
    {
        GamePause.SetPaused(enable);
    }

    private void OnDestroy()
    {
       GameEvents.OnTogglePause -= TogglePauseHandler;
    }
}
