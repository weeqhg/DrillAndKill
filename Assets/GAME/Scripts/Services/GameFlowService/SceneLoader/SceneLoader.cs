using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [SerializeField] private string[] arenaScenes;
    [SerializeField] private string[] secretScenes;
    [SerializeField] private string shopScene;
    [SerializeField] private string mainMenu;
    [SerializeField] private string finalScene;
    public int arenaIndex = 0;

    public void SceneHandler(SceneType sceneType)
    {
        string sceneName = "";

        switch (sceneType)
        {
            case SceneType.MainMenu:
                arenaIndex = 0;
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

        StartCoroutine(LoadSceneWithFade(sceneName));
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        yield return new WaitForSeconds(1f); // небольшой буфер перед началом загрузки

        SceneManager.LoadScene(sceneName);
    }

}
