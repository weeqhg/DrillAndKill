using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using Unity.VisualScripting;

public class LaunchWorld : MonoBehaviour, IInitializable
{
    private readonly Vector3 CameraOffset = new(0f, 50f, -20f);
    private readonly Vector3 SpawnOffset = new(0f, 0f, 20f);
    private CinemachineCamera cinemachineCamera;
    private PlayerManager player;
    private PepelatsController pepelats;
    private bool isReturn = true;



    public void Initialize()
    {
        pepelats = GetComponentInChildren<PepelatsController>();
        pepelats.Initialize();

        cinemachineCamera = GetComponentInChildren<CinemachineCamera>();

        pepelats.OnActiveNextLevel += NextLevel;
        pepelats.OnPepelatsDeparture += PepelatsDeparture;

        if (PlayerService.Player != null) SetPlayer(PlayerService.Player);
        PlayerService.OnPlayerChanged += SetPlayer;
    }

    /////////////////////////////////
    /// Внешний доступ
    /////////////////////////////////
    public void LaunchPlayerInWorld(SceneType sceneType)
    {
        if (player == null) return;
        
        EnableCamera();

        player?.HidePlayer();

        pepelats.LaunchPepelats();

        pepelats.OnPepelatsArrived -= PepelatsArrived;
        pepelats.OnPepelatsArrived += PepelatsArrived;

        if (sceneType == SceneType.Arena || sceneType == SceneType.Secret || sceneType == SceneType.Final)
        {
            isReturn = true;
        }
        else if (sceneType == SceneType.Shop)
        {
            isReturn = false;
        }
    }

    public void CallPepelats()
    {
        pepelats.ForceLaunchPepelats();
    }

    /////////////////////////////////
    /// Core
    /////////////////////////////////
    private void SetPlayer(PlayerManager player)
    {
        this.player = player;
    }

    private void PepelatsArrived()
    {
        pepelats.OnPepelatsArrived -= PepelatsArrived;

        Vector3 rawPos = pepelats.transform.position + SpawnOffset;
        Vector3 groundPos = SystemGet.GetGroundPosition(rawPos, 30f);

        player?.TeleportPlayer(groundPos);
        player?.ShowPlayer();

        cinemachineCamera.enabled = false;

        StartCoroutine(Return());
    }

    private IEnumerator Return()
    {
        yield return new WaitForSeconds(2f);
        if (isReturn) pepelats.Despawn();

        yield return new WaitForSeconds(2f);
        pepelats.SetAvailable(true);
    }

    private void NextLevel()
    {
        EnableCamera();

        player?.HidePlayer();
    }

    private void PepelatsDeparture()
    {
        G.GameFlow?.ShowLevelTree();
    }

    private void EnableCamera()
    {
        if (player == null) return;

        cinemachineCamera.enabled = true;
        cinemachineCamera.transform.position = player.Transform.position + CameraOffset;
    }

    private void OnDestroy()
    {
        PlayerService.OnPlayerChanged -= SetPlayer;

        pepelats.OnPepelatsArrived -= PepelatsArrived;
        pepelats.OnActiveNextLevel -= NextLevel;
        pepelats.OnPepelatsDeparture -= PepelatsDeparture;
    }
}
