using UnityEngine;

public static class SystemInitializer
{
    public static T CreateSystem<T>(T current, T prefab) where T : MonoBehaviour
    {
        if (current != null || prefab == null) return current;

        T instance = Object.Instantiate(prefab);
        if (instance is IInitializable init) init.Initialize();

        return instance;
    }

    public static T InitializeSystem<T>(Transform parent) where T : MonoBehaviour
    {
        T instance = parent.GetComponentInChildren<T>(true);
        if (instance is IInitializable init) init.Initialize();

        return instance;
    }
}
