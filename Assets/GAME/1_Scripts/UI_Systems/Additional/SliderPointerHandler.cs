using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Slider _slider;
    private bool _isDragging = false;
    private void Awake()
    {
        _slider = GetComponent<Slider>();
        if (_slider != null)
            _slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        // Воспроизводим звук только если пользователь активно взаимодействует со слайдером
        if (_isDragging)
        {
            G.AudioManager?.PlayAudioUI(TypeUiAudio.Slider);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
        G.AudioManager?.PlayAudioUI(TypeUiAudio.Slider);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
    }
}
