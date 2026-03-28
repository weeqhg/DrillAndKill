using UnityEngine;

public class PlayerFly : MonoBehaviour
{
    
    private float _flySpeed = 15f;
    private float _flyAccelRate = 10f;
    private Rigidbody _rb;
    private PlayerMovement _player;
    private float _verticalInput;

    public PlayerFly(PlayerMovement player)
    {
        _player = player;
        _rb = player.Rb;
    }
    public void HandleFlight()
    {
        // Ввод
        Vector3 targetVelocity = Vector3.zero;
        targetVelocity += _player.CameraTransform.right * _player.MoveInput.x;
        targetVelocity += _player.CameraTransform.forward * _player.MoveInput.y;
        targetVelocity += Vector3.up * _verticalInput;

        targetVelocity = targetVelocity.normalized * _flySpeed;

        // Плавное ускорение
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, _flyAccelRate * Time.fixedDeltaTime);
    }

    public void SetVerticalInput(bool isUpPressed, bool isDownPressed)
    {
        if (isUpPressed)
            _verticalInput = 1;
        else if (isDownPressed)
            _verticalInput = -1;
        else
            _verticalInput = 0;
    }
}
