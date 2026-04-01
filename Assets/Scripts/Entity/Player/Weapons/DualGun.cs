using UnityEngine;

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
    private StatsController stats;

    private float _nextFireTime;
    private bool _isLeftTurn = true;
    private InputManager _inputManager;
    private bool _isShooting;
    private float attackRate;
    private float damage;
    private float chancheCrit;
    private float critMultiplayer;
    public void Initialize(CameraShake cameraShake, StatsController statsController)
    {
        stats = statsController;
        stats.OnStatsChanged += UpdateStats;
        UpdateStats();

        vfx = GetComponentInChildren<WeaponVFX>();
        vfx.Initialize();
        sfx = GetComponentInChildren<PlayerRandomSFX>();
        aimAnimation = GetComponentInChildren<AimAnimation>();
        this.cameraShake = cameraShake;
        
        _mainCamera = Camera.main;
        _inputManager = InputManager.Instance;

        _inputManager.Actions.Player.Shoot.started += ctx => _isShooting = true;
        _inputManager.Actions.Player.Shoot.canceled += ctx => _isShooting = false;
    }

    private void UpdateStats()
    {
        attackRate = stats.GetStat(StatType.AttackRate);
        damage = stats.GetStat(StatType.Damage);
        chancheCrit = stats.GetStat(StatType.CritСhance) / 100f;
        critMultiplayer = stats.GetStat(StatType.CritMultiplayer);
    }

    private void Update()
    {
        if (_isShooting && Time.time >= _nextFireTime)
        {
            Shoot();
            float cooldown = 1f / attackRate;
            _nextFireTime = Time.time + cooldown;
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
            float finalDamage = CalculateHitDamage();
            damageable?.TakeDamage(finalDamage);
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
    private float CalculateHitDamage()
    {
        bool isCrit = Random.value < chancheCrit;

        float finalDamage = damage;

        if (isCrit)
            finalDamage *= critMultiplayer;

        return finalDamage;
    }

    private void OnDestroy()
    {
        if (stats != null) stats.OnStatsChanged -= UpdateStats;
    }
}