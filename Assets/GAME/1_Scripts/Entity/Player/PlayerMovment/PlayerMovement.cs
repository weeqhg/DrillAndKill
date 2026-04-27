using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float jumpHeight = 8f;

    private const float SlideGroundMultiplier = 3f;
    private const float GroundNormalMultiplier = 5f;
    private const float JumpBufferTime = 0.15f;
    private float _lastJumpPressedTime;

    //Статы
    private float _moveSpeed;
    private int _maxJump;

    // Компоненты
    private StatsController _stats;
    private Rigidbody _rb;
    private Transform _cameraTransform;
    private InputManager _inputManager;

    //-------Cnotrollers--------
    private MovementController _movement;
    private JumpController _jump;
    private SlideController _slide;
    private AnimationController _animation;
    private PlayerFly _fly;

    // Общие данные
    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; } = false;
    public bool IsFlying { get; private set; } = false;
    public bool IsSliding => IsDownPressed && IsGrounded;
    public bool IsJump => CanUseJump();
    public bool IsDownPressed { get; private set; } = false;
    public bool IsUpPressed { get; private set; } = false;
    public bool IsShoot { get; private set; } = false;
    public float MoveSpeed => _moveSpeed;
    public float JumpHeight => jumpHeight;
    public int MaxJump => _maxJump;
    public Rigidbody Rb => _rb;
    public Transform CameraTransform => _cameraTransform;
    public MovementController Movement => _movement;
    public JumpController Jump => _jump;
    public SlideController Slide => _slide;
    public AnimationController Animation => _animation;
    public PlayerFly Fly => _fly;

    //-------State Machine--------
    public PlayerStateMachine StateMachine { get; private set; }
    public IdleState IdleState { get; private set; }
    public RunState RunState { get; private set; }
    public JumpState JumpState { get; private set; }
    public FallState FallState { get; private set; }
    public LandState LandState { get; private set; }
    public SlideState SlideState { get; private set; }
    public FlyState FlyState { get; private set; }



    #region Initialize
    public void Initialize(StatsController statsController)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _stats = statsController;

        SetupStateMachine();
        SetupContorllers();
        SetupInput();
        SetupEvents();
        UpdateStats();
    }

    private void SetupStateMachine()
    {
        StateMachine = new PlayerStateMachine();

        IdleState = new IdleState(this);
        RunState = new RunState(this);
        JumpState = new JumpState(this);
        LandState = new LandState(this);
        FallState = new FallState(this);
        SlideState = new SlideState(this);
        FlyState = new FlyState(this);

        SetState(IdleState);
    }

    private void SetupContorllers()
    {
        _cameraTransform = Camera.main?.transform;
        _movement = new MovementController(this);
        _jump = new JumpController(this, jumpRiseGravity, jumpFallGravity);
        _slide = new SlideController(this);
        _fly = new PlayerFly(this);
        _animation = new AnimationController(GetComponent<Animator>());
    }

    private void SetupInput()
    {
        _inputManager = G.InputManager;
        _inputManager.Actions.Player.Shoot.started += OnShootStarted;
        _inputManager.Actions.Player.Shoot.canceled += OnShootCanceled;
        _inputManager.Actions.Player.Jump.performed += OnJump;
    }

    private void SetupEvents()
    {
        ConsoleEvents.OnCommandPlayerFly += SetFlying;
        _stats.OnStatsChanged += UpdateStats;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    #endregion

    /// <summary>
    /// State Machine
    /// </summary>
    public void SetState(PlayerState state)
    {
        StateMachine.ChangeState(state);
    }

    #region Input Events
    private void ReadInput()
    {
        if (_inputManager == null) return;

        MoveInput = _inputManager.Actions.Player.Move.ReadValue<Vector2>();
        IsUpPressed = _inputManager.Actions.Player.Jump.IsPressed();
        IsDownPressed = _inputManager.Actions.Player.Slide.IsPressed();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _cameraTransform = Camera.main?.transform;
    }

    private void OnShootStarted(InputAction.CallbackContext ctx)
    {
        IsShoot = true;
    }

    private void OnShootCanceled(InputAction.CallbackContext ctx)
    {
        IsShoot = false;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        _lastJumpPressedTime = Time.time;
    }
    #endregion

    /// <summary>
    /// Core
    /// </summary>
    private void Update()
    {
        if (_rb == null) return;

        ReadInput();
        CheckGrounded();

        StateMachine.Update();
        _animation.UpdateAnimator(MoveInput, IsGrounded, IsFlying);
    }

    private void FixedUpdate()
    {
        if (GamePause.IsGamePaused)
        {
            StopPlayer();
            return;
        }

        if (StateMachine != null) StateMachine.FixedUpdate();
    }

    private void SetFlying(bool value)
    {
        IsFlying = value;
        _rb.useGravity = !value;
    }

    private void UpdateStats()
    {
        _moveSpeed = _stats.GetStat(StatType.MoveSpeed);
        _maxJump = (int)_stats.GetStat(StatType.MaxJump);
    }

    private void StopPlayer()
    {
        if (_rb == null) return;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        MoveInput = Vector2.zero;
        IsShoot = false;
    }

    private bool CanUseJump()
    {
        return Time.time - _lastJumpPressedTime <= JumpBufferTime;
    }

    private void CheckGrounded()
    {
        if (groundCheck == null) return;

        if (IsSliding)
        {
            IsGrounded = Physics.Raycast(
                groundCheck.position,
                Vector3.down,
                groundRayLength * SlideGroundMultiplier,
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

    public Vector3 GetGroundNormal()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, groundRayLength * GroundNormalMultiplier, groundLayer))
        {
            return hit.normal;
        }
        return Vector3.up;
    }

    public void AddBonusSpeed(float amount) => _movement.AddBonusSpeed(amount);

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_inputManager != null)
        {
            _inputManager.Actions.Player.Shoot.started -= OnShootStarted;
            _inputManager.Actions.Player.Shoot.canceled -= OnShootCanceled;
            _inputManager.Actions.Player.Jump.performed -= OnJump;
        }

        ConsoleEvents.OnCommandPlayerFly -= SetFlying;
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