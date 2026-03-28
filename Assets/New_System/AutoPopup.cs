using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
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
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button toggleButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private PanelDirection openDirection = PanelDirection.FromBottom;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;
    [SerializeField] private float backgroundFadeDuration = 0.2f;

    [Header("Background")]
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.5f);

    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool isOpen = false;
    private Tween currentTween;
    public event Action<bool> OnTogglePanel;

    public void Initialize()
    {
        // Запоминаем конечную позицию
        endPosition = panel.anchoredPosition;

        // Вычисляем начальную позицию в зависимости от направления
        startPosition = GetHiddenPosition();

        // Устанавливаем начальное состояние
        panel.anchoredPosition = startPosition;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        if (backgroundOverlay != null)
        {
            backgroundOverlay.color = backgroundColor;
            backgroundOverlay.gameObject.SetActive(false);
        }

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);
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

    private void Update()
    {
        if (isOpen && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!RectTransformUtility.RectangleContainsScreenPoint(panel, mousePos) &&
                !RectTransformUtility.RectangleContainsScreenPoint(toggleButton.GetComponent<RectTransform>(), mousePos))
            {
                ClosePanel();
            }
        }
    }

    public void TogglePanel()
    {
        if (isOpen) ClosePanel();
        else OpenPanel();
    }

    public void OpenPanel()
    {
        if (isOpen) return;
        OnTogglePanel?.Invoke(true);
        isOpen = true;
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

        // Затемняющий фон
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
            backgroundOverlay.DOFade(backgroundColor.a, backgroundFadeDuration).From(0f);
        }
    }

    public void ClosePanel()
    {
        if (!isOpen) return;
        OnTogglePanel?.Invoke(false);
        isOpen = false;
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

        // Скрываем фон
        if (backgroundOverlay != null)
        {
            backgroundOverlay.DOFade(0f, backgroundFadeDuration)
                .OnComplete(() => backgroundOverlay.gameObject.SetActive(false));
        }
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(TogglePanel);
    }
}