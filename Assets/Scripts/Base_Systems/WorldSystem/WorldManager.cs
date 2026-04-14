using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public void Initialize()
    {
        SpawnObjectWorld spawnObjectWorld = gameObject.GetComponentInChildren<SpawnObjectWorld>();
        spawnObjectWorld?.Initialize();
        
        MainEnemyManager mainEnemyManager = gameObject.GetComponentInChildren<MainEnemyManager>();
        mainEnemyManager?.Initialize();
    }
}
