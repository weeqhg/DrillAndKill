using UnityEngine;
using System.Collections.Generic;
public enum PoolId
{
    ExpOrb,
    Projectile,
    TracerPlayer,
    TracerEnemy,
    Hit,
    ExploseEffect
}
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

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
    private Dictionary<PoolId, GameObject> prefabDictionary = new();

    private void Awake() => InitializeSingleton();
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Initialize()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var pool in pools)
        {
            var queue = new Queue<GameObject>();

            // Создаём объекты пула
            for (int i = 0; i < pool.poolSize; i++)
            {
                var obj = Instantiate(pool.prefab, pool.container);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            poolDictionary[pool.id] = queue;
            prefabDictionary[pool.id] = pool.prefab;
        }
    }
    public GameObject Get(PoolId id, Vector3 position)
    {
        if (!poolDictionary.ContainsKey(id))
        {
            Debug.LogError($"Pool '{id}' not found!");
            return null;
        }

        GameObject obj;

        if (poolDictionary[id].Count > 0)
        {
            obj = poolDictionary[id].Dequeue();
        }
        else
        {
            GameObject prefab = prefabDictionary[id];

            // Получаем контейнер для данного пула
            Transform container = pools.Find(p => p.id == id).container;

            // Инстанцируем в контейнер
            obj = Instantiate(prefab, container);
            obj.SetActive(false);

            // Добавляем в очередь, чтобы пул контролировал все объекты
            poolDictionary[id].Enqueue(obj);

            // Выдаём объект сразу
            obj = poolDictionary[id].Dequeue();
        }

        obj.transform.position = position;
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

    public void ClearAllPools()
    {
        foreach (var queue in poolDictionary.Values)
        {
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                if (obj != null)
                    Destroy(obj);
            }
        }

        poolDictionary.Clear();
        prefabDictionary.Clear();
    }

    private void OnDestroy()
    {
        ClearAllPools();
    }
}