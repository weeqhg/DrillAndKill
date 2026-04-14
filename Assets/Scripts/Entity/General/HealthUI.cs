using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    private float _currentHealth;
    private float _maxHealth;
    private Tween _currentValueTween;

    public void Initialize(float maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        UpdateText();
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        _currentHealth = currentHealth;
        _maxHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;

            // Сохраняем ссылку на анимацию значения
            _currentValueTween?.Kill();
            _currentValueTween = DOTween.To(
                () => healthSlider.value,
                x => healthSlider.value = x,
                currentHealth,
                0.3f
            ).SetEase(Ease.OutCubic);
        }

        UpdateText();

        if (healthSlider != null)
        {
            // Отменяем предыдущую анимацию пульсации
            DOTween.Kill(healthSlider.transform);
            // Сбрасываем масштаб
            healthSlider.transform.localScale = Vector3.one;
            // Запускаем новую
            healthSlider.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 1, 0);
        }
    }

    private void UpdateText()
    {
        if (healthText != null)
            healthText.text = $"{_currentHealth:F0}/{_maxHealth:F0}";
    }

    private void OnDestroy()
    {
        // Отменяем анимацию значения
        _currentValueTween?.Kill();

        // Отменяем все анимации на слайдере
        if (healthSlider != null)
            DOTween.Kill(healthSlider.transform);
    }
}