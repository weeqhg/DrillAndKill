using System.Collections;
using UnityEngine;

public class WeaponVFX : MonoBehaviour
{
    private PoolId tracerId = PoolId.TracerPlayer;
    private PoolId hitId = PoolId.Hit;
    [SerializeField] private ParticleSystem[] muzzleFlashes;

    private float tracerSpeed = 300f;

    public void PlayTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = PoolManager.Instance.Get(tracerId, start);

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

        PoolManager.Instance.Return(tracerId, tracer);
    }

    public void PlayImpact(Vector3 pos, Vector3 normal)
    {
        GameObject impact = PoolManager.Instance.Get(hitId, pos);
        impact.transform.rotation = Quaternion.LookRotation(normal);
        var ps = impact.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();

        StartCoroutine(ReturnImpactAfter(impact, 1f));
    }

    IEnumerator ReturnImpactAfter(GameObject impact, float delay)
    {
        yield return new WaitForSeconds(delay);

        PoolManager.Instance.Return(hitId, impact);
    }

    public void PlayMuzzleFlash(int index)
    {
        if (index < 0 || index >= muzzleFlashes.Length) return;

        muzzleFlashes[index].Play();
    }
}