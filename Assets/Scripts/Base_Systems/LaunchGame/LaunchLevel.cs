using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class LaunchLevel : MonoBehaviour
{
    private CinemachineCamera cinemachineCamera;
    private GameObject player;
    private BoerController boerController;


    public void Initialize()
    {
        boerController = GetComponentInChildren<BoerController>();
        boerController.Initialize();

        cinemachineCamera = GetComponentInChildren<CinemachineCamera>();

        boerController.OnActiveNextLevel += NextLevel;
        boerController.OnBoerDeparture += BoerDeparture;


        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            OnPlayerReady(GameManager.Instance.Player);
        }
        GameManager.Instance.OnPlayerSpawned += OnPlayerReady;

        if (player != null) StartLevel();
        else GameEvents.ConsoleMessage("Player not spawn");
    }

    private void OnPlayerReady(GameObject player)
    {
        this.player = player;
    }

    public void StartLevel()
    {
        if (player == null) return;

        cinemachineCamera.enabled = true;
        cinemachineCamera.transform.position = player.transform.position + new Vector3(0f, 50f, -20f);
        player.SetActive(false);

        boerController.OnBoerArrived += BoerArrived;
        boerController.NextLevelLaunch();
    }

    private void BoerArrived()
    {
        boerController.OnBoerArrived -= BoerArrived;

        Vector3 rawPos = boerController.transform.position + new Vector3(0f, 0f, 20f);

        Vector3 groundPos = GetGroundPosition(rawPos);

        player.transform.position = groundPos;
        player.gameObject.SetActive(true);


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
        cinemachineCamera.transform.position = player.transform.position + new Vector3(0f, 50f, -20f);

        player.gameObject.SetActive(false);
    }

    private void BoerDeparture()
    {
        GameEvents.TriggerLevelTree();
    }


    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerSpawned -= OnPlayerReady;

        boerController.OnBoerArrived -= BoerArrived;
        boerController.OnActiveNextLevel -= NextLevel;
        boerController.OnBoerDeparture -= BoerDeparture;
    }
}
