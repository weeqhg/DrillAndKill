using UnityEngine;


public class PauseManager : MonoBehaviour
{
    public void Initialize()
    {
        GameEvents.OnConsole += TogglePauseHandler;
        GameEvents.OnGameMenu += TogglePauseHandler;
    }

    private void TogglePauseHandler(bool enable)
    {
        GamePause.SetPaused(enable);
    }

    private void OnDestroy()
    {
        GameEvents.OnConsole -= TogglePauseHandler;
        GameEvents.OnGameMenu -= TogglePauseHandler;
    }
}
