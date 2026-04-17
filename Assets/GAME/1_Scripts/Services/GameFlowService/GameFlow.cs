using System;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    private SceneLoader sceneLoader;
    private LevelTree levelTree;
    public bool IsFirstScene { get; private set; } = false;
    public event Action OnResetProgress;
    public event Action<SceneType> OnNextScene;
    public event Action OnEndScene;
    public event Action OnEndGame;

    public void Initialize()
    {
        if (G.GameFlow != null && G.GameFlow != this)
        {
            Destroy(gameObject);
            return;
        }

        sceneLoader = GetComponentInChildren<SceneLoader>();

        levelTree = GetComponentInChildren<LevelTree>();
        levelTree?.Initialize();

        G.GameFlow = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGamePlay()
    {
        IsFirstScene = true;
        sceneLoader.SceneHandler(SceneType.Arena);
        OnResetProgress?.Invoke();
        OnEndScene?.Invoke();
    }

    public void NextHandler(SceneType sceneType)
    {
        IsFirstScene = false;
        sceneLoader.SceneHandler(sceneType);
        OnNextScene?.Invoke(sceneType);
        OnEndScene?.Invoke();
    }

    public void EndHandler()
    {
        IsFirstScene = false;
        sceneLoader.SceneHandler(SceneType.MainMenu);
        OnEndGame?.Invoke();
        OnEndScene?.Invoke();
    }

    public void ShowLevelTree()
    {
        levelTree.ShowTree();
    }
}
