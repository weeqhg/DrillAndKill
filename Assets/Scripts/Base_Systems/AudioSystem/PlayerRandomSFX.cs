using UnityEngine;

public class PlayerRandomSFX : MonoBehaviour
{
    [SerializeField] private AudioClip[] sounds;
    [SerializeField] private AudioSource audioSource;
    private float pitchVariation = 0.3f;

    public void PlayRandomSound()
    {
        if (sounds.Length == 0) return;

        AudioClip clip = sounds[Random.Range(0, sounds.Length)];

        float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip);
    }
}
