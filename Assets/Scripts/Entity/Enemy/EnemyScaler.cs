using UnityEngine;

public class EnemyScaler : MonoBehaviour
{
    private enum GamePhase
    {
        Early,
        Mid,
        Late,
        Endgame
    }

    private enum DirectorState
    {
        BuildUp,   // наращивание
        Peak,      // пик (жёстко)
        Relax,      // отдых
        BossFight
    }


    private EnemySpawner enemySpawner;
    private float spawnTimer;
    private float nextSpawnTime;

    private DirectorState currentState;
    private float stateTimer;

    private float currentDifficulty;

    private DifficultyManager difficulty;

    private bool isBossActive;

    public void Initialize()
    {
        GameEvents.OnBossStartFight += OnBossStart;
        GameEvents.OnBossDefeated += OnBossEnd;

        difficulty = GameManager.Instance.difficultyManager;
        difficulty.OnTimerUpdate += OnTimerUpdateHandler;

        enemySpawner = GetComponent<EnemySpawner>();

        ScheduleNextSpawn(0f);
    }

    private (int, int) CalculateReward(TypeEnemy type)
    {
        int baseExp = type switch
        {
            TypeEnemy.Default => 10,
            TypeEnemy.Elite => 25,
            TypeEnemy.Boss => 200,
            _ => 10
        };

        int baseCoins = type switch
        {
            TypeEnemy.Default => 5,
            TypeEnemy.Elite => 15,
            TypeEnemy.Boss => 100,
            _ => 5
        };

        // 🔥 множители
        float difficultyBonus = difficulty.DifficultyMultiplier;
        float phaseBonus = GetPhaseBonus();
        float stateBonus = GetDirectorBonus();

        int exp = Mathf.RoundToInt(baseExp * difficultyBonus * phaseBonus);
        int coins = Mathf.RoundToInt(baseCoins * difficultyBonus * stateBonus);

        return (exp, coins);
    }

    private float GetPhaseBonus()
    {
        return GetPhase(difficulty.timeDifficulty) switch
        {
            GamePhase.Early => 1f,
            GamePhase.Mid => 1.3f,
            GamePhase.Late => 1.6f,
            GamePhase.Endgame => 2f,
            _ => 1f
        };
    }

    private float GetDirectorBonus()
    {
        return currentState switch
        {
            DirectorState.Peak => 1.5f,
            DirectorState.Relax => 0.7f,
            DirectorState.BossFight => 2f,
            _ => 1f
        };
    }

    private void OnTimerUpdateHandler(float time)
    {
        currentDifficulty = CalculateDifficulty(time) * difficulty.DifficultyMultiplier;
        UpdateDirectorState(time);

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= nextSpawnTime)
        {
            SpawnWave(difficulty.timeDifficulty);
            ScheduleNextSpawn(difficulty.timeDifficulty);
            spawnTimer = 0f;
        }
    }

    private void UpdateDirectorState(float time)
    {
        if (isBossActive) return;

        stateTimer -= Time.deltaTime;

        if (stateTimer > 0f) return;

        float stress = difficulty.DifficultyMultiplier;

        switch (currentState)
        {
            case DirectorState.BuildUp:
                if (stress > 2.2f)
                {
                    currentState = DirectorState.Peak;
                    stateTimer = Random.Range(5f, 10f);
                }
                break;

            case DirectorState.Peak:
                currentState = DirectorState.Relax;
                stateTimer = Random.Range(5f, 8f);
                break;

            case DirectorState.Relax:
                currentState = DirectorState.BuildUp;
                stateTimer = Random.Range(10f, 20f);
                break;
        }
    }

    private void OnBossStart()
    {
        isBossActive = true;
        currentState = DirectorState.BossFight;

        // ❗ сразу уменьшаем давление
        nextSpawnTime *= 2f;
    }

    private void OnBossEnd()
    {
        isBossActive = false;

        // возвращаемся в нормальную фазу
        currentState = DirectorState.Relax;
        stateTimer = 5f;
    }

    private void SpawnWave(float time)
    {
        if (isBossActive)
        {
            // 👉 80% вообще не спавним
            if (Random.value < 0.8f) return;

            // 👉 если спавним — очень слабых
            int count = Mathf.Max(1, CalculateEnemyCount() / 4);
            int level = Mathf.Max(1, CalculateEnemyLevel() - 5);

            enemySpawner.SpawnEnemies(count, TypeEnemy.Default, level);
            return;
        }

        // обычная логика
        if (currentState == DirectorState.Relax)
        {
            if (Random.value < 0.7f) return;
        }

        int normalCount = CalculateEnemyCount();
        int normalLevel = CalculateEnemyLevel();
        TypeEnemy type = GetEnemyType(time);

        if (currentState == DirectorState.Peak)
        {
            normalCount *= 2;
            normalLevel += 3;
        }

        (int exp, int coins) = CalculateReward(type);
        enemySpawner.SpawnEnemies(normalCount, type, normalLevel, exp, coins);
    }

    private void ScheduleNextSpawn(float time)
    {
        float interval = Mathf.Lerp(5f, 0.7f, currentDifficulty / 20f);

        nextSpawnTime = Mathf.Clamp(interval, 0.5f, 5f);
    }

    private int CalculateEnemyCount()
    {
        return Mathf.Clamp(
            Mathf.RoundToInt(2f + currentDifficulty * 1.5f),
            1,
            100
        );
    }

    private int CalculateEnemyLevel()
    {
        return Mathf.Clamp(
            Mathf.RoundToInt(1f + currentDifficulty * 0.8f),
            1,
            50
        );
    }

    private float CalculateDifficulty(float time)
    {
        float minutes = time / 60f;

        // 1. Базовый рост (замедляющийся)
        float baseDifficulty = Mathf.Pow(minutes, 1.2f);

        // 2. Волны сложности (пульсация)
        float wave = Mathf.Sin(time * 0.2f) * 2f;

        // 3. Плавное ограничение (soft cap)
        float capped = baseDifficulty / (1f + baseDifficulty * 0.05f);

        return 1f + capped + wave;
    }

    private GamePhase GetPhase(float time)
    {
        if (time < 300f) return GamePhase.Early;     // 0–5 мин
        if (time < 1200f) return GamePhase.Mid;     // 5–20 мин
        if (time < 2400f) return GamePhase.Late;    // 20–40 мин
        return GamePhase.Endgame;                   // 40+ мин
    }

    private TypeEnemy GetEnemyType(float time)
    {
        switch (GetPhase(time))
        {
            case GamePhase.Early:
                return TypeEnemy.Default;

            case GamePhase.Mid:
                return Random.value > 0.7f
                    ? TypeEnemy.Elite
                    : TypeEnemy.Default;

            case GamePhase.Late:
                return Random.value > 0.5f
                    ? TypeEnemy.Elite
                    : TypeEnemy.Default;

            case GamePhase.Endgame:
                return TypeEnemy.Elite;
        }

        return TypeEnemy.Default;
    }

    private void OnDestroy()
    {
        GameEvents.OnBossStartFight -= OnBossStart;
        GameEvents.OnBossDefeated -= OnBossEnd;

        difficulty.OnTimerUpdate -= OnTimerUpdateHandler;
    }
}
