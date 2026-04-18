using UnityEngine;
using System.Collections.Generic;

public class LevelTree : MonoBehaviour, IInitializable
{
    [SerializeField] private LevelTreeUI levelTreeUI;
    [SerializeField] private LevelTreeConfig config = new LevelTreeConfig();

    private LevelTreeData data;
    private LevelTreeGenerator generator;

    public List<LevelNode> AllNode => data?.allNodes;
    public LevelNode GetNode(string id) => data?.GetNode(id);
    public bool CanUnlock(LevelNode node) => data?.CanUnlock(node) ?? false;



    public void Initialize()
    {
        generator = new LevelTreeGenerator(config);
        data = generator.Generate();

        levelTreeUI.Initialize(this);
    }

    public bool UnlockNode(LevelNode node)
    {
        if (!data.TryUnlock(node))
            return false;

        G.UIManager?.CloseAll();
        G.GameFlow?.NextHandler(node.data.sceneType);
        return true;
    }

    public void ShowTree() => G.UIManager?.OpenOverlay(levelTreeUI);
    public void ResetProgress() => Initialize();

    private void OnValidate()
    {
        config?.Validate();
    }
}
