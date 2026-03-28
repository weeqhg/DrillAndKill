using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVFX : MonoBehaviour
{
    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem[] muzzleFlashes;

    private int tracerPoolSize = 10;
    private int impactPoolSize = 10;

    private float tracerSpeed = 300f;

    private Queue<GameObject> _tracerPool = new Queue<GameObject>();
    private Queue<ParticleSystem> _impactPool = new Queue<ParticleSystem>();

    private Transform _playerTracerContainer;

    public void Initialize()
    {
        if (_playerTracerContainer == null)
        {
            GameObject container = new GameObject("--- Tracers Container ---");
            _playerTracerContainer = container.transform;
        }

        if (tracerPrefab != null) CreateTracerPool();
        if (hitEffect != null) CreateImpactPool();
    }

    #region Pool Creation

    private void CreateTracerPool()
    {
        for (int i = 0; i < tracerPoolSize; i++)
        {
            GameObject tracer = Instantiate(tracerPrefab, _playerTracerContainer);
            tracer.SetActive(false);
            _tracerPool.Enqueue(tracer);
        }
    }

    private void CreateImpactPool()
    {
        for (int i = 0; i < impactPoolSize; i++)
        {
            ParticleSystem impact = Instantiate(hitEffect, transform);
            impact.gameObject.SetActive(false);
            _impactPool.Enqueue(impact);
        }
    }

    #endregion

    #region Get from Pool

    private GameObject GetTracer()
    {
        var tracer = _tracerPool.Count > 0 ? _tracerPool.Dequeue() : Instantiate(tracerPrefab, _playerTracerContainer);

        return tracer;
    }

    private ParticleSystem GetImpact()
    {
        var impact = _impactPool.Count > 0 ? _impactPool.Dequeue() : Instantiate(hitEffect, _playerTracerContainer);

        return impact;
    }

    #endregion

    #region Return to Pool

    private void ReturnTracer(GameObject tracer)
    {
        tracer.SetActive(false);
        _tracerPool.Enqueue(tracer);
    }

    private void ReturnImpact(ParticleSystem impact)
    {
        impact.gameObject.SetActive(false);
        _impactPool.Enqueue(impact);
    }

    #endregion

    #region Public Methods

    public void PlayTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = GetTracer();

        tracer.transform.position = start;
        tracer.SetActive(true);

        StartCoroutine(MoveTracer(tracer, end));
    }

    IEnumerator MoveTracer(GameObject tracer, Vector3 target)
    {
        float elapsed = 0;
        float distance = Vector3.Distance(tracer.transform.position, target);
        float duration = distance / tracerSpeed;

        while (elapsed < duration)
        {
            if (tracer == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            tracer.transform.position = Vector3.Lerp(tracer.transform.position, target, t);

            yield return null;
        }

        tracer.transform.position = target;

        yield return new WaitForSeconds(0.05f);

        ReturnTracer(tracer);
    }

    public void PlayImpact(Vector3 pos, Vector3 normal)
    {
        ParticleSystem impact = GetImpact();
        impact.transform.position = pos;
        impact.transform.rotation = Quaternion.LookRotation(normal);
        impact.Play();

        StartCoroutine(ReturnImpactAfter(impact, 1f));
    }

    IEnumerator ReturnImpactAfter(ParticleSystem impact, float delay)
    {
        yield return new WaitForSeconds(delay);
        impact.Stop();
        ReturnImpact(impact);
    }

    public void PlayMuzzleFlash(int index)
    {
        if (index < 0 || index >= muzzleFlashes.Length) return;

        muzzleFlashes[index].Play();
    }

    #endregion
}