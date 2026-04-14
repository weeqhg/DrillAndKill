using System.Collections;
using UnityEngine;

public enum TypeSFX
{
    Explose,
    Die,
    BreakObject,
    LandObject,
    UpObject,
    Earthquake
}
public class AudioSFXController : MonoBehaviour
{
    [Header("SXF Sound")]
    [SerializeField] private AudioSource defaultSFX;
    [SerializeField] private AudioSource randomSFX;
    [SerializeField] private AudioSource changeVolumeSFX;
    [SerializeField] private AudioSource spatialSFX;
    public AudioClip explose;
    public AudioClip die;
    public AudioClip breakObj;
    public AudioClip landOnject;
    public AudioClip upObject;
    public AudioClip earthquake;

    private float lastExplose;
    private float minIntervalExplose = 0.1f;
    private float lastDie;
    private float minIntervalDie = 0.1f;
    private bool isPaused = false;


    public void Init()
    {
        GamePause.OnPauseGame += OnPauseSound;
    }

    private void OnPauseSound(bool enable)
    {
        isPaused = enable;

        if (enable)
        {
            if (changeVolumeSFX.isPlaying)
                changeVolumeSFX.Pause();
        }
        else
        {
            changeVolumeSFX.UnPause();
        }
    }

    public void PlayAudioSFX(TypeSFX type)
    {
        switch (type)
        {
            case TypeSFX.Explose:
                PlayExplose();
                break;
            case TypeSFX.Die:
                PlayDie();
                break;
            case TypeSFX.BreakObject:
                PlayRandomPitch(breakObj, 0.3f);
                break;
            case TypeSFX.LandObject:
                defaultSFX.PlayOneShot(landOnject);
                break;
            case TypeSFX.UpObject:
                defaultSFX.PlayOneShot(upObject);
                break;
        }
    }

    public void PlaySFX3D(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        float pitch = 1f + Random.Range(-0.3f, 0.3f);
        spatialSFX.pitch = pitch;
        spatialSFX.transform.position = position;

        spatialSFX.PlayOneShot(clip);
    }

    public void PlayAudiDurationSFX(TypeSFX type, float duration, float startVolume, float targetVolume, bool stopAfterFade)
    {
        switch (type)
        {
            case TypeSFX.Earthquake:
                changeVolumeSFX.volume = startVolume;
                changeVolumeSFX.clip = earthquake;
                changeVolumeSFX.Play();
                StartCoroutine(FadeAudio(duration, targetVolume, stopAfterFade));
                break;
        }
    }

    private void PlayExplose()
    {
        if (Time.time - lastExplose < minIntervalExplose)
            return;

        lastExplose = Time.time;

        PlayRandomPitch(explose, 0.3f);
    }

    private void PlayDie()
    {
        if (Time.time - lastDie < minIntervalDie)
            return;

        lastDie = Time.time;

        PlayRandomPitch(die, 0.3f);
    }

    private void PlayRandomPitch(AudioClip sounds, float variation)
    {
        if (sounds == null) return;

        float pitch = 1f + Random.Range(-variation, variation);
        randomSFX.pitch = pitch;

        randomSFX.PlayOneShot(sounds);
    }

    private IEnumerator FadeAudio(float duration, float targetVolume, bool stopAfterFade = false)
    {
        float startVolume = changeVolumeSFX.volume;
        float time = 0f;

        while (time < duration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            time += Time.deltaTime;
            changeVolumeSFX.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        changeVolumeSFX.volume = targetVolume;

        if (stopAfterFade)
        {
            float secondDuration = 1f;
            float secondTime = 0f;
            float secondStartVolume = targetVolume;

            while (secondTime < secondDuration)
            {
                if (isPaused)
                {
                    yield return null;
                    continue;
                }

                secondTime += Time.deltaTime;
                changeVolumeSFX.volume = Mathf.Lerp(secondStartVolume, 0f, secondTime / secondDuration);
                yield return null;
            }

            changeVolumeSFX.volume = 0f;
            changeVolumeSFX.Stop();
        }
    }
}

