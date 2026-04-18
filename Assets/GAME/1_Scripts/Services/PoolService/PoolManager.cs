using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
public enum PoolId
{
    ExpOrb,
    CoinOrb,
    Projectile,
    TracerPlayer,
    TracerEnemy,
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
    public class Pool
    {
        public PoolId id;                    // Уникальный идентификатор пула
        public GameObject prefab;            // Префаб объекта
        public int poolSize = 10;            // Размер пула
        public Transform container;          // Контейнер для объектов (опционально)
    }

    [SerializeField] private List<Pool> pools = new();
    private Dictionary<PoolId, Queue<GameObject>> poolDictionary = new();
    private Dictionary<PoolId, List<GameObject>> allPoolObjects = new();
    private Dictionary<PoolId, GameObject> prefabDictionary = new();

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
        foreach (var pool in pools)
        {
            if (pool.container == null)
            {
                GameObject containerObj = new GameObject(pool.id.ToString());
                containerObj.transform.SetParent(transform);
                pool.container = containerObj.transform;
            }

            var queue = new Queue<GameObject>();
            var list = new List<GameObject>();

            for (int i = 0; i < pool.poolSize; i++)
            {
                var obj = Instantiate(pool.prefab, pool.container);
                obj.SetActive(false);

                queue.Enqueue(obj);
                list.Add(obj);
            }

            poolDictionary[pool.id] = queue;
            allPoolObjects[pool.id] = list;
            prefabDictionary[pool.id] = pool.prefab;
        }
    }
    public GameObject Get(PoolId id, Vector3 position, float scale = 1f)
    {
        if (!poolDictionary.ContainsKey(id))
            return null;

        GameObject obj;

        if (poolDictionary[id].Count > 0)
        {
            obj = poolDictionary[id].Dequeue();
        }
        else
        {
            GameObject prefab = prefabDictionary[id];
            Transform container = pools.Find(p => p.id == id).container;

            // Инстанцируем в контейнер
            obj = Instantiate(prefab, container);
            obj.SetActive(false);

            allPoolObjects[id].Add(obj);
        }

        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * scale;
        obj.SetActive(true);
        return obj;
    }


    public void Return(PoolId id, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(id))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        poolDictionary[id].Enqueue(obj);
    }

    public void CallWithAutoReturn(PoolId id, Vector3 pos, float duration, float scale = 1f)
    {
        GameObject obj = Get(id, pos, scale);
        StartCoroutine(ReturnToPool(id, obj, duration));
    }

    private IEnumerator ReturnToPool(PoolId id, GameObject obj, float duration = 1f)
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
        foreach (var kvp in allPoolObjects)
        {
            PoolId id = kvp.Key;

            foreach (var obj in kvp.Value)
            {
                if (obj == null) continue;

                obj.SetActive(false);

                poolDictionary[id].Enqueue(obj);
            }
        }
    }
    private void ClearAllPools()
    {
        foreach (var kvp in allPoolObjects)
        {
            foreach (var obj in kvp.Value)
            {
                if (obj != null)
                    Destroy(obj);
            }
        }

        poolDictionary.Clear();
        allPoolObjects.Clear();
        prefabDictionary.Clear();
    }

    private void OnDestroy()
    {
        ClearAllPools();
    }
}