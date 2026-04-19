using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour, IInitializable
{
    private Stack<IUIWindow> windowStack = new Stack<IUIWindow>();



    public void Initialize()
    {
        if (G.UIManager != null && G.UIManager != this)
        {
            Destroy(gameObject);
            return;
        }

        G.UIManager = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // 🔥 OPEN
    // =========================
    public void OpenOverlay(IUIWindow window)
    {
        Open(window, false);
    }

    public void Open(IUIWindow window, bool hidePrevious = true)
    {
        if (window == null) return;

        if (windowStack.Contains(window))
        {
            BringToTop(window);
            return;
        }

        // 🔥 скрываем предыдущее только если нужно
        if (hidePrevious && windowStack.Count > 0)
        {
            windowStack.Peek().Hide();
        }

        if (windowStack.Count == 0)
        {
            G.InputManager?.PushInput(InputType.UI);
            GamePause.SetPaused(true);
        }

        windowStack.Push(window);
        window.Show();
    }

    // =========================
    // 🔥 CLOSE TOP
    // =========================
    public void CloseTop()
    {
        if (windowStack.Count == 0) return;

        IUIWindow top = windowStack.Peek();

        if (top is ICloseBlocker)
            return;

        top = windowStack.Pop();
        top.Hide();

        if (windowStack.Count > 0)
        {
            windowStack.Peek().Show(); // 🔥 возвращаем предыдущее
        }
        else
        {
            G.InputManager?.PopInput();
            GamePause.SetPaused(false);
        }
    }

    // =========================
    // 🔥 CLOSE SPECIFIC
    // =========================
    public void Close(IUIWindow window)
    {
        if (!windowStack.Contains(window)) return;

        var temp = new Stack<IUIWindow>();

        while (windowStack.Count > 0)
        {
            var w = windowStack.Pop();

            if (w == window)
            {
                w.Hide();
                break;
            }

            temp.Push(w);
        }

        while (temp.Count > 0)
        {
            windowStack.Push(temp.Pop());
        }

        if (windowStack.Count == 0)
        {
            G.InputManager?.PopInput();
            GamePause.SetPaused(false);
        }
    }

    // =========================
    // 🔥 BRING TO TOP
    // =========================
    private void BringToTop(IUIWindow window)
    {
        var temp = new Stack<IUIWindow>();

        while (windowStack.Peek() != window)
        {
            temp.Push(windowStack.Pop());
        }

        // окно уже наверху
        var target = windowStack.Pop();

        while (temp.Count > 0)
        {
            windowStack.Push(temp.Pop());
        }

        windowStack.Push(target);
    }

    // =========================
    // 🔥 STATE CHECKS
    // =========================
    public bool IsOpen(IUIWindow window)
    {
        return windowStack.Contains(window);
    }

    public bool IsOpen<T>() where T : class, IUIWindow
    {
        return windowStack.Any(w => w is T);
    }

    public bool HasAnyWindow()
    {
        return windowStack.Count > 0;
    }

    public IUIWindow GetTop()
    {
        return windowStack.Count > 0 ? windowStack.Peek() : null;
    }

    // =========================
    // 🔥 FORCE CLOSE ALL
    // =========================
    public void CloseAll()
    {
        while (windowStack.Count > 0)
        {
            var w = windowStack.Pop();
            w.Hide();
        }

        G.InputManager?.PopInput();
        GamePause.SetPaused(false);
    }
}