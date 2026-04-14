using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    MainMenu,
    Arena,
    Shop,
    Secret,
    Final
}
public class SceneLoader : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressImage;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    [SerializeField] private string[] arenaScenes;
    [SerializeField] private string[] secretScenes;
    [SerializeField] private string shopScene;
    [SerializeField] private string mainMenu;
    [SerializeField] private string finalScene;

    public int arenaIndex = 0;
    private string sceneName = "Game";

    public void Initialize()
    {
        GameEvents.OnStartGame += OnStartHandler;
        GameEvents.OnNextLevel += OnNextHandler;
        GameEvents.OnEndGame += OnEndHandler;
    }

    private void OnStartHandler()
    {
        ResetProgress();
        SceneHandler(SceneType.Arena);
    }

    private void OnNextHandler(SceneType sceneType)
    {
        SceneHandler(sceneType);
    }

    private void OnEndHandler()
    {
        SceneHandler(SceneType.MainMenu);
    }


    public void Show()
    {
        progressImage.fillAmount = 0f;
        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
    }

    public void Hide()
    {
        progressImage.fillAmount = 0f;
        canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InOutQuad);
    }
    public void SetProgress(float value)
    {
        progressImage.fillAmount = value;
    }

    public void ResetProgress()
    {
        arenaIndex = 0;
    }

    public void SceneHandler(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.MainMenu:
                sceneName = mainMenu;
                break;

            case SceneType.Arena:
                sceneName = arenaScenes[arenaIndex];
                arenaIndex = (arenaIndex + 1) % arenaScenes.Length;
                break;

            case SceneType.Secret:
                sceneName = secretScenes[Random.Range(0, secretScenes.Length)];
                break;

            case SceneType.Shop:
                sceneName = shopScene;
                break;

            case SceneType.Final:
                sceneName = finalScene;
                break;
        }

        StartCoroutine(LoadSceneWithFade());
    }

    private IEnumerator LoadSceneWithFade()
    {
        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        GameEvents.OnStartGame -= OnStartHandler;
        GameEvents.OnNextLevel -= OnNextHandler;
        GameEvents.OnEndGame -= OnEndHandler;
    }

}