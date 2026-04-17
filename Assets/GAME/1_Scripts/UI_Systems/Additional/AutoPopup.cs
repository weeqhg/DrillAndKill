using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class AutoPopup : MonoBehaviour
{
    public enum PanelDirection
    {
        FromBottom,
        FromTop,
        FromLeft,
        FromRight,
        Scale
    }

    [Header("Panel Settings")]
    [SerializeField] private PanelDirection openDirection = PanelDirection.FromBottom;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    private RectTransform panel;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private Tween currentTween;
    public event Action OnClosePanel;

    public void Initialize()
    {
        panel = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Запоминаем конечную позицию
        endPosition = panel.anchoredPosition;

        // Вычисляем начальную позицию в зависимости от направления
        startPosition = GetHiddenPosition();

        // Устанавливаем начальное состояние
        panel.anchoredPosition = startPosition;
    }

    private Vector2 GetHiddenPosition()
    {
        switch (openDirection)
        {
            case PanelDirection.FromBottom:
                return new Vector2(endPosition.x, -panel.rect.height);
            case PanelDirection.FromTop:
                return new Vector2(endPosition.x, Screen.height + panel.rect.height);
            case PanelDirection.FromLeft:
                return new Vector2(-panel.rect.width, endPosition.y);
            case PanelDirection.FromRight:
                return new Vector2(Screen.width + panel.rect.width, endPosition.y);
            case PanelDirection.Scale:
                return endPosition; // Для Scale используем текущую позицию
            default:
                return new Vector2(endPosition.x, -panel.rect.height);
        }
    }

    public void OpenPanel()
    {
        currentTween?.Kill();

        // Анимация панели
        if (openDirection == PanelDirection.Scale)
        {
            panel.localScale = Vector3.zero;
            currentTween = panel.DOScale(1f, animationDuration).SetEase(openEase);
        }
        else
        {
            currentTween = panel.DOAnchorPos(endPosition, animationDuration).SetEase(openEase);
        }

        // Анимация прозрачности
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(1f, animationDuration);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void ClosePanel()
    {
        OnClosePanel?.Invoke();
        currentTween?.Kill();

        // Анимация закрытия
        if (openDirection == PanelDirection.Scale)
        {
            currentTween = panel.DOScale(0f, animationDuration).SetEase(closeEase);
        }
        else
        {
            currentTween = panel.DOAnchorPos(startPosition, animationDuration).SetEase(closeEase);
        }

        // Анимация прозрачности
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, animationDuration);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }
}