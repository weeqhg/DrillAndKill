using UnityEngine;
using WekenDev.InputSystem;

public class DualGun : MonoBehaviour
{
    [Header("Shoot Points")]
    [SerializeField] private Transform leftShootPoint;
    [SerializeField] private Transform rightShootPoint;
    [SerializeField] private LayerMask shootableLayers = ~0;

    [Header("Settings")]
    private float range = 100f;
    private float rayStartOffset = 5f;

    private WeaponVFX vfx;
    private PlayerRandomSFX sfx;
    private Camera _mainCamera;
    private CameraShake cameraShake;
    private AimAnimation aimAnimation;
    private EntityStats stats;

    private float _nextFireTime;
    private bool _isLeftTurn = true;
    private InputManager _inputManager;
    private bool _isShooting;

    public void Initialize(CameraShake cameraShake)
    {
        stats = GetComponentInParent<EntityStats>();
        vfx = GetComponentInChildren<WeaponVFX>();
        vfx.Initialize();
        sfx = GetComponentInChildren<PlayerRandomSFX>();
        aimAnimation = GetComponentInChildren<AimAnimation>();
        this.cameraShake = cameraShake;
        sfx.Initialize();
        _mainCamera = Camera.main;
        _inputManager = InputManager.Instance;

        _inputManager.Actions.Player.Shoot.started += ctx => _isShooting = true;
        _inputManager.Actions.Player.Shoot.canceled += ctx => _isShooting = false;
    }

    private void Update()
    {
        if (_isShooting && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + stats.AttackSpeed;
        }
    }

    private void Shoot()
    {
        Transform currentShootPoint = _isLeftTurn ? leftShootPoint : rightShootPoint;

        FireFromPoint(currentShootPoint);

        if (_isLeftTurn) vfx.PlayMuzzleFlash(0);
        else vfx.PlayMuzzleFlash(1);

        sfx.PlayRandomSound();
        cameraShake.Shake(1f);
        aimAnimation.PlayScaleAnimation();

        _isLeftTurn = !_isLeftTurn;
    }

    private void FireFromPoint(Transform shootPoint)
    {
        if (shootPoint == null || _mainCamera == null) return;

        // Луч начинается не из камеры, а на расстоянии offset от неё
        Vector3 rayOrigin = _mainCamera.transform.position + _mainCamera.transform.forward * rayStartOffset;
        Vector3 rayDirection = _mainCamera.transform.forward;

        Ray ray = new Ray(rayOrigin, rayDirection);

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, range, shootableLayers))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(range);
        }

        Vector3 direction = (targetPoint - shootPoint.position).normalized;

        if (Physics.Raycast(shootPoint.position, direction, out RaycastHit bulletHit, range, shootableLayers))
        {
            // Визуальные эффекты
            if (vfx != null)
            {
                vfx.PlayTracer(shootPoint.position, bulletHit.point);
                vfx.PlayImpact(bulletHit.point, bulletHit.normal);
            }

            // Нанесение урона
            var damageable = bulletHit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(stats.AttackDamage);
        }
        else
        {
            // Промах
            if (vfx != null)
            {
                vfx.PlayTracer(shootPoint.position, shootPoint.position + direction * range);
            }
        }
    }

    // Для отладки
    private void OnDrawGizmos()
    {
        // Отрисовка позиции начала луча для прицела
        if (_mainCamera != null)
        {
            Vector3 rayOrigin = _mainCamera.transform.position + _mainCamera.transform.forward * rayStartOffset;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rayOrigin, 0.1f);

            // Линия от камеры до начала луча
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_mainCamera.transform.position, rayOrigin);

            // Направление луча
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayOrigin, _mainCamera.transform.forward * range);
        }
    }
}