using UnityEngine;

public enum TypeUiAudio
{
    Button,
    ButtonSelect,
    Slider
}

namespace WekenDev.AudioManagerGame
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioUIController : MonoBehaviour
    {
        [Header("UI Sound")]
        [SerializeField] private AudioClip[] _buttons;
        [SerializeField] private AudioClip[] _buttonsSelect;
        [SerializeField] private AudioClip[] _sliders;
        private AudioSource _audio;
        public void Init()
        {
            _audio = GetComponent<AudioSource>();
        }

        public void PlayAudioUI(TypeUiAudio type)
        {
            switch (type)
            {
                case TypeUiAudio.Button:
                    PlayButton();
                    break;
                case TypeUiAudio.ButtonSelect:
                    PlaySelect();
                    break;
                case TypeUiAudio.Slider:
                    PlaySlide();
                    break;
            }
        }

        private void PlayButton()
        {
            if (_buttons.Length > 0)
            {
                AudioClip clip = _buttons[Random.Range(0, _buttons.Length)];
                _audio.PlayOneShot(clip);
            }
        }

        private void PlaySelect()
        {
            if (_buttons.Length > 0)
            {
                AudioClip clip = _buttonsSelect[Random.Range(0, _buttonsSelect.Length)];
                _audio.PlayOneShot(clip);
            }
        }

        private float _lastSliderTime;
        private float _sliderCooldown = 0.1f;

        private void PlaySlide()
        {
            if (_sliders == null || _sliders.Length == 0) return;

            if (Time.time - _lastSliderTime < _sliderCooldown) return;

            _lastSliderTime = Time.time;

            AudioClip clip = _sliders[Random.Range(0, _sliders.Length)];
            _audio.PlayOneShot(clip);
        }
    }
}
