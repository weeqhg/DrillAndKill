using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;

public class Thumper : BaseInteractable
{
    [SerializeField] private Transform thumper;
    [SerializeField] private LocalizedString localizedHint;
    private CameraShake cameraShake;
    private bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;
    private static bool isBossDefeated = false;
    private Sequence loopSeq;
    
    //Sounds
    private SoundData earthquakeSound;
    private SoundData landSound;
    private SoundData upSound;



    protected override void SetupDerived()
    {
        earthquakeSound = Resources.Load<SoundData>("Audio/SFX/Earthquake");
        landSound = Resources.Load<SoundData>("Audio/SFX/LandBigObject");
        upSound = Resources.Load<SoundData>("Audio/SFX/UpObject");
    }

    public override void Interact(PlayerInteractor playerInteractor)
    {
        if (isUsed) return;
        isUsed = true;

        cameraShake = playerInteractor.gameObject.GetComponentInChildren<CameraShake>();
        StartCoroutine(SpawnCoroutine());

        loopSeq = CreateImpactLoop();
        loopSeq.SetLoops(-1, LoopType.Restart);
    }

    public override string GetHint()
    {
        if (localizedHint != null) return localizedHint.GetLocalizedString();
        else return "";
    }

    private IEnumerator SpawnCoroutine()
    {
        G.AudioManager?.Play(earthquakeSound);

        if (isBossDefeated) G.WorldManager?.CallBoerNextWorld();

        yield return WaitWithPause(9f);

        thumper.localPosition = new Vector3(0f, 3f, 0f);
        loopSeq.Kill();

        G.WorldManager?.CallBossWorld();

        if (!isBossDefeated)
        {
            G.WorldManager.OnBossDefeated += OnBossDefeated;
        }

        G.AudioManager?.Play(upSound);
    }

    private Sequence CreateImpactLoop()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(thumper.DOPunchPosition(Vector3.up * 3f, 0.8f, 1, 0));

        seq.InsertCallback(0.6f, () =>
        {
            G.AudioManager?.Play(landSound);
            cameraShake.ShakeLight(3f);
            G.PoolManager.CallWithAutoReturn(PoolId.Dust_Land, thumper.position + Vector3.up * 7f, 1f, 6f);
        });
        return seq;
    }

    private void OnBossDefeated()
    {
        isBossDefeated = true;
        G.WorldManager.OnBossDefeated -= OnBossDefeated;
    }

    private IEnumerator WaitWithPause(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (IsStoped)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private void OnDestroy()
    {
        thumper.position = Vector3.zero;
        if (loopSeq != null) loopSeq.Kill();
        if (isBossDefeated) isBossDefeated = false;
    }
}
