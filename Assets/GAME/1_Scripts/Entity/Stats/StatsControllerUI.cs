using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using TMPro;

public class StatsControllerUI : UIWindow
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform[] rectTransforms;
    [Header("Stat References")]
    [SerializeField] private StatDisplay[] statDisplays;
    private StatsController stats;
    private AutoPopup statsPopup;
    private bool isOpen = false;
    private bool isInit = false;


    [System.Serializable]
    private class StatDisplay
    {
        public StatType type;
        public LocalizedString label;
        public TextMeshProUGUI valueText;
        public string prefix = "";
        public string suffix = "";
        public string format = "F0"; // Формат вывода
        public float multiplier = 1f;
    }

    public void Initialize(StatsController stats)
    {
        statsPopup = GetComponent<AutoPopup>();

        if (statsPopup != null && !isInit)
        {
            statsPopup.Initialize();
            gameObject.SetActive(false);
            isInit = true;
        }

        this.stats = stats;

        UpdateStats();
    }

    private void Update()
    {
        if (isOpen && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[0], mousePos) && !RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[1], mousePos))
            {
                G.UIManager.CloseTop();
            }
        }
    }

    public void TogglePanel()
    {
        if (gameObject.activeSelf)
        {
            G.UIManager.CloseTop();
        }
        else
        {
            G.UIManager.OpenOverlay(this);
        }
    }
    public override void Show()
    {
        base.Show();
        isOpen = true;
        statsPopup.OpenPanel();
        UpdateStats();
    }

    public override void Hide()
    {
        base.Hide();
        isOpen = false;
        statsPopup.ClosePanel();
    }

    public void UpdateStats()
    {
        foreach (var display in statDisplays)
        {
            float value = stats.GetStat(display.type) * display.multiplier;
            string formattedValue = value.ToString(display.format);
            string localizedLabel = display.label.GetLocalizedString();
            string prefix = display.prefix;
            string suffix = display.suffix;
            display.valueText.text = $"<b>{localizedLabel}</b> {prefix}{formattedValue}{suffix}";
        }
    }

    private void OnDestroy()
    {
        if (stats != null) stats.OnStatsChanged -= UpdateStats;
    }
}