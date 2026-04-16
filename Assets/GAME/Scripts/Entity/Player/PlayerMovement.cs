using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundRayLength = 0.7f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Gravity")]
    [SerializeField] private float jumpRiseGravity = 1.5f;
    [SerializeField] private float jumpFallGravity = 2.5f;
    //Статы
    private float _moveSpeed;
    private int _maxJump;
    private float _jumpHeight;
    // Компоненты
    private StatsController _stats;
    private Rigidbody _rb;

    // Подсистемы
    private MovementController _movement;
    private JumpController _jump;
    private SlideController _slide;
    private AnimationController _animation;
    private PlayerFly _fly;

    // Общие данные
    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsJumping { get; set; }
    public bool IsFlying { get; private set; }

    public Rigidbody Rb => _rb;
    public Transform CameraTransform => Camera.main?.transform;
    public Transform GroundCheck => groundCheck;
    public LayerMask GroundLayer => groundLayer;
    public float GroundRayLength => groundRayLength;
    public float GroundCheckRadius => groundCheckRadius;
    private bool _isShooting = false;

    public float MoveSpeed => _moveSpeed;
    public int MaxJump => _maxJump;
    public float JumpHeight => _jumpHeight;

    public void Initialize(StatsController statsController)
    {
        _stats = statsController;
        _rb = GetComponent<Rigidbody>();

        _rb.freezeRotation = true;

        _movement = new MovementController(this);
        _jump = new JumpController(this, jumpRiseGravity, jumpFallGravity);
        _slide = new SlideController(this);
        _fly = new PlayerFly(this);
        _animation = new AnimationController(GetComponent<Animator>());

        ConsoleEvents.OnCommandPlayerFly += OnChangeFlyState;

        _stats.OnStatsChanged += UpdateStats;
        UpdateStats();

        var input = G.InputManager;
        input.Actions.Player.Shoot.started += OnShootStarted;
        input.Actions.Player.Shoot.canceled += OnShootCanceled;
        input.Actions.Player.Jump.performed += OnJump;
    }

    private void OnShootStarted(InputAction.CallbackContext ctx)
    {
        _isShooting = true;
    }

    private void OnShootCanceled(InputAction.CallbackContext ctx)
    {
        _isShooting = false;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        _jump.QueueJump();
    }

    private void UpdateStats()
    {
        _moveSpeed = _stats.GetStat(StatType.MoveSpeed);
        _maxJump = (int)_stats.GetStat(StatType.MaxJump);
        _jumpHeight = _stats.GetStat(StatType.JumpHeight);
    }

    private void Update()
    {
        if (_rb == null) return;

        ReadInput();
        CheckGrounded();
        _animation.UpdateAnimator(MoveInput, IsGrounded, IsSliding, IsFlying);
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;
        if (_rb.isKinematic == true) return;
        if (GamePause.IsGamePaused)
        {
            StopPlayer();
            return;
        }

        if (IsFlying)
        {
            _fly.HandleFlight();
            _movement.HandleRotation(MoveInput.sqrMagnitude > 0.01f || _isShooting);
            return;
        }

        _jump.Update(IsGrounded, _rb.linearVelocity.y);
        _slide.UpdateState(IsGrounded, _rb.linearVelocity);

        if (IsSliding)
            _slide.HandleMovement(MoveInput, IsGrounded);
        else
            _movement.HandleMovement(MoveInput, IsGrounded);
        _movement.HandleRotation(MoveInput.sqrMagnitude > 0.01f || _isShooting);

        _jump.HandleGravity(ref _rb);
        _jump.HandleAirSounds(IsGrounded);
        _movement.DecayBonusSpeed();

        _jump.ClearJumpQueued();
    }

    private void StopPlayer()
    {
        // Полностью остановить Rigidbody
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Обнуляем ввод
        MoveInput = Vector2.zero;
        _isShooting = false;

        // Можно остановить внутренние подсистемы, если нужно
        _slide?.SetSlideInput(false);
        _fly?.SetVerticalInput(false, false);
    }

    private void ReadInput()
    {
        var input = G.InputManager;
        if (input == null) return;

        MoveInput = input.Actions.Player.Move.ReadValue<Vector2>();

        bool isUpPressed = input.Actions.Player.Jump.IsPressed();
        bool isDownPressed = input.Actions.Player.Slide.IsPressed();

        _fly.SetVerticalInput(isUpPressed, isDownPressed);
        _slide.SetSlideInput(input.Actions.Player.Slide.IsPressed());
    }

    private void CheckGrounded()
    {
        if (groundCheck == null) return;

        if (IsSliding)
        {
            IsGrounded = Physics.Raycast(
                groundCheck.position,
                Vector3.down,
                groundRayLength * 3f,
                groundLayer
            );
        }
        else
        {
            IsGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
    }

    public void SetSliding(bool sliding)
    {
        IsSliding = sliding;
    }

    public Vector3 GetGroundNormal()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, groundRayLength * 5f, groundLayer))
        {
            return hit.normal;
        }
        return Vector3.up;
    }

    public void AddBonusSpeed(float amount) => _movement.AddBonusSpeed(amount);
    public void OnJumpPerformed() => _animation.TriggerJump();

    private void OnChangeFlyState(bool value)
    {
        IsFlying = value;
        _rb.useGravity = !value;
    }

    private void OnDestroy()
    {
        if (G.InputManager != null)
        {
            var input = G.InputManager;
            input.Actions.Player.Shoot.started -= OnShootStarted;
            input.Actions.Player.Shoot.canceled -= OnShootCanceled;
            input.Actions.Player.Jump.performed -= OnJump;
        }

        ConsoleEvents.OnCommandPlayerFly -= OnChangeFlyState;
        if (_stats != null) _stats.OnStatsChanged -= UpdateStats;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            // Рисуем сферу groundCheckRadius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            // Рисуем луч groundRayLength
            Gizmos.color = Color.red;
            Gizmos.DrawRay(groundCheck.position, Vector3.down * groundRayLength);
        }
    }
}