using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    private int money;
    public int Money => money;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private RectTransform moneyUI;


    private EventSFX eventSFX;
    private int bigRewardThreshold = 50;

    public void Initialize()
    {
        eventSFX = GetComponentInChildren<EventSFX>();
        
        GameEvents.OnCommandCoin += AddCoin;
        GameEvents.OnDifficultyScalerCommand += OnDifficultyScalerCommandHandler;

        canvasGroup.alpha = 1f;

        ResetMoney();
    }

    private void OnDifficultyScalerCommandHandler(bool isEnabled)
    {
        if (isEnabled)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
            ResetMoney();
        }
    }

    public void AddCoin(int amount)
    {
        if (amount == 0) return;

        money = Math.Max(0, money + amount);

        if (amount > 0)
            eventSFX?.PlayLootPickup();

        PlayMoneyEffect(amount);
        UpdateMoneyText();
    }

    public void SpendCoin(int amount)
    {
        amount = Mathf.Abs(amount);
        if (money < amount) return;
        money -= amount;

        if (amount > 0)
            eventSFX?.PlayLootDroop();

        UpdateMoneyText();
    }

    private void PlayMoneyEffect(int amount)
    {
        // Останавливаем текущую анимацию
        moneyUI.DOKill();

        // Возвращаем в исходное состояние
        moneyUI.localScale = Vector3.one;
        moneyUI.localRotation = Quaternion.identity;

        // Анимация увеличения и возврата
        float strength = Mathf.Clamp(amount * 0.2f, 0.5f, 2f);
        moneyUI.DOPunchScale(Vector3.one * 0.1f, 0.3f, 1, 1f);

        if (amount >= bigRewardThreshold)
        {
            bigRewardThreshold = amount;
            moneyUI.DOPunchRotation(new Vector3(0, 0, 15f), 0.3f, 10, 1f);
        }
    }

    private void UpdateMoneyText()
    {
        moneyText.text = $"{money}";
    }

    private void ResetMoney()
    {
        money = 0;
        UpdateMoneyText();
    }


    private void OnDestroy()
    {
        GameEvents.OnCommandCoin -= AddCoin;

        GameEvents.OnDifficultyScalerCommand -= OnDifficultyScalerCommandHandler;
    }
}
