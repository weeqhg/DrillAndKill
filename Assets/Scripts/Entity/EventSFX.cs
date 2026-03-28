using UnityEngine;

public class EventSFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip[] getHitSounds;
    [SerializeField] private AudioClip[] dieSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip _slideClip;
    private float _lastFootstepTime;
    private float _minInterval = 0.2f;
    public void Initialize()
    {

    }

    public void PlayFootstepSound()
    {
        if (Time.time - _lastFootstepTime < _minInterval)
            return;

        _lastFootstepTime = Time.time;

        PlayRandomSound(footstepSounds);
    }

    public void PlayGetHitSound()
    {
        PlayRandomSound(getHitSounds);
    }

    public void PlayDieSound()
    {
        PlayRandomSound(dieSounds);
    }

    public void PlayJumpSound()
    {
        PlayRandomSound(jumpSounds);
    }

    public void PlayOnLandSound()
    {
        PlayRandomSound(footstepSounds);
    }

    public void PlaySliceSound()
    {
        if (_slideClip == null) return;

        audioSource.clip = _slideClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopSlideSound()
    {
        audioSource.loop = false;
        audioSource.Stop();
    }

    private void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds == null || sounds.Length == 0) return;

        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        audioSource.PlayOneShot(clip);
    }
}
