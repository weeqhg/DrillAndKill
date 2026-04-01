using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using DG.Tweening;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private LocalizedString levelText;
    private Tween _currentValueTween;
    public void LevelChanged(int level)
    {
        levelText.Arguments = new object[] { level };

        levelLabel.text = levelText.GetLocalizedString();
    }

    public void ExpChanged(int current, int maxExp)
    {
        if (expSlider != null)
        {
            expSlider.maxValue = maxExp;

            // Сохраняем ссылку на анимацию значения
            _currentValueTween?.Kill();
            _currentValueTween = DOTween.To(
                () => expSlider.value,
                x => expSlider.value = x,
                current,
                0.1f
            ).SetEase(Ease.OutCubic);
        }

        if (expSlider != null)
        {
            // Отменяем предыдущую анимацию пульсации
            DOTween.Kill(expSlider.transform);
            // Сбрасываем масштаб
            expSlider.transform.localScale = Vector3.one;
            // Запускаем новую
            expSlider.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0);
        }
    }

    private void OnDestroy()
    {
        // Отменяем анимацию значения
        _currentValueTween?.Kill();

        // Отменяем все анимации на слайдере
        if (expSlider != null)
            DOTween.Kill(expSlider.transform);
    }
}
