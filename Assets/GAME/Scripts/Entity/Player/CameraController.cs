using UnityEngine;
using Unity.Cinemachine;

public class CameraContorller : MonoBehaviour
{
    private float gain = 1f;
    private CinemachineCamera cinemachineCamera;
    private CinemachineInputAxisController axisController;

    public void Initialize()
    {
        axisController = GetComponent<CinemachineInputAxisController>();
        cinemachineCamera = GetComponent<CinemachineCamera>();
        gain = PlayerPrefs.GetFloat(PlayerPrefsKeys.Sensitivity);
        cinemachineCamera.enabled = true;
        ConsoleEvents.OnSensitivityChanged += OnSensitivityChanged;
        GamePause.OnPauseGame += OnCameraEnabled;

        OnCameraEnabled(GamePause.IsGamePaused);

        SetSensitivity(gain);
    }

    private void SetSensitivity(float gain)
    {

        if (axisController == null) return;

        foreach (var controller in axisController.Controllers)
        {
            // Горизонтальная ось
            if (controller.Name == "Look Orbit X")
            {
                controller.Input.Gain = gain;
            }
            // Вертикальная ось
            else if (controller.Name == "Look Orbit Y")
            {
                controller.Input.Gain = -gain;
            }
        }
    }

    private void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(PlayerPrefsKeys.Sensitivity, value);
        gain = value;
        SetSensitivity(gain);
    }

    private void OnCameraEnabled(bool isPaused)
    {
        if (axisController == null) return;

        foreach (var controller in axisController.Controllers)
        {
            if (controller.Name == "Look Orbit X" || controller.Name == "Look Orbit Y")
            {
                controller.Enabled = !isPaused;
            }
        }
    }

    private void OnDestroy()
    {
        ConsoleEvents.OnSensitivityChanged -= OnSensitivityChanged;
        GamePause.OnPauseGame -= OnCameraEnabled;
    }
}