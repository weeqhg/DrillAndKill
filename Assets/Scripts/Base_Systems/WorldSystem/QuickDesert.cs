using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class QuickDesert : MonoBehaviour
{
    [Header("Dune Settings")]
    [SerializeField] private float duneFrequency = 10f;  // Частота дюн (чем выше, тем чаще)
    [SerializeField] private float duneHeight = 0.3f;    // Высота дюн
    [SerializeField] private float baseHeight = 0.1f;    // Базовая высота
    
    [ContextMenu("Make Desert")]
    void MakeDesert()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData data = terrain.terrainData;
        
        int res = data.heightmapResolution;
        float[,] heights = new float[res, res];
        
        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                float u = (float)x / res;
                float v = (float)z / res;
                
                // Основные дюны (частые)
                float mainDunes = Mathf.Sin(u * duneFrequency) * Mathf.Cos(v * duneFrequency);
                
                // Мелкие дюны для детализации
                float smallDunes = Mathf.Sin(u * duneFrequency * 2.5f) * 
                                   Mathf.Cos(v * duneFrequency * 2.2f) * 0.3f;
                
                // Рябь на песке
                float ripples = Mathf.Sin(u * 50) * Mathf.Cos(v * 50) * 0.05f;
                
                // Суммируем все слои
                float height = mainDunes * duneHeight + smallDunes * 0.15f + ripples + baseHeight;
                
                // Нормализуем
                heights[z, x] = Mathf.Clamp01(height);
            }
        }
        
        data.SetHeights(0, 0, heights);
        
        Debug.Log($"Desert ready! Dune frequency: {duneFrequency}");
    }
}