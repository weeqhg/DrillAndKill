using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class LaunchWorld : MonoBehaviour
{
    private CinemachineCamera cinemachineCamera;
    private PlayerManager player;
    private BoerController boerController;


    public void Initialize()
    {
        boerController = GetComponentInChildren<BoerController>();
        boerController.Initialize();

        cinemachineCamera = GetComponentInChildren<CinemachineCamera>();

        boerController.OnActiveNextLevel += NextLevel;
        boerController.OnBoerDeparture += BoerDeparture;

        if (PlayerService.Player != null) SetPlayer(PlayerService.Player);
        PlayerService.OnPlayerChanged += SetPlayer;
    }

    private void SetPlayer(PlayerManager player)
    {
        if (player == null) return;
        
        this.player = player;
        StartLevel();
    }

    public void StartLevel()
    {
        if (player == null) return;

        cinemachineCamera.enabled = true;
        cinemachineCamera.transform.position = player.Transform.position + new Vector3(0f, 50f, -20f);
        player?.HidePlayer();

        boerController.OnBoerArrived += BoerArrived;
        boerController.NextLevelLaunch();
    }

    public void CallBoer()
    {
        boerController.ForceLaunchBoer();
    }

    private void BoerArrived()
    {
        boerController.OnBoerArrived -= BoerArrived;

        Vector3 rawPos = boerController.transform.position + new Vector3(0f, 0f, 20f);

        Vector3 groundPos = GetGroundPosition(rawPos);

        player?.TeleportPlayer(groundPos);
        player?.ShowPlayer();


        cinemachineCamera.enabled = false;

        StartCoroutine(Return());
    }

    private Vector3 GetGroundPosition(Vector3 basePos)
    {
        Vector3 origin = basePos + Vector3.up * 25f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 50f))
        {
            return hit.point;
        }

        return basePos;
    }

    private IEnumerator Return()
    {
        yield return new WaitForSeconds(2f);
        boerController.Despawn();
    }

    private void NextLevel()
    {
        cinemachineCamera.enabled = true;
        cinemachineCamera.transform.position = player.Transform.position + new Vector3(0f, 50f, -20f);

        player.HidePlayer();
    }

    private void BoerDeparture()
    {
        G.GameFlow?.ShowLevelTree();
    }


    private void OnDestroy()
    {
        PlayerService.OnPlayerChanged -= SetPlayer;

        boerController.OnBoerArrived -= BoerArrived;
        boerController.OnActiveNextLevel -= NextLevel;
        boerController.OnBoerDeparture -= BoerDeparture;
    }
}
