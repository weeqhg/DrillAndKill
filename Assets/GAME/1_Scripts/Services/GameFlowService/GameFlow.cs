using System;
using UnityEngine;
using TMPro;

public class GameFlow : MonoBehaviour, IInitializable
{
    [Header("Reference")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI timerText;

    //Геттеры
    public float GameTIME { get; private set; } = 0f;
    public float DifficultyMultiplier => difficulty.Multiplier;
    public int DifficultyLevel => difficulty.Level;
    public bool IsFirstScene { get; private set; } = false;

    //События
    public event Action OnResetProgress;
    public event Action<SceneType> OnNextScene;
    public event Action OnEndScene;
    public event Action OnEndGame;
    public event Action<float> OnTimerUpdate;

    //Компонетны
    private SceneLoader sceneLoader;
    private LevelTree levelTree;
    private DifficultyManager difficulty;

    private bool isTimerRun = false;
    private bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;



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

        difficulty = GetComponentInChildren<DifficultyManager>();
        difficulty?.Initialize();

        ConsoleEvents.OnCommandToggleDifficultyScaler += ConsoleStartRun;
        GamePause.OnPauseGame += ToggleHUD;
        ToggleHUD(GamePause.IsGamePaused);

        G.GameFlow = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (IsStoped) return;

        if (isTimerRun)
        {
            GameTIME += Time.deltaTime;
            difficulty?.UpdateTime();
            UpdateTimerDisplay(GameTIME);
            OnTimerUpdate?.Invoke(GameTIME);
        }
    }

    // =========================
    // Блок обрабатывающий внешние запросы
    // =========================
    public void StartGamePlay()
    {
        ResetTimer();
        IsFirstScene = true;

        sceneLoader.SceneHandler(SceneType.Arena);
        difficulty?.StartRun();
        levelTree?.ResetProgress();

        OnGameStateChanged(SceneType.Arena);
        OnResetProgress?.Invoke();
        OnEndScene?.Invoke();
    }

    public void NextHandler(SceneType sceneType)
    {
        IsFirstScene = false;

        sceneLoader.SceneHandler(sceneType);
        difficulty?.NextLevel(sceneType);

        OnGameStateChanged(sceneType);
        OnNextScene?.Invoke(sceneType);
        OnEndScene?.Invoke();
    }

    public void EndHandler()
    {
        IsFirstScene = false;

        sceneLoader.SceneHandler(SceneType.MainMenu);
        difficulty?.EndRun();

        OnGameStateChanged(SceneType.MainMenu);
        OnEndGame?.Invoke();
        OnEndScene?.Invoke();
    }

    public void ShowLevelTree()
    {
        levelTree.ShowTree();
    }

    // =========================
    // Доп. методы
    // =========================
    private void ConsoleStartRun(bool isEnabled)
    {
        if (isEnabled)
        {
            isTimerRun = true;
        }
        else
        {
            isTimerRun = false;
        }
    }

    private void OnGameStateChanged(SceneType state)
    {
        switch (state)
        {
            case SceneType.MainMenu:
                isTimerRun = false;
                HideHUD();
                break;

            case SceneType.Arena:
                isTimerRun = true;
                ShowHUD();
                break;

            case SceneType.Shop:
                isTimerRun = false;
                ShowHUD();
                break;

            case SceneType.Secret:
                isTimerRun = false;
                ShowHUD();
                break;

            case SceneType.Final:
                isTimerRun = false;
                ShowHUD();
                break;
        }
    }

    // =========================
    // UI
    // =========================
    private void ToggleHUD(bool value)
    {
        if (value)
            HideHUD();
        else
            ShowHUD();
    }

    private void ShowHUD()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    private void HideHUD()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void UpdateTimerDisplay(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);

        if (timeSpan.Hours > 0)
        {
            timerText.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        else
        {
            timerText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }

    // =========================
    // Reset
    // =========================
    private void ResetTimer()
    {
        GameTIME = 0f;
        OnTimerUpdate?.Invoke(GameTIME);
    }

    private void OnDestroy()
    {
        ConsoleEvents.OnCommandToggleDifficultyScaler -= ConsoleStartRun;
        GamePause.OnPauseGame -= ToggleHUD;
    }
}
