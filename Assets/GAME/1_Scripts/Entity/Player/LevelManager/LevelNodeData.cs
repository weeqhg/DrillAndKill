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