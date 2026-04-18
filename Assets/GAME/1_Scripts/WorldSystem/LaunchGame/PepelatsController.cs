using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Localization;

public class PepelatsController : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LocalizedString localizedHint;
    private bool IsStoped => GamePause.IsGamePaused;
    private float duration = 2f;
    private float riseHeight = 30f;
    private float undergroundOffset = 20f;

    private Vector3 targetPos;
    private OutLine outLine;
    private bool isBusy;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private int poolSize = 5;

    private PlayerManager player;
    private CameraShake cameraShake;
    private bool isAvailable = false;
    private Vector3 posPlayer => PlayerService.Player != null ? player.Transform.position : Vector3.zero;


    public event Action OnBoerArrived;
    public event Action OnBoerDeparture;
    public event Action OnActiveNextLevel;

    //Sounds
    private SoundData earthquakeSound;
    private SoundData landSound;
    private SoundData upSound;



    public void Initialize()
    {
        ConsoleEvents.OnCommandLaunchPepelats += ForceLaunchBoer;

        if (PlayerService.Player != null) SetPlayer(PlayerService.Player);
        PlayerService.OnPlayerChanged += SetPlayer;

        outLine = GetComponent<OutLine>();
        outLine.SetActive(false);
        gameObject.SetActive(false);

        earthquakeSound = Resources.Load<SoundData>("Audio/SFX/Earthquake");
        landSound = Resources.Load<SoundData>("Audio/SFX/LandObject");
        upSound = Resources.Load<SoundData>("Audio/SFX/UpObject");
    }

    public string GetHint()
    {
        if (localizedHint != null) return localizedHint.GetLocalizedString();
        else return "";
    }

    private void SetPlayer(PlayerManager player)
    {
        if (player == null) return;

        this.player = player;
        cameraShake = this.player.CameraShake;
    }

    // 📌 Вызвать для появления
    public void NextLevelLaunch()
    {
        isAvailable = false;
        isBusy = false;
        gameObject.SetActive(true);

        StartCoroutine(LaunchCoroutine());
    }

    public void ForceLaunchBoer()
    {
        if (isBusy)
        {
            ConsoleEvents.ConsoleMessage("Bore is busy");
            return;
        }
        else
        {
            ConsoleEvents.ConsoleMessage("Bore is launching");
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            StartCoroutine(LaunchCoroutine());
        }
        else if (gameObject.activeSelf)
        {
            Despawn(() =>
            {
                gameObject.SetActive(true);
                StartCoroutine(LaunchCoroutine());
            });
        }
    }

    private IEnumerator LaunchCoroutine()
    {
        SoundHandle loopSound = G.AudioManager?.Play(earthquakeSound);
        cameraShake.ShakeHeavy(5f, 3f);

        yield return WaitWithPause(3f);

        G.AudioManager?.Stop(loopSound);
        Vector3 position = GetSpawnPositionInCone(30f, 50f, 30);

        transform.position = position - Vector3.up * undergroundOffset;
        transform.rotation = Quaternion.Euler(90, 0, 0);

        CreateHole(position);
        G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Default, position, 1f, 10f);
        G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Default, position, 1f, 10f);
        cameraShake.ShakeLight(5f);

        G.AudioManager?.Play(upSound);

        Vector3 endPos = position;

        Sequence seq = DOTween.Sequence();

        // Подъём
        seq.Append(transform.DOMoveY(endPos.y + riseHeight, duration * 0.6f).SetEase(Ease.OutCubic));

        seq.Join(transform.DORotate(new Vector3(-90, 0, 1080), duration * 0.8f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));

        // Падение (резкое)
        seq.Append(transform.DOMoveY(endPos.y, duration * 0.2f).SetEase(Ease.InCubic));

        seq.Join(transform.DORotate(new Vector3(0, 0, 1080), duration * 0.2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
        // Удар
        seq.Join(transform.DOPunchPosition(Vector3.down * 0.05f, 0.06f, 6, 1.5f));


        seq.OnComplete(() =>
        {
            G.AudioManager?.Play(landSound);

            cameraShake.ShakeLight(7f);

            G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Land, transform.position, 1f, 5f);

            isBusy = false;

            OnBoerArrived?.Invoke();
        });
    }

    // 📌 Вызвать для исчезновения
    public void Despawn(Action onComplete = null)
    {
        if (isBusy) return;
        isBusy = true;

        G.AudioManager?.Play(upSound);
        cameraShake.ShakeHeavy(5f, 3f);

        Vector3 startPos = transform.position;
        Vector3 endPos = targetPos - Vector3.up * undergroundOffset;

        G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Land, transform.position, 1f, 3f);
        G.PoolManager?.CallWithAutoReturn(PoolId.Dust_Land, transform.position, 1f, 3f);

        Sequence seq = DOTween.Sequence();

        // Небольшой подъём вверх (быстрый)
        seq.Append(transform.DOMoveY(startPos.y + riseHeight * 0.5f, duration * 0.2f)
            .SetEase(Ease.OutQuad));

        // Небольшой поворот (имитация прокручивания)
        seq.Join(transform.DORotate(new Vector3(0, 0, 1080), duration, RotateMode.LocalAxisAdd)
    .SetEase(Ease.Linear));

        // Резкое падение вниз
        seq.Join(transform.DOMoveY(endPos.y, duration * 0.5f)
        .SetEase(Ease.InBack));

        SoundHandle loopSound = G.AudioManager?.Play(earthquakeSound);

        seq.AppendInterval(1f);


        seq.OnComplete(() =>
        {
            G.AudioManager?.Stop(loopSound);
            gameObject.SetActive(false);
            isBusy = false;
            isAvailable = true;
            onComplete?.Invoke();
        });
    }

    public void Interact(PlayerInteractor playerInteractor)
    {
        if (isAvailable)
        {
            OnActiveNextLevel?.Invoke();

            Despawn(() =>
            {
                OnBoerDeparture?.Invoke();
            });
            OnLoseFocus();
        }
    }
    public bool IsUsed()
    {
        return isBusy || !isAvailable;
    }
    public void OnFocus()
    {
        if (isBusy) return;

        outLine.SetActive(true);
    }

    public void OnLoseFocus()
    {
        outLine.SetActive(false);
    }

    private Vector3 GetSpawnPositionInCone(float minRadius, float maxRadius, float angleRange = 90f, int attempts = 10)
    {
        for (int i = 0; i < attempts; i++)
        {
            float dist = UnityEngine.Random.Range(minRadius, maxRadius);

            // угол в пределах конуса
            float angle = UnityEngine.Random.Range(-angleRange / 2f, angleRange / 2f);

            Vector3 dir = Quaternion.Euler(0, angle, 0) * (player != null ? player.Transform.forward : Vector3.forward);

            Vector3 origin = posPlayer + dir * dist + Vector3.up * 5f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                Vector3 spawnPos = hit.point;

                if (!Physics.CheckSphere(spawnPos, 1.5f, ~groundLayer))
                {
                    return spawnPos;
                }
            }
        }

        return player.Transform.position;
    }

    private void CreateHole(Vector3 position)
    {
        RaycastHit hit;
        Vector3 normal = Vector3.up;

        if (Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out hit, 10f))
            normal = hit.normal;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        if (pool.Count >= poolSize)
        {
            GameObject oldest = pool.Dequeue();
            G.PoolManager?.Return(PoolId.Hole, oldest);
        }

        GameObject obj = G.PoolManager?.Get(PoolId.Hole, position);
        obj.transform.rotation = rotation;
        pool.Enqueue(obj);
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
        PlayerService.OnPlayerChanged -= SetPlayer;
        ConsoleEvents.OnCommandLaunchPepelats -= ForceLaunchBoer;
    }
}
