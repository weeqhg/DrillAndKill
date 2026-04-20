using UnityEngine;

public class EventSFX : MonoBehaviour
{
    private SoundData footStepData;
    private SoundData gethitData;
    private SoundData dieData;
    private SoundData jumpData;



    private void Start()
    {
        footStepData = Resources.Load<SoundData>("Audio/SFX/Footstep");
        gethitData = Resources.Load<SoundData>("Audio/SFX/GetHit");
        dieData = Resources.Load<SoundData>("Audio/SFX/Die");
        jumpData = Resources.Load<SoundData>("Audio/SFX/Jump");
    }

    public void PlayFootstepSound()
    {
        G.AudioManager?.Play(footStepData);
    }

    public void PlayGetHitSound()
    {
        G.AudioManager?.Play(gethitData);
    }

    public void PlayDieSound()
    {
        G.AudioManager?.Play(dieData);
    }

    public void PlayJumpSound()
    {
        G.AudioManager?.Play(jumpData);
    }

    public void PlayOnLandSound()
    {
        G.AudioManager?.Play(footStepData);
    }
}
