using System.Collections.Generic;
using UnityEngine;

public enum InputType { Player, UI }

public class InputManager : MonoBehaviour, IInitializable
{
    public InputSystem_Actions Actions => _actions;
    private InputSystem_Actions _actions;
    private Stack<InputType> inputStack = new Stack<InputType>();



    public void Initialize()
    {
        if (G.InputManager != null && G.InputManager != this)
        {
            Destroy(gameObject);
            return;
        }


        _actions = new InputSystem_Actions();
        _actions.Enable();

        SetInput(InputType.Player);

        G.InputManager = this;
        DontDestroyOnLoad(gameObject);
    }

    private void SetInput(InputType type)
    {
        switch (type)
        {
            case InputType.Player:
                _actions.Player.Enable();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case InputType.UI:
                _actions.Player.Disable();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    public void PushInput(InputType type)
    {
        inputStack.Push(type);
        SetInput(type);
    }

    public void PopInput()
    {
        if (inputStack.Count == 0)
        {
            SetInput(InputType.Player);
            return;
        }

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

    public void ResetInput()
    {
        inputStack.Clear();
        SetInput(InputType.Player);
    }

    private void OnDestroy()
    {
        if (_actions != null)
        {
            _actions.Disable();
            _actions.Dispose();
            _actions = null;
        }
    }
}


