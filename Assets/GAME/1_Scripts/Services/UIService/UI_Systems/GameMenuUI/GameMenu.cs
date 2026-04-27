using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenu : MonoBehaviour
{
    private GameMenuUI gameMenuUI;
    private StatsControllerUI statsUI;
    private ItemInformationUI itemUI;



    public void Initialize()
    {
        G.InputManager.Actions.UI.ESC.performed += OnCancel;

        statsUI = GetComponentInChildren<StatsControllerUI>(true);
        itemUI = GetComponentInChildren<ItemInformationUI>(true);
        gameMenuUI = GetComponentInChildren<GameMenuUI>(true);
        gameMenuUI.Initialize();

        if (PlayerService.Player != null) SetPlayer(PlayerService.Player);
        PlayerService.OnPlayerChanged += SetPlayer;
    }

    private void SetPlayer(PlayerManager player)
    {
        if (PlayerService.Player != null) statsUI.Initialize(player.StatsController);
        if (PlayerService.Player != null) itemUI.Initialize(player.ItemsUI);
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (G.UIManager.HasAnyWindow())
        {
            G.UIManager.CloseTop();
            return;
        }

        if (G.UIManager.IsOpen<GameMenuUI>())
        {
            G.UIManager.Close(gameMenuUI);
        }
        else
        {
            statsUI?.UpdateStats();
            G.UIManager.Open(gameMenuUI);
        }
    }


    private void OnDestroy()
    {
        PlayerService.OnPlayerChanged -= SetPlayer;
        if (G.InputManager != null) G.InputManager.Actions.UI.ESC.performed -= OnCancel;
    }
}
