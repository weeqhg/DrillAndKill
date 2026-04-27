using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkillTreeGraph", menuName = "SkillTree/Graph")]
public class SkillTreeGraph : ScriptableObject
{
    public string treeName;
    public Sprite icon;
    public string newNodeFolder = "Assets/Resources/Talents";
    public Vector2 defaultNodeSize = new Vector2(140f, 90f);
    public float gridSnapSize = 25f;
    public List<TalentNodeData> nodes = new List<TalentNodeData>();
}
