using UnityEngine;

public class MainEnemyManager : MonoBehaviour
{
    public void Initialize()
    {
        EnemySpawner enemySpawner = GetComponentInChildren<EnemySpawner>();
        enemySpawner?.Initialize();

        EnemyScaler enemyScaler = GetComponentInChildren<EnemyScaler>();
        enemyScaler?.Initialize();

    }
}
