using UnityEngine;
using UnityEngine.Video;

public class EventSFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSourceDefautl;
    [SerializeField] private AudioSource audioSourceRandomPitch;
    [SerializeField] private AudioSource audioSourceChangeVolume;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip[] getHitSounds;
    [SerializeField] private AudioClip[] dieSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip[] expSounds;
    [SerializeField] private AudioClip _slideClip;
    [SerializeField] private AudioClip _windClip;
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip _landClip;
    [SerializeField] private AudioClip _dropClip;
    private float _minIntervalFootsteps = 0f;
    private float _lastFootstepTime;
    private float _minIntervalLoot = 0.05f;
    private float _lastLootTime;

    private float _minIntervalGetHit = 0.05f;
    private float _lastGetHitTime;
    private float _minIntervalLand = 1f;
    private float _lastLadnTime;
    private int expIndexer = 0;

    public void ToggleWindSound(bool enable)
    {
        if (_windClip == null || audioSourceChangeVolume == null) return;

        if (enable)
        {
            if (!audioSourceChangeVolume.isPlaying || audioSourceChangeVolume.clip != _windClip)
            {
                audioSourceChangeVolume.clip = _windClip;
                audioSourceChangeVolume.volume = 0f;
                audioSourceChangeVolume.Play();
            }
            StopAllCoroutines();
            StartCoroutine(FadeAudio(audioSourceChangeVolume, 0.5f, 1f));
        }
        else
        {
            StopAllCoroutines();
            audioSourceChangeVolume.Stop();

            if (Time.time - _lastLadnTime < _minIntervalLand)
                return;
            _lastLadnTime = Time.time;

            PlayRandomPitch(_landClip, 0.3f);
        }
    }

    public void PlayAttackSound()
    {
        if (attackSounds.Length == 0) return;

        AudioClip clip = GetRandomSound(attackSounds);
        PlayRandomPitch(clip, 0.3f);
    }

    public void PlayFootstepSound()
    {
        if (Time.time - _lastFootstepTime < _minIntervalFootsteps)
            return;

        _lastFootstepTime = Time.time;

        AudioClip clip = GetRandomSound(footstepSounds);
        PlayRandomPitch(clip, 0.3f);
    }

    public void PlayGetHitSound()
    {
        if (Time.time - _lastGetHitTime < _minIntervalGetHit)
            return;

        _lastGetHitTime = Time.time;

        AudioClip clip = GetRandomSound(getHitSounds);
        PlayRandomPitch(clip, 0.3f);
    }

    public void PlayDieSound()
    {
        AudioClip clip = GetRandomSound(dieSounds);
        PlayDefaultSound(clip);
    }

    public void PlayJumpSound()
    {
        AudioClip clip = GetRandomSound(jumpSounds);
        PlayRandomPitch(clip, 0.3f);
    }

    public void PlayOnLandSound()
    {
        AudioClip clip = GetRandomSound(footstepSounds);
        PlayRandomPitch(clip, 0.6f);
    }

    public void PlaySliceSound()
    {
        ToggleSliceSound(true);
    }
    public void ToggleSliceSound(bool enable)
    {
        if (_slideClip == null || audioSourceChangeVolume == null) return;

        if (enable)
        {
            if (!audioSourceChangeVolume.isPlaying || audioSourceChangeVolume.clip != _slideClip)
            {
                audioSourceChangeVolume.clip = _slideClip;
                audioSourceChangeVolume.volume = 0f;
                audioSourceChangeVolume.Play();
            }
            StopAllCoroutines();
            StartCoroutine(FadeAudio(audioSourceChangeVolume, 1f, 1f));
        }
        else
        {
            StopAllCoroutines();
            audioSourceChangeVolume.Stop();
        }
    }

    public void PlayLootPickup()
    {
        if (Time.time - _lastLootTime < _minIntervalLoot)
            return;

        _lastLootTime = Time.time;

        expIndexer = PlayNextSound(expSounds, expIndexer);
    }


    public void PlayLootDroop()
    {
        if (Time.time - _lastLootTime < _minIntervalLoot)
            return;

        _lastLootTime = Time.time;

        PlayRandomPitch(_dropClip, 0.6f);
    }



    private void PlayDefaultSound(AudioClip sounds)
    {
        if (sounds == null) return;
        audioSourceDefautl.PlayOneShot(sounds);
    }
    private void PlayRandomPitch(AudioClip sounds, float variation)
    {
        if (sounds == null) return;

        float pitch = 1f + Random.Range(-variation, variation);
        audioSourceRandomPitch.pitch = pitch;

        audioSourceRandomPitch.PlayOneShot(sounds);
    }


    private AudioClip GetRandomSound(AudioClip[] sounds)
    {
        if (sounds == null || sounds.Length == 0) return null;

        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        return clip;
    }

    private int PlayNextSound(AudioClip[] sounds, int currentIndex)
    {
        int nextIndex = (currentIndex + 1) % sounds.Length;
        AudioClip clip = sounds[nextIndex];
        audioSourceDefautl.PlayOneShot(clip);
        return nextIndex;
    }

    private System.Collections.IEnumerator FadeAudio(AudioSource source, float duration, float targetVolume, bool stopAfterFade = false)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        source.volume = targetVolume;

        if (stopAfterFade) source.Stop();
    }

}
