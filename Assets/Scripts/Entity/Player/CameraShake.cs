using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private CinemachineImpulseSource impulseSource;

    private void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float impulseStrength)
    {
        impulseSource.GenerateImpulse(impulseStrength);
    }
}