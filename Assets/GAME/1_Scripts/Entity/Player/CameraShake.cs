using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private bool IsStoped => GamePause.IsGamePaused;

    [SerializeField] private CinemachineImpulseSource lightShake;
    [SerializeField] private CinemachineImpulseSource heavyShake;

    private float currentStrength = 0;
    private float remainingDuration = 0;

    private Coroutine shakeCoroutine;


    public void ShakeLight(float impulseStrength)
    {
        lightShake.GenerateImpulse(impulseStrength);
    }

    public void ShakeHeavy(float impulseStrength, float duration = 0)
    {
        currentStrength = impulseStrength;
        remainingDuration = duration;

        // Запускаем тряску
        heavyShake.ImpulseDefinition.TimeEnvelope.SustainTime = duration;
        heavyShake.GenerateImpulse(impulseStrength);

        if (isActiveAndEnabled)
        {
            // Останавливаем старую корутину если есть
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            // Запускаем новую
            shakeCoroutine = StartCoroutine(ShakeWithPause());
        }
    }

    private IEnumerator ShakeWithPause()
    {
        float elapsed = 0f;
        bool wasPaused = false;

        while (elapsed < remainingDuration)
        {
            if (IsStoped)
            {
                wasPaused = true;
                yield return null;
                continue;
            }

            if (wasPaused)
            {
                wasPaused = false;
                float remaining = remainingDuration - elapsed;
                heavyShake.ImpulseDefinition.TimeEnvelope.SustainTime = remaining;
                heavyShake.GenerateImpulse(currentStrength);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeCoroutine = null;
    }
}