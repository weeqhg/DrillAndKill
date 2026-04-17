using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Leveltree/LevelNodeData")]
public class LevelNodeData : ScriptableObject
{
    public string id;
    public SceneType sceneType;
    public LocalizedString nodeName;
    public LocalizedString description;
}