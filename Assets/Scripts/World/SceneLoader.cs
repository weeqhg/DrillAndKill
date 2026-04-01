using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;
    private string sceneName = "Game";

    public void Initialize()
    {
        fadeImage.DOFade(0f, 0f);
        GameEvents.OnGameStart += OnStartButtonClick;
    }
    private void OnStartButtonClick(string sceneName)
    {
        this.sceneName = sceneName;
        StartCoroutine(LoadSceneWithFade());
    }

    private System.Collections.IEnumerator LoadSceneWithFade()
    {
        fadeImage.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        GameEvents.OnGameStart -= OnStartButtonClick;
    }
}