using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image highlightImage;
    private float duration = 0.25f;
    private Ease ease = Ease.OutCubic;
    private Tween _scaleTween;
    private Tween _fillTween;
    private void Start()
    {
        highlightImage = GetComponent<Image>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Button);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayAudioUI(TypeUiAudio.ButtonSelect);
        AnimateFill(1);
        AnimateScale(1.1f);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateFill(0);
        AnimateScale(1f);
    }

    private void OnDisable()
    {
        _fillTween?.Kill();
        _scaleTween?.Kill();

        if (highlightImage != null) highlightImage.fillAmount = 0;
        transform.localScale = Vector3.one;
    }

    private void AnimateFill(float target)
    {
        if (highlightImage == null) return;

        _fillTween?.Kill();

        _fillTween = highlightImage
            .DOFillAmount(target, duration)
            .SetEase(ease);
    }
    private void AnimateScale(float target)
    {
        _scaleTween?.Kill();

        _scaleTween = transform
            .DOScale(target, 0.2f)
            .SetEase(Ease.OutBack);
    }
}