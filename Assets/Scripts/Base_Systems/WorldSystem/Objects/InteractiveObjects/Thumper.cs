using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Thumper : BaseInteractable
{
    [SerializeField] private Transform thumper;
    private CameraShake cameraShake;
    private bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;
    private static bool isBossDefeated = false;
    private Sequence loopSeq;
    protected override void SetupDerived() { }

    public override void Interact(PlayerInteractor playerInteractor)
    {
        if (isUsed) return;
        isUsed = true;

        cameraShake = playerInteractor.gameObject.GetComponentInChildren<CameraShake>();
        StartCoroutine(SpawnCoroutine());

        loopSeq = CreateImpactLoop();
        loopSeq.SetLoops(-1, LoopType.Restart);
    }

    private IEnumerator SpawnCoroutine()
    {
        AudioManager.Instance.PlayAudiDurationSFX(TypeSFX.Earthquake, 9f, 0f, 1f, true);

        if (isBossDefeated) GameEvents.BoerLaunch();

        yield return WaitWithPause(9f);

        thumper.localPosition = new Vector3(0f, 3f, 0f);
        loopSeq.Kill();

        GameEvents.EnemySpawnWithType(TypeEnemy.Boss, 1);
        GameEvents.BossStartFight();

        if (!isBossDefeated)
        {
            GameEvents.OnBossDefeated += OnBossDefeated;
        }

        AudioManager.Instance.PlayAudioSFX(TypeSFX.UpObject);
    }

    private Sequence CreateImpactLoop()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(thumper.DOPunchPosition(Vector3.up * 3f, 0.8f, 1, 0));

        seq.InsertCallback(0.6f, () =>
        {
            AudioManager.Instance.PlayAudioSFX(TypeSFX.LandObject);
            cameraShake.ShakeLight(3f);
            PoolManager.Instance.CallWithAutoReturn(PoolId.Dust_Land, thumper.position + Vector3.up * 7f, 1f, 6f);
        });
        return seq;
    }

    private void OnBossDefeated()
    {
        isBossDefeated = true;
        GameEvents.BoerLaunch();
        GameEvents.OnBossDefeated -= OnBossDefeated;
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
