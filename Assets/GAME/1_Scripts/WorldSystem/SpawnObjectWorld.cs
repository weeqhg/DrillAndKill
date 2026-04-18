using System.Collections.Generic;
using UnityEngine;

public class SpawnObjectWorld : MonoBehaviour
{
    [Header("Настройка спавна")]
    [Tooltip("Радиус центральной зоны, где объекты не будут спавниться")]
    [SerializeField] private Terrain terrain;
    public Transform containerInteract;
    public Transform containerStatic;
    public float centerSafeZoneRadius = 25f;
    [Tooltip("Включить безопасную зону в центре")]
    public bool enableCenterSafeZone = true;

    [Header("Настройка объектов")]
    [SerializeField] private List<SpawnedObject> staticObjectDatas = new();
    [SerializeField] private List<SpawnedObject> interactiveObjectDatas = new();

    private List<GameObject> interactiveSpawnedObjects = new();
    private List<GameObject> staticSpawnedObjects = new();


    [System.Serializable]
    public class SpawnedObject
    {
        public string objectName;
        public GameObject prefab;
        public int count = 5;
        public float yOffset = 0f;
        public bool alignToTerrain = false;
        [Range(0f, 20f)] public float radius = 0f;
        [Range(0f, 100f)] public float randomScale = 0f;
        [Range(0f, 5f)] public float spawnDensity = 0.5f;
    }

    public void Initialize()
    {
        SpawnAllInteractive();
        ConsoleEvents.OnCommandObjectSpawn += SpawnInteractiveHandler;
    }

    public void SpawnAllInteractive()
    {
        ClearInteractiveObject();

        foreach (var spawn in interactiveObjectDatas)
        {
            for (int i = 0; i < spawn.count; i++)
            {
                TrySpawnObject(spawn, true);
            }
        }
    }

    private void SpawnInteractiveHandler(int id, int count)
    {
        for (int i = 0; i < count; i++)
        {
            TrySpawnObject(interactiveObjectDatas[id], true);
        }
    }

    private void ClearInteractiveObject()
    {
        foreach (var obj in interactiveSpawnedObjects)
        {
            if (obj == null) continue;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        interactiveSpawnedObjects.Clear();
    }


    private void TrySpawnObject(SpawnedObject spawn, bool isInteractive)
    {
        Vector3 size = terrain.terrainData.size;
        Vector3 pos = terrain.transform.position;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float spawnX = Random.Range(0f, size.x) + pos.x;
            float spawnZ = Random.Range(0f, size.z) + pos.z;

            Vector3 terrainPos = new Vector3(spawnX, 0, spawnZ);
            float terrainHeight = terrain.SampleHeight(terrainPos) + terrain.transform.position.y;

            Vector3 worldPos = new Vector3(spawnX, terrainHeight, spawnZ);

            // Safe zone
            if (enableCenterSafeZone && IsPositionInSafeZone(worldPos))
                continue;

            // Проверка занятости
            if (IsPositionOccupied(worldPos, spawn.spawnDensity))
                continue;

            // Проверка рельефа
            if (spawn.radius > 0.1f)
            {
                if (!IsTerrainFlatEnough(worldPos, spawn.radius, out float y))
                    continue;
            }

            // Спавн
            if (isInteractive) SpawnInteractive(spawn, worldPos);
            else SpawnStaticObject(spawn, worldPos);
            return;
        }
    }

    private bool IsPositionOccupied(Vector3 position, float radius)
    {
        float sqrRadius = radius * radius;

        // interactive
        foreach (var obj in interactiveSpawnedObjects)
        {
            if (obj == null) continue;

            if ((obj.transform.position - position).sqrMagnitude < sqrRadius)
                return true;
        }

        // Static
        foreach (var obj in staticSpawnedObjects)
        {
            if (obj == null) continue;

            if ((obj.transform.position - position).sqrMagnitude < sqrRadius)
                return true;
        }

        return false;
    }

    private void SpawnInteractive(SpawnedObject spawn, Vector3 position)
    {
        Ray ray = new Ray(position + Vector3.up * 500f, Vector3.down);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return;

        float randomY = Random.Range(0f, 360f);

        Quaternion rotation;

        if (spawn.alignToTerrain)
        {
            rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            rotation *= Quaternion.Euler(0f, randomY, 0f);
        }
        else
        {
            rotation = Quaternion.Euler(0f, randomY, 0f);
        }

        GameObject obj = Instantiate(spawn.prefab, Vector3.zero, rotation);

        // scale
        float scale = 1f;
        if (spawn.randomScale > 0f)
        {
            scale = 1f + Random.Range(0, spawn.randomScale);
        }
        obj.transform.localScale = Vector3.one * scale;

        // ставим позицию
        obj.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);

        obj.transform.position += Vector3.up * spawn.yOffset;

        obj.transform.SetParent(containerInteract);

        BaseInteractable interactableObj = obj.GetComponent<BaseInteractable>();

        if (interactableObj != null) interactableObj.Initialize();

        interactiveSpawnedObjects.Add(obj);
    }

    private void SpawnStaticObject(SpawnedObject spawn, Vector3 position)
    {
        Ray ray = new Ray(position + Vector3.up * 500f, Vector3.down);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return;

        float randomY = Random.Range(0f, 360f);

        Quaternion rotation;

        if (spawn.alignToTerrain)
        {
            rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            rotation *= Quaternion.Euler(0f, randomY, 0f);
        }
        else
        {
            rotation = Quaternion.Euler(0f, randomY, 0f);
        }

        GameObject obj = Instantiate(spawn.prefab, Vector3.zero, rotation);

        // scale
        float scale = 1f;
        if (spawn.randomScale > 0f)
        {
            scale = 1f + Random.Range(0, spawn.randomScale);
        }
        obj.transform.localScale = Vector3.one * scale;

        // 🔥 получаем радиус объекта
        float objectRadius = GetObjectRadius(obj);

        // 🔥 ищем НИЖНЮЮ точку земли
        float groundY = GetGroundMinY(hit.point, objectRadius);

        // ставим позицию
        obj.transform.position = new Vector3(hit.point.x, groundY, hit.point.z);

        float yOffset = GetObjectBottomOffset(obj);
        obj.transform.position += Vector3.up * (spawn.yOffset);

        obj.transform.SetParent(containerStatic);

        staticSpawnedObjects.Add(obj);
    }

    private float GetObjectBottomOffset(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return 0f;

        Bounds combinedBounds = renderers[0].bounds;

        foreach (var r in renderers)
        {
            combinedBounds.Encapsulate(r.bounds);
        }

        // половина высоты объекта
        return combinedBounds.extents.y;
    }

    private float GetGroundMinY(Vector3 center, float radius)
    {
        int checks = 6;
        float minY = float.MaxValue;

        // центр тоже проверяем
        if (Physics.Raycast(center + Vector3.up * 500f, Vector3.down, out RaycastHit centerHit, 1000f))
        {
            minY = centerHit.point.y;
        }

        for (int i = 0; i < checks; i++)
        {
            float angle = i * (360f / checks);

            float x = center.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = center.z + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            Vector3 rayStart = new Vector3(x, center.y + 500f, z);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 1000f))
            {
                if (hit.point.y < minY)
                    minY = hit.point.y;
            }
        }

        return minY;
    }

    private float GetObjectRadius(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return 1f;

        Bounds bounds = renderers[0].bounds;

        foreach (var r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return Mathf.Max(bounds.extents.x, bounds.extents.z);
    }

    private bool IsPositionInSafeZone(Vector3 position)
    {
        Vector3 terrainCenter = terrain.transform.position + terrain.terrainData.size / 2f;
        Vector2 positionXZ = new Vector2(position.x, position.z);

        Vector2 centerXZ = new Vector2(terrainCenter.x, terrainCenter.z);
        float distanceToCenter = Vector2.Distance(centerXZ, positionXZ);

        return distanceToCenter <= centerSafeZoneRadius;
    }

    private bool IsTerrainFlatEnough(Vector3 centerPosition, float radius, out float finalY)
    {
        int checkPoints = 8;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < checkPoints; i++)
        {
            float angle = i * (360f / checkPoints);
            float x = centerPosition.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = centerPosition.z + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            Vector3 rayStart = new Vector3(x, centerPosition.y + 500f, z);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 1000f))
            {
                float y = hit.point.y;

                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
            else
            {
                finalY = 0;
                return false;
            }
        }

        // 🔥 проверка "плоскости"
        float heightDiff = maxY - minY;

        if (heightDiff > 2f) // настрой под себя
        {
            finalY = 0;
            return false;
        }

        finalY = minY; // ставим в самую низкую точку
        return true;
    }

    [ContextMenu("Spawn Objects")]
    public void SpawnStaticObjects()
    {
        ClearStaticObject();

        foreach (var spawn in staticObjectDatas)
        {
            for (int i = 0; i < spawn.count; i++)
            {
                TrySpawnObject(spawn, false);
            }
        }
    }

    [ContextMenu("Clear All Objects")]
    public void ClearStaticObject()
    {
        foreach (var obj in staticSpawnedObjects)
        {
            if (obj == null) continue;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        staticSpawnedObjects.Clear();
    }

    private void OnDestroy()
    {
        ConsoleEvents.OnCommandObjectSpawn -= SpawnInteractiveHandler;
    }
}
