using UnityEngine;
using UnityEngine.Localization.Components;

public class EntityStatsUI : MonoBehaviour
{
    [Header("Localize Events")]
    [SerializeField] private LocalizeStringEvent healthLocalizeEvent;
    [SerializeField] private LocalizeStringEvent moveSpeedLocalizeEvent;
    [SerializeField] private LocalizeStringEvent attackDamageLocalizeEvent;
    [SerializeField] private LocalizeStringEvent attackSpeedLocalizeEvent;
    private EntityStats entityStats;

    public void UpdateComponent(PlayerManager playerManager)
    {
        EntityStats entityStats = playerManager.gameObject.GetComponent<EntityStats>();
        this.entityStats = entityStats;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (entityStats == null) return;

        // Передаем значения в локализацию
        healthLocalizeEvent.StringReference.Arguments = new object[] { entityStats.MaxHealth };
        moveSpeedLocalizeEvent.StringReference.Arguments = new object[] { entityStats.MoveSpeed };
        attackDamageLocalizeEvent.StringReference.Arguments = new object[] { entityStats.AttackDamage };
        attackSpeedLocalizeEvent.StringReference.Arguments = new object[] { entityStats.AttackSpeed };

        // Обновляем текст
        healthLocalizeEvent.RefreshString();
        moveSpeedLocalizeEvent.RefreshString();
        attackDamageLocalizeEvent.RefreshString();
        attackSpeedLocalizeEvent.RefreshString();
    }
}