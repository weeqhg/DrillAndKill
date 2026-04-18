using System;
using UnityEngine;

[Serializable]
public class LevelTreeConfig
{
    [Header("Tree Structure")]
    public int levelCount = 7;
    public int pathCount = 4;
    [Range(0f, 1f)] public float crossConnectionChance = 0.3f;

    [Header("Spawn Chances")]
    [Range(0f, 1f)] public float arenaChance = 0.6f;
    [Range(0f, 1f)] public float shopChance = 0.25f;
    [Range(0f, 1f)] public float secretChance = 0.15f;

    [Header("Node Data")]
    public LevelNodeData startNodeData;
    public LevelNodeData enemyNodeData;
    public LevelNodeData shopNodeData;
    public LevelNodeData secretNodeData;
    public LevelNodeData finalNodeData;

    public void Validate()
    {
        float total = arenaChance + shopChance + secretChance;
        if (Mathf.Abs(total - 1f) > 0.01f) Debug.LogWarning($"Total spawn chance = {total}. Should be 1.0");
    }
}