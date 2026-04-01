using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputType
{
    Player,
    UI
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public InputSystem_Actions Actions => _actions;
    private InputSystem_Actions _actions;
    private Stack<InputType> inputStack = new Stack<InputType>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _actions = new InputSystem_Actions();
        _actions.Enable();

        SetInput(InputType.Player);
    }

    private void SetInput(InputType type)
    {
        switch (type)
        {
            case InputType.Player:
                _actions.Player.Enable();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameEvents.TogglePause(false);
                break;

            case InputType.UI:
                _actions.Player.Disable();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                GameEvents.TogglePause(true);
                break;
        }
    }

    public void PushInput(InputType type)
    {
        inputStack.Push(type);
        SetInput(type);
    }

    // 🔥 Возвращаемся к предыдущему
    public void PopInput()
    {
        if (inputStack.Count == 0)
        {
            SetInput(InputType.Player);
            return;
        }

        // удаляем текущее
        inputStack.Pop();

        if (inputStack.Count == 0)
        {
            SetInput(InputType.Player);
        }
        else
        {
            SetInput(inputStack.Peek());
        }
    }

    // (опционально) полный сброс
    public void ResetInput()
    {
        inputStack.Clear();
        SetInput(InputType.Player);
    }

    private void OnDestroy()
    {
        if (_actions != null)
            _actions.Disable();
    }
}


