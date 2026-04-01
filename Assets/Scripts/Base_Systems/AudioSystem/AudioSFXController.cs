using UnityEngine;

public enum TypeSFX
{
    Explose
}
public class AudioSFXController : MonoBehaviour
{
    [Header("SXF Sound")]
    [SerializeField] private AudioSource _defaultSFX;
    [SerializeField] private AudioSource _randomSFX;
    public AudioClip[] _explose;

    private float pitchVariation = 0.3f;
    private float _lastExplose;
    private float _minIntervalExplose = 0.1f;

    public void Init()
    {

    }

    public void PlayAudioSFX(TypeSFX type)
    {
        switch (type)
        {
            case TypeSFX.Explose:
                PlayExplose();
                break;
        }
    }

    private void PlayExplose()
    {
        if (Time.time - _lastExplose < _minIntervalExplose)
            return;

        _lastExplose = Time.time;

        float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        _randomSFX.pitch = pitch;
        AudioClip clip = _explose[Random.Range(0, _explose.Length)];

        _randomSFX.PlayOneShot(clip);
    }
}

