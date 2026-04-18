using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private SoundData sliderSound;
    private Slider slider;
    private bool isDragging = false;

    private void Start()
    {
        sliderSound = Resources.Load<SoundData>("Audio/UI/SliderSound");

        slider = GetComponent<Slider>();
        if (slider != null) slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        if (isDragging)
        {
            G.AudioManager?.Play(sliderSound);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        G.AudioManager?.Play(sliderSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
}
