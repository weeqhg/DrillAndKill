using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    private PoolManager pool;
    private SoundData exploseSound;
    private float damage;
    private float explosionRadius;
    private bool IsStoped => GamePause.IsGameFrozen || GamePause.IsGamePaused;
    public void Init(PoolManager pool, Vector3 target, float speed, float arcHeight, float damage, float explosionRadius)
    {
        exploseSound = Resources.Load<SoundData>("Audio/SFX/Explose");

        this.pool = pool;
        this.damage = damage;
        this.explosionRadius = explosionRadius;

        StartCoroutine(Move(target, speed, arcHeight));
    }


    private IEnumerator Move(Vector3 target, float speed, float arcHeight)
    {
        Vector3 startPos = transform.position;

        float distance = Vector3.Distance(startPos, target);
        float duration = distance / speed;

        float elapsed = 0;

        while (elapsed < duration)
        {
            if (IsStoped)
            {
                yield return null; // Ждём один кадр и проверяем снова
                continue; // Пропускаем обновление позиции в этом кадре
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 pos = Vector3.Lerp(startPos, target, t);
            pos.y += arcHeight * Mathf.Sin(Mathf.PI * t);

            transform.position = pos;

            yield return null;
        }

        Explode(target);
        PlayImpact(target);
        G.AudioManager?.Play(exploseSound);
        pool.Return(PoolId.Projectile, gameObject);
    }

    private void Explode(Vector3 pos)
    {

        Collider[] hitColliders = Physics.OverlapSphere(pos, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                var damageable = hit.GetComponent<IDamageable>();
                damageable?.TakeDamage(damage);
            }
        }

    }

    private void PlayImpact(Vector3 pos)
    {
        GameObject impact = pool.Get(PoolId.ExploseEffect, pos);
        var ps = impact.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();

        StartCoroutine(ReturnImpactAfter(impact, 1f));
    }

    private IEnumerator ReturnImpactAfter(GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        pool.Return(PoolId.ExploseEffect, obj);
    }
}