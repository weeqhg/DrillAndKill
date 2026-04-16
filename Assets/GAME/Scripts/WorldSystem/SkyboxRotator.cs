using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [Header("Rotation Speed")]
    [SerializeField] private float rotationSpeed = 1f;
    
    private float _rotation = 0f;
    
    private void Update()
    {
        if (GamePause.IsGamePaused || GamePause.IsGameFrozen) return;
        
        // Вращаем skybox (бесконечный цикл)
        _rotation += rotationSpeed * Time.deltaTime;
        
        // Сбрасываем значение при достижении 360 градусов (опционально)
        if (_rotation >= 360f)
            _rotation -= 360f;
        
        // Применяем вращение к skybox
        RenderSettings.skybox.SetFloat("_Rotation", _rotation);
    }
    
    // Опционально: метод для изменения скорости вращения
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
    
    // Опционально: метод для сброса вращения
    public void ResetRotation()
    {
        _rotation = 0f;
        RenderSettings.skybox.SetFloat("_Rotation", 0f);
    }
}