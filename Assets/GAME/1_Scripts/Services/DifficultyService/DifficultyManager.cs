using System;
using TMPro;
using UnityEngine;

public struct DifficultySnapshot
{
    public float multiplier;
    public float time;
    public int level;
}

public class DifficultyManager : MonoBehaviour
{
    public float DifficultyMultiplier { get; private set; } = 1f;
    private bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;
    private float targetDifficulty = 1f;
    private float performanceScore = 1f;

    private float damageTaken;
    private float damageDealt;
    private float kills;

    private float stress;        // текущий стресс игрока
    private float panicCooldown; // защита от спама паники

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI timerText;
    private int difficulty = 0;
    public float timeDifficulty = 0f;
    private float timer;
    private bool isTimerRunning = false;
    public bool IsTimerRunning => isTimerRunning;

    public DifficultySnapshot GetDifficultySnapshot()
    {
        return new DifficultySnapshot
        {
            multiplier = DifficultyMultiplier,
            time = timeDifficulty,
            level = difficulty
        };
    }

    public event Action<float> OnTimerUpdate;
    public void Initialize()
    {
        if (G.DifficultyManager != null && G.DifficultyManager != this)
        {
            Destroy(gameObject);
            return;
        }

        G.GameFlow.OnResetProgress += OnStartHandler;
        G.GameFlow.OnNextScene += OnNextHandler;
        G.GameFlow.OnEndGame += OnEndHandler;

        PlayerService.OnDamageTaken += RegisterDamageTaken;

        ConsoleEvents.OnCommandToggleDifficultyScaler += OnCommandToggleDifficultyScaler;

        HideHUD();

        G.DifficultyManager = this;

        GamePause.OnPauseGame += ToggleHUD;
        ToggleHUD(GamePause.IsGamePaused);

        DontDestroyOnLoad(gameObject);
    }

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


    private void Update()
    {
        if (IsStoped) return;

        if (panicCooldown > 0f)
            panicCooldown -= Time.deltaTime;

        float speed = Mathf.Max(0.5f, Mathf.Abs(DifficultyMultiplier - targetDifficulty) * 2f);

        DifficultyMultiplier = Mathf.Lerp(
            DifficultyMultiplier,
            targetDifficulty,
            Time.deltaTime * speed
        );

        if (isTimerRunning)
        {
            timeDifficulty += Time.deltaTime;
            UpdateTimerDisplay(timeDifficulty);
            OnTimerUpdate?.Invoke(timeDifficulty);

            timer += Time.deltaTime;
            if (timer >= 2f) // обновляем каждые 2 секунды
            {
                Evaluate();
                ResetStats();
                timer = 0f;
                //Debug.Log($"Stress: {stress:F2} | Difficulty x{DifficultyMultiplier:F2}");
            }
        }
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


    private void OnStartHandler()
    {
        ResetDifficulty();

        OnGameStateChanged(SceneType.Arena);
    }

    private void OnNextHandler(SceneType sceneType)
    {
        if (sceneType == SceneType.Arena || sceneType == SceneType.Secret)
        {
            difficulty++;
        }

        OnGameStateChanged(sceneType);
    }

    private void OnEndHandler()
    {
        difficulty = 0;

        OnGameStateChanged(SceneType.MainMenu);
    }

    public void RegisterDamageTaken(float value) => damageTaken += value;
    private void RegisterDamageDealt(float value) => damageDealt += value;
    public void RegisterKill() => kills++;


    private void Evaluate()
    {
        float dps = damageDealt * 0.5f;
        float danger = damageTaken;

        // 👉 основной перформанс
        performanceScore =
              (dps * 0.5f) +
            (kills * 1.5f) -
           (danger * 1.2f);

        // 💥 СТРЕСС (главное нововведение)
        stress = Mathf.Clamp01(danger / (dps + 5f)) * 5f;

        AdjustDifficulty();
    }

    private void AdjustDifficulty()
    {
        // 💥 PANIC MODE (игрок страдает)
        if (stress > 3f && panicCooldown <= 0f)
        {
            if (stress > 4f)        // паника
            {
                targetDifficulty -= 0.4f;
            }
            else if (stress > 2f)   // давление
            {
                targetDifficulty -= 0.15f;
            }
            panicCooldown = 5f; // 5 секунд защита

            Debug.Log("PANIC MODE 🔻");
        }

        // 😎 DOMINATION MODE
        else if (performanceScore > 10f)
        {
            targetDifficulty += 0.1f;
        }
        else if (performanceScore < 2f)
        {
            targetDifficulty -= 0.1f;
        }

        targetDifficulty = Mathf.Clamp(targetDifficulty, 0.5f, 3f);
    }

    private void ResetStats()
    {
        damageTaken = 0;
        damageDealt = 0;
        kills = 0;
    }

    private void ResetDifficulty()
    {
        difficulty = 0;
        timeDifficulty = 0f;
        targetDifficulty = 1f;
        DifficultyMultiplier = 1f;
        OnTimerUpdate?.Invoke(timeDifficulty);
    }

    private void OnCommandToggleDifficultyScaler(bool isEnabled)
    {
        if (isEnabled)
        {
            isTimerRunning = true;
        }
        else
        {
            isTimerRunning = false;
            ResetDifficulty();
        }
    }

    private void OnGameStateChanged(SceneType state)
    {
        switch (state)
        {
            case SceneType.MainMenu:
                isTimerRunning = false;
                HideHUD();
                break;

            case SceneType.Arena:
                isTimerRunning = true;
                ShowHUD();
                break;

            case SceneType.Shop:
                isTimerRunning = false;
                ShowHUD();
                break;

            case SceneType.Secret:
                isTimerRunning = false;
                ShowHUD();
                break;

            case SceneType.Final:
                isTimerRunning = false;
                ShowHUD();
                break;
        }
    }


    private void OnDestroy()
    {
        PlayerService.OnDamageTaken -= RegisterDamageTaken;

        G.GameFlow.OnResetProgress -= OnStartHandler;
        G.GameFlow.OnNextScene -= OnNextHandler;
        G.GameFlow.OnEndGame -= OnEndHandler;

        ConsoleEvents.OnCommandToggleDifficultyScaler -= OnCommandToggleDifficultyScaler;

        ResetDifficulty();
    }
}
