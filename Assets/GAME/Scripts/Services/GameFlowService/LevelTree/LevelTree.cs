using UnityEngine;
using System;
using System.Collections.Generic;


[Serializable]
public class LevelNode
{
    public LevelNodeData data;
    public bool isUnlocked;
    public List<string> parents = new();
    public List<string> connections = new();
}

public class LevelTree : MonoBehaviour
{
    [Header("Tree Generation")]
    [SerializeField] private int levelCount = 7; // Количество уровней
    [SerializeField] private int pathCount = 4; // Количество путей (дорог)
    [SerializeField] private float crossConnectionChance = 0.3f; // Шанс соединения между дорогами

    [SerializeField] private LevelNodeData startNodeData;
    [SerializeField] private LevelNodeData enemyNodeData;
    [SerializeField] private LevelNodeData shopNodeData;
    [SerializeField] private LevelNodeData secretNodeData;
    [SerializeField] private LevelNodeData finalNodeData;

    public List<LevelNode> allNodes = new();
    public LevelNode GetNode(string id) => allNodes.Find(n => n.data.id == id);
    [SerializeField] private LevelTreeUI levelTreeUI;
    private int nodeCounter = 0;
    public LevelNode currentNode;

    public void Initialize()
    {
        allNodes.Clear();
        allNodes = GenerateLevelNodes();
        levelTreeUI.Initialize(this);

        G.GameFlow.OnResetProgress += ResetProgress;
    }

    public void ShowTree()
    {
        G.UIManager.OpenOverlay(levelTreeUI);
    }

    private void ResetProgress()
    {
        allNodes = GenerateLevelNodes();
        levelTreeUI.ResetTreeLevel();
    }

    public bool UnlockNode(LevelNode node)
    {
        if (!CanUnlock(node))
            return false;

        node.isUnlocked = true;

        currentNode = node;

        G.UIManager.CloseAll();
        G.GameFlow.NextHandler(currentNode.data.sceneType); //Запускаем след. уровень

        return true;
    }

    public bool CanUnlock(LevelNode node)
    {
        if (node.isUnlocked)
            return false;

        if (currentNode == null)
            return false;

        // 🔥 ТОЛЬКО прямое соединение
        return currentNode.connections.Contains(node.data.id);
    }

    private List<LevelNode> GenerateLevelNodes()
    {
        List<LevelNode> nodes = new List<LevelNode>();
        nodeCounter = 0;

        // Стартовая нода
        LevelNode startNode = new LevelNode();
        startNode.data = CloneNodeData(startNodeData, "Start");
        startNode.parents = new List<string>();
        startNode.isUnlocked = true;
        currentNode = startNode;

        nodes.Add(startNode);

        List<LevelNode> previousNodes = new List<LevelNode>();

        for (int p = 0; p < pathCount; p++)
        {
            SceneType type = GetRandomLevelType();
            LevelNode firstNode = new LevelNode();

            switch (type)
            {
                case SceneType.Arena:
                    firstNode.data = CloneNodeData(enemyNodeData, $"Arena_0_{p}_{nodeCounter++}");
                    break;
                case SceneType.Shop:
                    firstNode.data = CloneNodeData(shopNodeData, $"Shop_0_{p}_{nodeCounter++}");
                    break;
                case SceneType.Secret:
                    firstNode.data = CloneNodeData(secretNodeData, $"Secret_0_{p}_{nodeCounter++}");
                    break;
            }

            firstNode.parents = new List<string>() { startNode.data.id };
            startNode.connections.Add(firstNode.data.id);

            nodes.Add(firstNode);
            previousNodes.Add(firstNode);
        }

        // Генерируем остальные уровни
        for (int level = 1; level < levelCount; level++)
        {
            List<LevelNode> currentNodes = new List<LevelNode>();

            foreach (var prevNode in previousNodes)
            {
                SceneType type = GetRandomLevelType();
                LevelNode newNode = new LevelNode();

                switch (type)
                {
                    case SceneType.Arena:
                        newNode.data = CloneNodeData(enemyNodeData, $"Arena_{level}_{nodeCounter++}");
                        break;
                    case SceneType.Shop:
                        newNode.data = CloneNodeData(shopNodeData, $"Shop_{level}_{nodeCounter++}");
                        break;
                    case SceneType.Secret:
                        newNode.data = CloneNodeData(secretNodeData, $"Secret_{level}_{nodeCounter++}");
                        break;
                }

                newNode.parents = new List<string>() { prevNode.data.id };
                prevNode.connections.Add(newNode.data.id);

                nodes.Add(newNode);
                currentNodes.Add(newNode);
            }

            if (level == levelCount - 1)
            {
                previousNodes = currentNodes;
                continue;
            }

            HashSet<LevelNode> usedInCross = new HashSet<LevelNode>();

            for (int i = 0; i < currentNodes.Count - 1; i++)
            {
                var nodeA = currentNodes[i];
                var nodeB = currentNodes[i + 1];

                if (usedInCross.Contains(nodeA) || usedInCross.Contains(nodeB))
                    continue;

                if (UnityEngine.Random.value < crossConnectionChance)
                {
                    if (!nodeA.connections.Contains(nodeB.data.id))
                    {
                        nodeB.connections.Add(nodeA.data.id);
                        nodeA.parents.Add(nodeB.data.id);

                        // помечаем обе ноды как использованные
                        usedInCross.Add(nodeA);
                        usedInCross.Add(nodeB);
                    }
                }
            }

            previousNodes = currentNodes;
        }

        // Финальная нода
        LevelNode finalNode = new LevelNode();
        finalNode.data = CloneNodeData(finalNodeData, "Final");
        finalNode.parents = new List<string>();

        foreach (var lastNode in previousNodes)
        {
            lastNode.connections.Add(finalNode.data.id);
            finalNode.parents.Add(lastNode.data.id);
        }

        nodes.Add(finalNode);

        return nodes;
    }


    private SceneType GetRandomLevelType()
    {
        float roll = UnityEngine.Random.value;

        if (roll < 0.6f) return SceneType.Arena;
        if (roll < 0.85f) return SceneType.Shop;
        return SceneType.Secret;
    }

    private LevelNodeData CloneNodeData(LevelNodeData original, string newId)
    {
        LevelNodeData clone = ScriptableObject.CreateInstance<LevelNodeData>();
        clone.id = newId;
        clone.nodeName = original.nodeName;
        clone.description = original.description;
        clone.sceneType = original.sceneType;
        return clone;
    }

    private void OnDestroy()
    {
        G.GameFlow.OnResetProgress -= ResetProgress;
    }
}
