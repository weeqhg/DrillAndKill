using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;

    private int count;

    public void Initialize(ItemData item)
    {
        icon.sprite = item.icon;
    }

    public void SetCount(int value)
    {
        count = value;
        countText.text = count > 1 ? count.ToString() : "";
    }

}