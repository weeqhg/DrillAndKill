using UnityEngine;

public abstract class UIWindow : MonoBehaviour, IUIWindow
{
    [SerializeField] protected CanvasGroup canvasGroup;
    public virtual bool CanBeClosed => true;
    public virtual void Show()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public virtual void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }
}