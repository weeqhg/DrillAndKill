using UnityEngine;

public class DifficultyManager : MonoBehaviour, IInitializable
{
    public float Multiplier { get; private set; } = 1f;
    public int Level { get; private set; } = 0;
    private float timer;

    private float targetDifficulty = 1f;
    private float performanceScore = 1f;
    private float stress;        // текущий стресс игрока
    private float panicCooldown; // защита от спама паники

    private float damageTaken;
    private float damageDealt;
    private float kills;

    public void Initialize()
    {
        PlayerService.OnDamageTaken += RegisterDamageTaken;
        PlayerService.OnDamageDelta += RegisterDamageDealt;
        PlayerService.OnKill += RegisterKill;
    }

    // =========================
    // Game Flow управляет сложностью
    // =========================
    public void StartRun()
    {
        ResetDifficulty();
    }

    public void NextLevel(SceneType sceneType)
    {
        if (sceneType == SceneType.Arena || sceneType == SceneType.Secret)
        {
            Level++;
        }
    }

    public void EndRun()
    {
        Level = 0;
    }

    // =========================
    // Таймер
    // =========================
    public void UpdateTime()
    {
        if (panicCooldown > 0f) panicCooldown -= Time.deltaTime;

        float speed = Mathf.Max(0.5f, Mathf.Abs(Multiplier - targetDifficulty) * 2f);

        Multiplier = Mathf.Lerp(Multiplier, targetDifficulty, Time.deltaTime * speed);

        timer += Time.deltaTime;
        if (timer >= 2f)
        {
            Evaluate();
            ResetStats();
            timer = 0f;
            //Debug.Log($"Stress: {stress:F2} | Difficulty x{DifficultyMultiplier:F2}");
        }
    }


    // =========================
    // Оброботчики событий
    // =========================
    private void RegisterDamageTaken(float value) => damageTaken += value;
    private void RegisterDamageDealt(float value) => damageDealt += value;
    private void RegisterKill() => kills++;


    // =========================
    // Расчеты
    // =========================
    private void Evaluate()
    {
        float dps = damageDealt * 0.5f;

        performanceScore = (dps * 0.5f) + (kills * 1.5f) - (damageTaken * 1.2f);

        stress = Mathf.Clamp01(damageTaken / (dps + 5f)) * 5f;

        AdjustDifficulty();
    }

    private void AdjustDifficulty()
    {
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

    // =========================
    // Resets
    // =========================
    private void ResetStats()
    {
        damageTaken = 0;
        damageDealt = 0;
        kills = 0;
    }

    private void ResetDifficulty()
    {
        targetDifficulty = 1f;
        Level = 0;
        Multiplier = 1f;
    }


    private void OnDestroy()
    {
        PlayerService.OnDamageTaken -= RegisterDamageTaken;
        PlayerService.OnDamageDelta -= RegisterDamageDealt;
        PlayerService.OnKill -= RegisterKill;

        ResetDifficulty();
    }
}
