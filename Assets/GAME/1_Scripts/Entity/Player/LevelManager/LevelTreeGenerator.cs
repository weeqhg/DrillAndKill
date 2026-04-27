using System.Collections.Generic;
using UnityEngine;

public class LevelTreeGenerator
{
    private readonly LevelTreeConfig _config;
    private int _nodeCounter;

    public LevelTreeGenerator(LevelTreeConfig config)
    {
        _config = config;
    }

    public LevelTreeData Generate()
    {
        _nodeCounter = 0;
        var data = new LevelTreeData();

        // Создаём стартовую ноду
        var startNode = CreateStartNode();
        data.allNodes.Add(startNode);
        data.currentNode = startNode;

        // Создаём первый уровень
        var previousNodes = CreateFirstLevel(data, startNode);

        // Создаём промежуточные уровни
        for (int level = 1; level < _config.levelCount; level++)
        {
            var currentNodes = CreateNextLevel(data, previousNodes, level);

            // Добавляем кросс-соединения (кроме последнего уровня)
            if (level < _config.levelCount - 1)
                AddCrossConnections(currentNodes);

            previousNodes = currentNodes;
        }

        // Создаём финальную ноду
        CreateFinalNode(data, previousNodes);

        return data;
    }

    private LevelNode CreateStartNode()
    {
        var node = new LevelNode
        {
            data = CloneNodeData(_config.startNodeData, "Start"),
            parents = new List<string>(),
            isUnlocked = true
        };

        return node;
    }

    private List<LevelNode> CreateFirstLevel(LevelTreeData data, LevelNode startNode)
    {
        var firstLevelNodes = new List<LevelNode>();

        for (int p = 0; p < _config.pathCount; p++)
        {
            var node = CreateRandomNode($"0_{p}");
            node.parents = new List<string> { startNode.data.id };
            startNode.connections.Add(node.data.id);

            data.allNodes.Add(node);
            firstLevelNodes.Add(node);
        }

        return firstLevelNodes;
    }

    private List<LevelNode> CreateNextLevel(LevelTreeData data, List<LevelNode> previousNodes, int levelIndex)
    {
        var currentNodes = new List<LevelNode>();

        foreach (var prevNode in previousNodes)
        {
            var newNode = CreateRandomNode($"{levelIndex}_{GetPathIndex(prevNode)}");
            newNode.parents = new List<string> { prevNode.data.id };
            prevNode.connections.Add(newNode.data.id);

            data.allNodes.Add(newNode);
            currentNodes.Add(newNode);
        }

        return currentNodes;
    }

    private void AddCrossConnections(List<LevelNode> nodes)
    {
        var usedInCross = new HashSet<LevelNode>();

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            var nodeA = nodes[i];
            var nodeB = nodes[i + 1];

            if (usedInCross.Contains(nodeA) || usedInCross.Contains(nodeB))
                continue;

            if (UnityEngine.Random.value < _config.crossConnectionChance)
            {
                if (!nodeA.connections.Contains(nodeB.data.id))
                {
                    // Добавляем двустороннюю связь
                    nodeA.connections.Add(nodeB.data.id);
                    nodeB.parents.Add(nodeA.data.id);

                    // Помечаем как использованные
                    usedInCross.Add(nodeA);
                    usedInCross.Add(nodeB);
                }
            }
        }
    }

    private void CreateFinalNode(LevelTreeData data, List<LevelNode> lastLevelNodes)
    {
        var finalNode = new LevelNode
        {
            data = CloneNodeData(_config.finalNodeData, "Final"),
            parents = new List<string>()
        };

        foreach (var lastNode in lastLevelNodes)
        {
            lastNode.connections.Add(finalNode.data.id);
            finalNode.parents.Add(lastNode.data.id);
        }

        data.allNodes.Add(finalNode);
    }

    private LevelNode CreateRandomNode(string pathId)
    {
        var type = GetRandomLevelType();
        var node = new LevelNode();

        switch (type)
        {
            case SceneType.Arena:
                node.data = CloneNodeData(_config.enemyNodeData, $"Arena_{pathId}_{_nodeCounter++}");
                break;
            case SceneType.Shop:
                node.data = CloneNodeData(_config.shopNodeData, $"Shop_{pathId}_{_nodeCounter++}");
                break;
            case SceneType.Secret:
                node.data = CloneNodeData(_config.secretNodeData, $"Secret_{pathId}_{_nodeCounter++}");
                break;
            default:
                node.data = CloneNodeData(_config.enemyNodeData, $"Default_{pathId}_{_nodeCounter++}");
                break;
        }

        return node;
    }

    private SceneType GetRandomLevelType()
    {
        float roll = UnityEngine.Random.value;

        if (roll < _config.arenaChance)
            return SceneType.Arena;

        if (roll < _config.arenaChance + _config.shopChance)
            return SceneType.Shop;

        return SceneType.Secret;
    }

    private string GetPathIndex(LevelNode node)
    {
        // Извлекаем индекс пути из ID ноды
        // Формат: "Arena_1_2_5" где 1 - уровень, 2 - путь, 5 - счётчик
        var parts = node.data.id.Split('_');
        if (parts.Length >= 3)
            return parts[2]; // возвращаем индекс пути

        return "0";
    }

    private LevelNodeData CloneNodeData(LevelNodeData original, string newId)
    {
        var clone = ScriptableObject.CreateInstance<LevelNodeData>();
        clone.id = newId;
        clone.nodeName = original.nodeName;
        clone.description = original.description;
        clone.sceneType = original.sceneType;
        return clone;
    }
}