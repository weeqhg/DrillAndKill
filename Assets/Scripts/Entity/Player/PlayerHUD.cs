using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    public void Initialize()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        GamePause.OnPauseGame += ToggleHUD;

        ToggleHUD(GamePause.IsGamePaused);
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
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
    private void HideHUD()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnDestroy()
    {
        GamePause.OnPauseGame -= ToggleHUD;
    }
}
