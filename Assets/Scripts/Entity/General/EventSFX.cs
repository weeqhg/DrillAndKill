using UnityEngine;

public class EventSFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSourceDefautl;
    [SerializeField] private AudioSource audioSourceRandomPitch;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip[] getHitSounds;
    [SerializeField] private AudioClip[] dieSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip[] expSounds;
    [SerializeField] private AudioClip _slideClip;
    private float _lastFootstepTime;
    private float _minIntervalFootsteps = 0f;
    private float _lastExpPickupTime;
    private float _minIntervalExpPickup = 0.05f;
    private int expIndexer = 0;

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
        AudioClip clip = GetRandomSound(getHitSounds);
        PlayDefaultSound(clip);
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
        if (_slideClip == null) return;

        audioSourceDefautl.clip = _slideClip;
        audioSourceDefautl.loop = true;
        audioSourceDefautl.Play();
    }
    public void StopSlideSound()
    {
        audioSourceDefautl.loop = false;
        audioSourceDefautl.Stop();
    }
    public void PlayExpPickup()
    {
        if (Time.time - _lastExpPickupTime < _minIntervalExpPickup)
            return;

        _lastExpPickupTime = Time.time;

        expIndexer = PlayNextSound(expSounds, expIndexer);
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

}
