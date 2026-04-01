using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public enum ModifierType { Flat, Increased, More }

[Serializable]
public class TalentNode
{
    public TalentNodeData data;
    public bool isUnlocked;
    public List<string> parents = new();
}

public class SkillTreeStats : MonoBehaviour
{

    [Header("Loading Settings")]
    [Tooltip("Путь в папке Resources (без расширения)")]
    public string talentNodesResourcesPath = "Talents/Jack_Duelist";
    public string keyTree = "JackSkillTreeUnlock";

    [Header("Tree Settings")]
    public Sprite iconCharacter;
    public List<TalentNode> allNodes = new();
    private TalentPointsCounter talentPoints;
    private int cost = 1;

    public event Action<TalentNode> OnNodeUnlocked;

    private Dictionary<StatType, List<StatModifier>> modifiers = new();
    public event Action OnStatBonusTree;
    public event Action OnResetTree;

    [Serializable]
    public struct StatModifier
    {
        public float value;
        public ModifierType type;
        public StatModifier(float value, ModifierType type)
        {
            this.value = value;
            this.type = type;
        }
    }

    public void Initialize()
    {
        talentPoints = GetComponentInChildren<TalentPointsCounter>();

        talentPoints.Initialize();
        LoadNodesFromResources();

        if (allNodes.Count > 0)
            allNodes[0].isUnlocked = true;

        LoadProgress();
        RebuildModifiers();

        GameEvents.OnCommandResetTree += ResetTreeProgress;
    }



    public void ResetTreeProgress()
    {
        int refundedPoints = 0;

        foreach (var node in allNodes)
        {
            if (node.isUnlocked && node != allNodes[0])
            {
                refundedPoints += cost; // стоимость каждой ноды (cost)
                node.isUnlocked = false;
            }
        }

        // Корневая нода оставляем открытой
        if (allNodes.Count > 0)
            allNodes[0].isUnlocked = true;

        // Возврат очков
        talentPoints.AddPoints(refundedPoints);

        // Сбрасываем модификаторы
        RebuildModifiers();

        // Сохраняем прогресс
        PlayerPrefs.DeleteKey(keyTree);
        PlayerPrefs.Save();

        OnResetTree?.Invoke();
        OnStatBonusTree?.Invoke();
    }

    private void LoadNodesFromResources()
    {
        allNodes.Clear();

        TalentNodeData[] loaded = Resources.LoadAll<TalentNodeData>(talentNodesResourcesPath);

        foreach (var data in loaded)
        {
            TalentNode node = new TalentNode
            {
                data = data,
                isUnlocked = false,
                parents = new List<string>()
            };

            allNodes.Add(node);
        }
    }

    // =========================
    // STATS
    // =========================

    public float Apply(StatType type, float baseValue)
    {
        if (!modifiers.TryGetValue(type, out var list))
            return baseValue;

        float flat = 0f;
        float increased = 0f;
        float more = 1f;

        foreach (var mod in list)
        {
            switch (mod.type)
            {
                case ModifierType.Flat: flat += mod.value; break;
                case ModifierType.Increased: increased += mod.value; break;
                case ModifierType.More: more *= (1 + mod.value); break;
            }
        }

        return (baseValue + flat) * (1 + increased) * more;
    }

    private void RebuildModifiers()
    {
        modifiers.Clear();

        foreach (var node in allNodes.Where(n => n.isUnlocked))
            ApplyBonus(node);
    }

    private void ApplyBonus(TalentNode node)
    {
        if (node.data.keystoneEffect != null)
            node.data.keystoneEffect?.Apply(this);
        else
            AddModifier(node.data.statType, node.data.statValue, node.data.modifierType);
    }
    public void AddModifier(StatType type, float value, ModifierType modType)
    {
        if (!modifiers.TryGetValue(type, out var list))
        {
            list = new List<StatModifier>();
            modifiers[type] = list;
        }

        list.Add(new StatModifier(value, modType));
    }

    // =========================
    // UNLOCK
    // =========================

    public bool UnlockNode(TalentNode node)
    {
        if (!CanUnlock(node) || talentPoints.Points < cost)
            return false;

        talentPoints.RemovePoints(cost);
        node.isUnlocked = true;

        ApplyBonus(node);

        OnNodeUnlocked?.Invoke(node);
        OnStatBonusTree?.Invoke();

        SaveProgress();
        return true;
    }

    public bool CanUnlock(TalentNode node)
    {
        if (node.isUnlocked)
            return false;

        if (node == allNodes[0])
            return true;

        if (node.parents == null || node.parents.Count == 0)
            return false;

        return node.parents
            .Select(GetNode)
            .Any(parent => parent != null && parent.isUnlocked);
    }

    public TalentNode GetNode(string id) => allNodes.Find(n => n.data.id == id);

    // =========================
    // SAVE / LOAD
    // =========================

    private void SaveProgress()
    {
        var unlockedIds = allNodes.Where(n => n.isUnlocked).Select(n => n.data.id).ToArray();
        PlayerPrefs.SetString(keyTree, string.Join(",", unlockedIds));
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        string saved = PlayerPrefs.GetString(keyTree, "");
        if (!string.IsNullOrEmpty(saved))
        {
            var ids = saved.Split(',');
            foreach (var node in allNodes)
                node.isUnlocked = ids.Contains(node.data.id);
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnCommandResetTree -= ResetTreeProgress;
    }
}