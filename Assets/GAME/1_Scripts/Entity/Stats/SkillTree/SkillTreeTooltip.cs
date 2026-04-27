using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class SkillTreeTooltip : MonoBehaviour
{
    [Header("Tooltip")]
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TextMeshProUGUI tooltipText;

    public LocalizedString flatText;
    public LocalizedString increasedText;
    public LocalizedString moreText;

    private Vector2 tooltipOffset = new Vector2(100f, -150f);



    public void ShowTooltip(TalentNode node)
    {
        tooltip.SetActive(true);
        tooltipText.text = TooltipString(node);
    }

    public void HideTooltip()
    {
        tooltip.SetActive(false);
    }

    private void Update()
    {
        UpdateTooltipPosition();
    }

    private void UpdateTooltipPosition()
    {
        if (!tooltip.activeSelf) return;
        tooltip.transform.position =
        Mouse.current.position.ReadValue() + tooltipOffset;
    }

    private string TooltipString(TalentNode node)
    {
        if (node.data.itemEffect != null) return GetItemTooltip(node);

        return GetStatTooltip(node);
    }

    private string GetItemTooltip(TalentNode node)
    {
        string name = $"<u><b><size=120%><color=#FFFFFF><mark=#000000AA> {node.data.nodeName.GetLocalizedString()}_</mark></color></size></b></u>";
        string description = $"\n<color=#00FFFF>{node.data.description.GetLocalizedString()}</color>";
        return name + description;
    }

    private string GetStatTooltip(TalentNode node)
    {
        string name = node.data.nodeName.GetLocalizedString();
        string type = GetLocalizedModifier(node.data.modifierType);
        string value = FormatValue(node);
        return $"<u><b><size=120%><color=#FFFFFF><mark=#000000AA> {name}_</mark></color></size></b></u>" +
        $"\n<color=#00FFFF>{value} {type} {name}</color>";
    }

    private string GetLocalizedModifier(ModifierType type)
    {
        return type switch
        {
            ModifierType.Flat => flatText.GetLocalizedString(),
            ModifierType.Increased => increasedText.GetLocalizedString(),
            ModifierType.More => moreText.GetLocalizedString(),
            _ => type.ToString()
        };
    }

    private string FormatValue(TalentNode node)
    {
        float value = node.data.statValue;

        return node.data.modifierType switch
        {
            ModifierType.Increased => $"+{value}%",
            ModifierType.More => $"x{1f + value}",
            _ => $"+{value}"
        };
    }
}
