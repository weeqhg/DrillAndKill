using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;



public class ItemTooltip : MonoBehaviour
{
    [Header("Tooltip")]
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TextMeshProUGUI tooltipText;



    public void ShowTooltip(ItemData item)
    {
        tooltip.SetActive(true);
        tooltipText.text = GetItemTooltip(item);
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
        Mouse.current.position.ReadValue() + new Vector2(200f, -100f);
    }
    private string GetItemTooltip(ItemData item)
    {
        string name = $"<u><b><size=120%>{item.itemName.GetLocalizedString()} </size></b></u>";

        string rawDescription = item.description.GetLocalizedString();

        // 👇 подставляем значение
        string descriptionWithValue = rawDescription.Replace(
            "{}",
            item.statModule.value.ToString()
        );
        
        string description = $"\n<color=#00FFFF>{descriptionWithValue}</color>";
        return name + description;
    }


}
