using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("Mixer")]
    public AudioMixerGroup mixerGroup;
    [Header("Clips")]
    public AudioClip[] clips;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Pitch Settings")]
    public bool randomPitch = true;
    public float pitchVariation = 0.1f;

    [Header("Fade")]
    public float fadeIn = 0f;
    public float fadeOut = 0f;

    [Header("3D Settings")]
    public bool is3D = false;
    public float minDistance = 1f;
    public float maxDistance = 500f;

    [Header("Settings")]
    public bool loop = false;
    public float minInterval = 0f;

    [Header("Priority")]
    [Range(0, 256)]
    public int priority = 128;


    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;

        return clips[Random.Range(0, clips.Length)];
    }
}
