using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

public class SoundHandle
{
    public AudioSource Source { get; private set; }

    public SoundHandle(AudioSource source)
    {
        Source = source;
    }
}

public class AudioManager : MonoBehaviour, IInitializable
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer MAIN;

    private AudioSource sourcePrefab;
    private SoundData ambientSound;

    private bool isPaused = false;
    private int poolSize = 10;

    private Queue<AudioSource> pool = new();
    private List<AudioSource> activeSources = new();
    private Dictionary<SoundData, float> lastPlayTime = new();

    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";



    public void Initialize()
    {
        if (G.AudioManager != null && G.AudioManager != this)
        {
            Destroy(gameObject);
            return;
        }
        sourcePrefab = GetComponentInChildren<AudioSource>();
        sourcePrefab.gameObject.SetActive(true);

        pool.Enqueue(sourcePrefab);

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = Instantiate(sourcePrefab, transform);
            src.gameObject.SetActive(false);
            pool.Enqueue(src);
        }

        ambientSound = Resources.Load<SoundData>("Audio/Ambient/RandomAmbient");
        if (ambientSound != null)
            Play(ambientSound);

        GamePause.OnPauseGame += SetPause;
        LoadVolumes();

        G.AudioManager = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // Методы для работы с внешними вызовами
    // =========================
    public SoundHandle Play(SoundData sound, Vector3 position = default(Vector3))
    {
        if (sound == null) return null;
        if (!CanPlay(sound)) return null;

        AudioClip clip = sound.GetClip();
        if (clip == null) return null;

        AudioSource source = CreateSource(sound);

        source.clip = clip;
        source.transform.position = position;
        source.Play();

        if (sound.fadeIn > 0f)
            StartCoroutine(Fade(source, 0f, sound.volume, sound.fadeIn));

        if (!sound.loop)
            StartCoroutine(AutoDestroy(source));

        return new SoundHandle(source);
    }

    public void Stop(SoundHandle handle, float fadeOut = 0f)
    {
        if (handle == null || handle.Source == null) return;

        if (fadeOut > 0f)
            StartCoroutine(FadeAndStop(handle.Source, fadeOut));
        else
            DestroySource(handle.Source);
    }

    #region CORE
    private AudioSource GetSource()
    {
        if (pool.Count > 0)
        {
            var src = pool.Dequeue();
            src.gameObject.SetActive(true);
            return src;
        }

        return Instantiate(sourcePrefab, transform);
    }

    private void ReturnSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.loop = false;
        source.volume = 1f;
        source.pitch = 1f;

        source.gameObject.SetActive(false);

        pool.Enqueue(source);
    }

    private void SetPause(bool value)
    {
        isPaused = value;

        foreach (var src in activeSources)
        {
            if (src.ignoreListenerPause) continue;
            
            if (value) src.Pause();
            else src.UnPause();
        }
    }

    private AudioSource CreateSource(SoundData sound)
    {
        AudioSource source = GetSource();

        source.outputAudioMixerGroup = sound.mixerGroup;

        source.ignoreListenerPause = sound.ignorePause;

        if (sound.is3D)
        {
            source.spatialBlend = 1f;
            source.minDistance = sound.minDistance;
            source.maxDistance = sound.maxDistance;
        }
        else
        {
            source.spatialBlend = 0f;
        }

        if (sound.randomPitch)
        {
            source.pitch = 1f + Random.Range(-sound.pitchVariation, sound.pitchVariation);
        }

        source.loop = sound.loop;
        source.volume = (sound.fadeIn > 0f) ? 0f : sound.volume;

        activeSources.Add(source);
        return source;
    }

    private bool CanPlay(SoundData sound)
    {
        if (sound.minInterval <= 0f) return true;

        if (!lastPlayTime.TryGetValue(sound, out float last))
            last = 0f;

        if (Time.time - last < sound.minInterval)
            return false;

        lastPlayTime[sound] = Time.time;
        return true;
    }

    #endregion

    #region FADE

    private IEnumerator Fade(AudioSource source, float start, float target, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            time += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        source.volume = target;
    }

    private IEnumerator FadeAndStop(AudioSource source, float duration)
    {
        float start = source.volume;
        float time = 0f;

        while (time < duration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            time += Time.deltaTime;
            source.volume = Mathf.Lerp(start, 0f, time / duration);
            yield return null;
        }

        DestroySource(source);
    }

    #endregion

    #region UTILS

    private IEnumerator AutoDestroy(AudioSource source)
    {
        // ждём пока звук реально играет
        yield return new WaitUntil(() => source != null && source.isPlaying);

        // ждём пока закончится
        yield return new WaitWhile(() => source != null && source.isPlaying);

        DestroySource(source);
    }

    private void DestroySource(AudioSource source)
    {
        if (source == null) return;

        activeSources.Remove(source);
        ReturnSource(source);
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// Установить громкость мастера (0-100)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        float dB = ConvertToDecibels(volume);
        MAIN.SetFloat(MASTER_VOLUME, dB);
        PlayerPrefs.SetFloat(PlayerPrefsKeys.MasterVolume, volume);
    }

    /// <summary>
    /// Установить громкость музыки (0-100)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        float dB = ConvertToDecibels(volume);
        MAIN.SetFloat(MUSIC_VOLUME, dB);
        PlayerPrefs.SetFloat(PlayerPrefsKeys.MusicVolume, volume);
    }

    /// <summary>
    /// Установить громкость звуковых эффектов (0-100)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        float dB = ConvertToDecibels(volume);
        MAIN.SetFloat(SFX_VOLUME, dB);
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
        volume = Mathf.Clamp(volume, 0.0001f, 100f);
        return Mathf.Log10(volume / 100f) * 20f;
    }
    #endregion

    private void OnDestroy()
    {
        GamePause.OnPauseGame -= SetPause;
    }
}