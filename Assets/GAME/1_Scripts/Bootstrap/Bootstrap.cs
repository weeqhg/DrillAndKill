using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public static class G
{
    public static GameFlow GameFlow;
    public static AudioManager AudioManager;
    public static PoolManager PoolManager;
    public static InputManager InputManager;
    public static UIManager UIManager;
    public static WorldManager WorldManager;
    public static LootSystem LootSystem;
}

public class Bootstrap : MonoBehaviour
{
    [Header("Обязательные системы")]
    [SerializeField] private GameFlow gameFlowPrefab;
    [SerializeField] private AudioManager audioManagerPrefab;
    [SerializeField] private PoolManager poolManagerPrefab;
    [SerializeField] private InputManager inputManagerPrefab;
    [SerializeField] private UIManager uiManagerPrefab;
    [SerializeField] private LootSystem lootSystemPrefab;

    [Header("В зависимости от сцены")]
    [SerializeField] private MainMenuManager mainMenuManagerPrefab;
    [SerializeField] private GameMenu gameMenuPrefab;
    [SerializeField] private WorldManager worldManager;

    [Header("Доп. системы для тестов")]
    [SerializeField] private Console consolePrefab;

    // --- UI ---
    private CanvasGroup canvasGroup;
    private Image progressImage;
    private float fadeDuration = 0.5f;
    private int totalSteps = 7;
    private int currentStep = 0;



    private void Awake()
    {
        canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        progressImage = GetComponentInChildren<Image>(true);

        StartCoroutine(BootstrapRoutine());
    }

    #region Initialize System
    private IEnumerator BootstrapRoutine()
    {
        G.GameFlow = InitSystem(G.GameFlow, gameFlowPrefab);
        G.GameFlow.OnEndScene += Show;

        Show();
        yield return null;

        // --- Initialize Service ---
        G.InputManager = InitSystem(G.InputManager, inputManagerPrefab);

        StepDone();
        yield return null;

        G.AudioManager = InitSystem(G.AudioManager, audioManagerPrefab);

        StepDone();
        yield return null;

        G.PoolManager = InitSystem(G.PoolManager, poolManagerPrefab);

        StepDone();
        yield return null;

        G.UIManager = InitSystem(G.UIManager, uiManagerPrefab);

        StepDone();
        yield return null;

        G.LootSystem = InitSystem(G.LootSystem, lootSystemPrefab);

        StepDone();
        yield return null;

        // --- Create other systme ---
        if (worldManager != null) G.WorldManager = worldManager;
        MainMenuManager mainMenuManager = mainMenuManagerPrefab != null ? Instantiate(mainMenuManagerPrefab) : null;
        Console console = consolePrefab != null ? Instantiate(consolePrefab) : null;
        GameMenu gameMenu = gameMenuPrefab != null ? Instantiate(gameMenuPrefab) : null;

        StepDone();
        yield return null;

        worldManager?.Initialize();
        mainMenuManager?.Initialize();
        console?.Initialize();
        gameMenu?.Initialize();

        StepDone();
        Hide();
        yield return new WaitForSeconds(0.1f);
    }

    private T InitSystem<T>(T current, T prefab) where T : MonoBehaviour
    {
        if (current != null || prefab == null) return current;

        T instance = Instantiate(prefab);

        if (instance is IInitializable init)
            init.Initialize();

        return instance;
    }
    #endregion

    #region UI
    private void Show()
    {
        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
    }

    private void StepDone()
    {
        currentStep++;
        float step = currentStep / totalSteps;
        progressImage.fillAmount = step;
    }

    private void Hide()
    {
        canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InOutQuad).OnComplete(() => progressImage.fillAmount = 0f);
    }
    #endregion

    // =========================
    // Reset
    // =========================
    private void OnDestroy()
    {
        G.GameFlow.OnEndScene -= Show;
    }
}
