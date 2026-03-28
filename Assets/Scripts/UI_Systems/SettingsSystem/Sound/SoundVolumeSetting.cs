using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


namespace WekenDev.Settings.Sound
{

    public class SoundVolumeSetting : MonoBehaviour
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Volume Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Volume Texts")]
        [SerializeField] private TMP_Text masterVolumeText;
        [SerializeField] private TMP_Text musicVolumeText;
        [SerializeField] private TMP_Text sfxVolumeText;

        private AudioManager _audioManager;
        public void Init()
        {
            _audioManager = AudioManager.Instance;

            InitializeSliders();
        }

        private void InitializeSliders()
        {
            // Мастер-громкость
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 100f;
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                masterVolumeText.text = $"{Mathf.RoundToInt(masterVolumeSlider.value)}%";
            }

            // Музыка
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 100f;
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
                musicVolumeText.text = $"{Mathf.RoundToInt(musicVolumeSlider.value)}%";
            }

            // Звуковые эффекты
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 100f;
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeText.text = $"{Mathf.RoundToInt(sfxVolumeSlider.value)}%";
            }
        }

        public void SetMasterVolume(float volume)
        {
            _audioManager.SetMasterVolume(volume);
            masterVolumeText.text = $"{Mathf.RoundToInt(volume)}%";
        }

        public void SetMusicVolume(float volume)
        {
            _audioManager.SetMusicVolume(volume);
            musicVolumeText.text = $"{Mathf.RoundToInt(volume)}%";
        }

        public void SetSFXVolume(float volume)
        {
            _audioManager.SetSFXVolume(volume);
            sfxVolumeText.text = $"{Mathf.RoundToInt(volume)}%";
        }
    }

}