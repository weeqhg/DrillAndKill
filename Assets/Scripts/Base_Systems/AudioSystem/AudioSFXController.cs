using UnityEngine;

namespace WekenDev.AudioManagerGame
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSFXController : MonoBehaviour
    {
        [Header("SXF Sound")]
        private AudioSource _audio;
        public AudioClip _rewardSound;
        public void Init()
        {
            _audio = GetComponent<AudioSource>();
        }

        public void PlaySoundClear()
        {
            _audio.Play();
        }

        public void StopSoundClear()
        {
            _audio.Stop();
        }
        public void PlayReward()
        {
            _audio.PlayOneShot(_rewardSound);
        }
    }
}
