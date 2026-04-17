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
    public static DifficultyManager DifficultyManager;
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
    [SerializeField] private DifficultyManager difficultyManagerPrefab;
    [SerializeField] private LootSystem lootSystemPrefab;

    [Header("В зависимости от сцены")]
    [SerializeField] private MainMenuManager mainMenuManagerPrefab;
    [SerializeField] private GameMenu gameMenuPrefab;
    [SerializeField] private WorldManager worldManager;

    [Header("Доп. ситсемы для тестов")]
    [SerializeField] private Console consolePrefab;

    private MainMenuManager mainMenuManager;
    private Console console;
    private GameMenu gameMenu;

    // --- UI ---
    private CanvasGroup canvasGroup;
    private Image progressImage;
    private float fadeDuration = 0.5f;

    private void Awake()
    {
        canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        progressImage = GetComponentInChildren<Image>(true);

        StartCoroutine(BootstrapRoutine());
    }

    private IEnumerator BootstrapRoutine()
    {
        if (G.GameFlow == null && gameFlowPrefab != null)
        {
            G.GameFlow = Instantiate(gameFlowPrefab);
            G.GameFlow?.Initialize();
        }

        G.GameFlow.OnEndScene += Show;

        Show();
        yield return null;

        float progress = 0f;

        // --- Singleton'ы ---
        if (G.InputManager == null && inputManagerPrefab != null)
        {
            G.InputManager = Instantiate(inputManagerPrefab);
            G.InputManager?.Initialize();
        }

        progress += 0.1f;
        SetProgress(progress);
        yield return null;

        if (G.AudioManager == null && audioManagerPrefab != null)
        {
            G.AudioManager = Instantiate(audioManagerPrefab);
            G.AudioManager?.Initialize();
        }

        progress += 0.1f;
        SetProgress(progress);
        yield return null;

        if (G.PoolManager == null && poolManagerPrefab != null)
        {
            G.PoolManager = Instantiate(poolManagerPrefab);
            G.PoolManager?.Initialize();
        }

        progress += 0.1f;
        SetProgress(progress);
        yield return null;

        if (G.UIManager == null && uiManagerPrefab != null)
        {
            G.UIManager = Instantiate(uiManagerPrefab);
            G.UIManager?.Initialize();
        }

        progress += 0.1f;
        SetProgress(progress);
        yield return null;

        if (G.DifficultyManager == null && difficultyManagerPrefab != null)
        {
            G.DifficultyManager = Instantiate(difficultyManagerPrefab);
            G.DifficultyManager?.Initialize();
        }

        progress += 0.1f;
        SetProgress(progress);
        yield return null;

        if (G.LootSystem== null && lootSystemPrefab != null)
        {
            G.LootSystem = Instantiate(lootSystemPrefab);
            G.LootSystem?.Initialize();
        }

        progress += 0.1f;
        SetProgress(progress);
        yield return null;

        // --- Остальные системы ---
        if (worldManager != null) G.WorldManager = worldManager;

        if (mainMenuManagerPrefab != null) mainMenuManager = Instantiate(mainMenuManagerPrefab);
        if (consolePrefab != null) console = Instantiate(consolePrefab);
        if (gameMenuPrefab != null) gameMenu = Instantiate(gameMenuPrefab);

        progress += 0.3f;
        SetProgress(progress);
        yield return null;

        Initialize();

        progress = 1f;
        SetProgress(progress);
        yield return new WaitForSeconds(0.3f);

        Hide();
    }

    private void Initialize()
    {
        worldManager?.Initialize();
        mainMenuManager?.Initialize();
        console?.Initialize();
        gameMenu?.Initialize();
    }

    private void Show()
    {
        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
    }

    private void SetProgress(float value)
    {
        progressImage.fillAmount = value;
    }

    private void Hide()
    {
        canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InOutQuad).OnComplete(() => progressImage.fillAmount = 0f);
    }

    private void OnDestroy()
    {
        G.GameFlow.OnEndScene -= Show;
    }

}
