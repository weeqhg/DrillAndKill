using System;
using System.Collections.Generic;

[Serializable]
public class LevelTreeData
{
    public List<LevelNode> allNodes = new();
    public LevelNode currentNode;

    public LevelNode GetNode(string id) => allNodes.Find(n => n.data.id == id);

    public bool CanUnlock(LevelNode node)
    {
        if (node == null || node.isUnlocked) return false;
        if (currentNode == null) return false;

        return currentNode.connections.Contains(node.data.id);
    }

    public bool TryUnlock(LevelNode node)
    {
        if (!CanUnlock(node)) return false;

        node.isUnlocked = true;
        currentNode = node;
        return true;
    }

    public void Clear()
    {
        allNodes.Clear();
        currentNode = null;
    }
}