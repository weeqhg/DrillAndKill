using UnityEngine;
using System.Collections.Generic;

public class EnemyVFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionPrefab;   
    private Queue<ParticleSystem> _pool = new Queue<ParticleSystem>();
    
    public void PlayImpact(Vector3 position)
    {
        var vfx = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(explosionPrefab, transform);
        
        vfx.transform.position = position;
        vfx.Play();
        
        StartCoroutine(ReturnToPool(vfx));
    }
    
    private System.Collections.IEnumerator ReturnToPool(ParticleSystem vfx)
    {
        yield return new WaitForSeconds(vfx.main.duration);
        _pool.Enqueue(vfx);
    }
}