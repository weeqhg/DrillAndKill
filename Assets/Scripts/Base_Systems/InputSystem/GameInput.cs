using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private Console consoleWindow;
    private GameMenu gameMenuWindow;
    private InputManager _input;

    public void Initialize(Console console, PauseManager pauseManager)
    {
        if (console != null) consoleWindow = console;
        if (pauseManager != null) gameMenuWindow = pauseManager.gameObject.GetComponentInChildren<GameMenu>(true);
        _input = InputManager.Instance;

        _input.Actions.UI.Console.performed += OnConsole;

        _input.Actions.UI.ESC.performed += OnCancel;
    }

    #region Handlers

    private void OnConsole(InputAction.CallbackContext ctx)
    {
        if (UIManager.Instance.IsOpen<GameMenu>())
        {
            return;
        }
        if (UIManager.Instance.IsOpen<Console>())
        {
            UIManager.Instance.Close(consoleWindow);
        }
        else
        {
            UIManager.Instance.OpenOverlay(consoleWindow);
        }
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (UIManager.Instance.HasAnyWindow())
        {
            UIManager.Instance.CloseTop();
            return;
        }

        if (UIManager.Instance.IsOpen<GameMenu>())
        {
            UIManager.Instance.Close(gameMenuWindow);
        }
        else
        {
            UIManager.Instance.Open(gameMenuWindow);
        }
    }


    #endregion

    private void OnDestroy()
    {
        _input.Actions.UI.Console.performed -= OnConsole;
        _input.Actions.UI.ESC.performed -= OnCancel;
    }
}