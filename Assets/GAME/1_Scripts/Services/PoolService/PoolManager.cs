using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum PoolId
{
    ExpOrb,
    CoinOrb,
    Projectile,
    TracerPlayer,
    Hit,
    ExploseEffect,
    Dust_Default,
    Dust_Land,
    Hole,
    Indicator
}

public class PoolManager : MonoBehaviour, IInitializable
{
    [System.Serializable]
    public class PoolConfig
    {
        public PoolId id;
        public GameObject prefab;
        public int poolSize = 10;
        public Transform container;
    }

    private class PoolRuntime
    {
        public GameObject prefab;
        public Transform container;
        public Queue<GameObject> available = new();
        public HashSet<GameObject> inPool = new();
        public List<GameObject> allObjects = new();
    }

    [SerializeField] private List<PoolConfig> pools = new();
    private Dictionary<PoolId, PoolRuntime> runtimePools = new();

    public void Initialize()
    {
        if (G.PoolManager != null && G.PoolManager != this)
        {
            Destroy(gameObject);
            return;
        }

        CreatePools();

        G.PoolManager = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void CreatePools()
    {
        foreach (var config in pools)
        {
            if (config.prefab == null)
            {
                Debug.LogWarning($"Pool {config.id} has no prefab!");
                continue;
            }

            if (config.container == null)
            {
                var go = new GameObject(config.id.ToString());
                go.transform.SetParent(transform);
                config.container = go.transform;
            }

            var runtime = new PoolRuntime
            {
                prefab = config.prefab,
                container = config.container
            };

            for (int i = 0; i < config.poolSize; i++)
            {
                var obj = CreateObject(runtime);
                ReturnInternal(runtime, obj);
            }

            runtimePools[config.id] = runtime;
        }
    }

    private GameObject CreateObject(PoolRuntime pool)
    {
        var obj = Instantiate(pool.prefab, pool.container);
        obj.SetActive(false);

        pool.allObjects.Add(obj);
        return obj;
    }

    public GameObject Get(PoolId id, Vector3 position, float scale = 1f)
    {
        if (!runtimePools.TryGetValue(id, out var pool))
        {
            Debug.LogError($"Pool {id} not found!");
            return null;
        }

        GameObject obj;

        if (pool.available.Count > 0)
        {
            obj = pool.available.Dequeue();
            pool.inPool.Remove(obj);
        }
        else
        {
            obj = CreateObject(pool);
        }

        obj.transform.SetPositionAndRotation(position, Quaternion.identity);
        obj.transform.localScale = Vector3.one * scale;
        obj.SetActive(true);

        return obj;
    }

    public void Return(PoolId id, GameObject obj)
    {
        if (!runtimePools.TryGetValue(id, out var pool))
        {
            Destroy(obj);
            return;
        }

        if (pool.inPool.Contains(obj))
            return; // защита от двойного возврата

        ReturnInternal(pool, obj);
    }

    private void ReturnInternal(PoolRuntime pool, GameObject obj)
    {
        obj.SetActive(false);
        pool.available.Enqueue(obj);
        pool.inPool.Add(obj);
    }

    public void CallWithAutoReturn(PoolId id, Vector3 pos, float duration, float scale = 1f)
    {
        var obj = Get(id, pos, scale);
        if (obj != null)
            StartCoroutine(ReturnCoroutine(id, obj, duration));
    }

    private IEnumerator ReturnCoroutine(PoolId id, GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        Return(id, obj);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DeactivateAll();
    }

    public void DeactivateAll()
    {
        foreach (var pool in runtimePools.Values)
        {
            foreach (var obj in pool.allObjects)
            {
                if (obj == null) continue;

                if (!pool.inPool.Contains(obj))
                    ReturnInternal(pool, obj);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var pool in runtimePools.Values)
        {
            foreach (var obj in pool.allObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
        }

        runtimePools.Clear();
    }
}