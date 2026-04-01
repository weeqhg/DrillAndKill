using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;

public class StatsControllerUI : UIWindow
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform[] rectTransforms;

    [Header("Localize Events")]
    [SerializeField] private LocalizeStringEvent healthLocalizeEvent;
    [SerializeField] private LocalizeStringEvent moveSpeedLocalizeEvent;
    [SerializeField] private LocalizeStringEvent attackDamageLocalizeEvent;
    [SerializeField] private LocalizeStringEvent attackSpeedLocalizeEvent;
    private StatsController stats;
    private AutoPopup statsPopup;
    private bool isOpen = false;
    private float maxHealth;
    private float moveSpeed;
    private float attackDamage;
    private float attackRate;
    private float critChance;

    public void Initialize(StatsController stats)
    {
        statsPopup = GetComponent<AutoPopup>();

        if (statsPopup != null)
        {
            statsPopup.Initialize();
            gameObject.SetActive(false);
        }



        this.stats = stats;

        stats.OnStatsChanged += UpdateStats;
        UpdateStats();
        UpdateUI();
    }

    private void Update()
    {
        if (isOpen && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[0], mousePos) && !RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[1], mousePos))
            {
                UIManager.Instance.CloseTop();
            }
        }
    }

    public void TogglePanel()
    {
        if (gameObject.activeSelf)
        {
            UIManager.Instance.CloseTop();
        }
        else
        {
            UIManager.Instance.OpenOverlay(this);
        }
    }
    public override void Show()
    {
        base.Show();
        isOpen = true;
        statsPopup.OpenPanel();
    }

    public override void Hide()
    {
        base.Hide();
        isOpen = false;
        statsPopup.ClosePanel();
    }
    private void UpdateStats()
    {
        maxHealth = Mathf.RoundToInt(stats.GetStat(StatType.MaxHealth));
        moveSpeed = Mathf.RoundToInt(stats.GetStat(StatType.MoveSpeed));
        attackDamage = Mathf.RoundToInt(stats.GetStat(StatType.Damage));
        attackRate = (float)Math.Round(1f / stats.GetStat(StatType.AttackRate), 1);
        critChance = (float)Math.Round(stats.GetStat(StatType.CritСhance) / 100);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (stats == null) return;

        // Передаем значения в локализацию
        healthLocalizeEvent.StringReference.Arguments = new object[] { maxHealth };
        moveSpeedLocalizeEvent.StringReference.Arguments = new object[] { moveSpeed };
        attackDamageLocalizeEvent.StringReference.Arguments = new object[] { attackDamage };
        attackSpeedLocalizeEvent.StringReference.Arguments = new object[] { attackRate };

        // Обновляем текст
        healthLocalizeEvent.RefreshString();
        moveSpeedLocalizeEvent.RefreshString();
        attackDamageLocalizeEvent.RefreshString();
        attackSpeedLocalizeEvent.RefreshString();
    }

    private void OnDestroy()
    {
        stats.OnStatsChanged -= UpdateStats;
    }
}