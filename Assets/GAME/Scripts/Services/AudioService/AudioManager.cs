using UnityEngine;
using UnityEngine.Audio;
using WekenDev.AudioManagerGame;



public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Parameters")]
    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";
    private AudioMusicController _musicController;
    private AudioUIController _UIController;
    private AudioSFXController _sxfController;

    public void Initialize()
    {
        if (G.AudioManager != null && G.AudioManager != this)
        {
            Destroy(gameObject);
            return;
        }

        _musicController = GetComponentInChildren<AudioMusicController>();
        _musicController.Init();
        ChangeMusic(AudioDesign.Calm);

        _UIController = GetComponentInChildren<AudioUIController>();
        _UIController.Init();

        _sxfController = GetComponentInChildren<AudioSFXController>();
        _sxfController.Init();

        LoadVolumes();
        
        G.AudioManager = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeMusic(AudioDesign audioDesign)
    {
        _musicController.ChangeAudioDesign(audioDesign);
    }
    public void PlayAudioUI(TypeUiAudio type)
    {
        if (_UIController == null) return;

        _UIController.PlayAudioUI(type);
    }

    public void PlayAudioSFX(TypeSFX type)
    {
        _sxfController.PlayAudioSFX(type);
    }

    public void PlayAudiDurationSFX(TypeSFX type, float duration, float startVolume, float targetVolume, bool stopAfterFade)
    {
        _sxfController.PlayAudiDurationSFX(type, duration, startVolume, targetVolume, stopAfterFade);
    }

    public void PlayAudio3DSFX(AudioClip clip, Vector3 pos)
    {
        _sxfController.PlaySFX3D(clip, pos);
    }


    #region Volume Control

    /// <summary>
    /// Установить громкость мастера (0-100)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        float dB = ConvertToDecibels(volume);
        audioMixer.SetFloat(MASTER_VOLUME, dB);
        PlayerPrefs.SetFloat(PlayerPrefsKeys.MasterVolume, volume);
    }

    /// <summary>
    /// Установить громкость музыки (0-100)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        float dB = ConvertToDecibels(volume);
        audioMixer.SetFloat(MUSIC_VOLUME, dB);
        PlayerPrefs.SetFloat(PlayerPrefsKeys.MusicVolume, volume);
    }

    /// <summary>
    /// Установить громкость звуковых эффектов (0-100)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        float dB = ConvertToDecibels(volume);
        audioMixer.SetFloat(SFX_VOLUME, dB);
        PlayerPrefs.SetFloat(PlayerPrefsKeys.SFXVolume, volume);
    }


    /// <summary>
    /// Получить громкость мастера
    /// </summary>
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(PlayerPrefsKeys.MasterVolume, 80f);
    }

    /// <summary>
    /// Получить громкость музыки
    /// </summary>
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(PlayerPrefsKeys.MusicVolume, 80f);
    }

    /// <summary>
    /// Получить громкость SFX
    /// </summary>
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(PlayerPrefsKeys.SFXVolume, 80f);
    }

    /// <summary>
    /// Загрузить сохранённые настройки
    /// </summary>
    public void LoadVolumes()
    {
        SetMasterVolume(GetMasterVolume());
        SetMusicVolume(GetMusicVolume());
        SetSFXVolume(GetSFXVolume());
    }

    /// <summary>
    /// Конвертировать линейное значение (0-100) в децибелы (-80 - 0)
    /// </summary>
    private float ConvertToDecibels(float volume)
    {
        if (volume <= 0) return -80f;
        return Mathf.Log10(volume / 100f) * 20f;
    }

    #endregion

}

