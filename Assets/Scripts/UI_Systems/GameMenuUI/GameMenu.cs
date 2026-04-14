using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenu : MonoBehaviour
{
    private GameMenuUI gameMenuUI;
    public void Initialize()
    {
        GameEvents.OnTogglePause += TogglePauseHandler;
        gameMenuUI = GetComponentInChildren<GameMenuUI>(true);
        gameMenuUI.Initialize();

        InputManager.Instance.Actions.UI.ESC.performed += OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (UIManager.Instance.HasAnyWindow())
        {
            UIManager.Instance.CloseTop();
            return;
        }

        if (UIManager.Instance.IsOpen<GameMenuUI>())
        {
            UIManager.Instance.Close(gameMenuUI);
        }
        else
        {
            UIManager.Instance.Open(gameMenuUI);
        }
    }

    private void TogglePauseHandler(bool enable)
    {
        GamePause.SetPaused(enable);
    }

    private void OnDestroy()
    {
        InputManager.Instance.Actions.UI.ESC.performed -= OnCancel;
        GameEvents.OnTogglePause -= TogglePauseHandler;
    }
}
