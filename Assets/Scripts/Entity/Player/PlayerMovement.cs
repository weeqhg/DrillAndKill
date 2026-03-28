using UnityEngine;
using WekenDev.InputSystem;

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

    // Компоненты
    private EntityStats _stats;
    private Rigidbody _rb;
    private InputManager _input;
    private Transform _cameraTransform;

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
    public EntityStats Stats => _stats;
    public Transform CameraTransform => _cameraTransform;
    public Transform GroundCheck => groundCheck;
    public LayerMask GroundLayer => groundLayer;
    public float GroundRayLength => groundRayLength;
    public float GroundCheckRadius => groundCheckRadius;
    private bool _isShooting = false;

    public void Initialize()
    {
        _stats = GetComponent<EntityStats>();
        _rb = GetComponent<Rigidbody>();
        _cameraTransform = Camera.main?.transform;
        _input = InputManager.Instance;

        _rb.freezeRotation = true;

        _movement = new MovementController(this);
        _jump = new JumpController(this, jumpRiseGravity, jumpFallGravity);
        _slide = new SlideController(this);
        _fly = new PlayerFly(this);
        _animation = new AnimationController(GetComponent<Animator>());

        _jump.Initialize();

        GameEvents.OnCommandPlayerFly += OnChangeFlyState;
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
        _movement.DecayBonusSpeed();

        _jump.ClearJumpQueued();
    }

    private void ReadInput()
    {
        if (_input == null) return;

        MoveInput = _input.Actions.Player.Move.ReadValue<Vector2>();

        if (_input.Actions.Player.Jump.WasPressedThisFrame())
            _jump.QueueJump();

        bool isUpPressed = _input.Actions.Player.Jump.IsPressed();
        bool isDownPressed = _input.Actions.Player.Slide.IsPressed();
        _fly.SetVerticalInput(isUpPressed, isDownPressed);

        _slide.SetSlideInput(_input.Actions.Player.Slide.IsPressed());

        _input.Actions.Player.Shoot.started += ctx => _isShooting = true;
        _input.Actions.Player.Shoot.canceled += ctx => _isShooting = false;
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
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, groundRayLength, groundLayer))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle > 5f ? hit.normal : Vector3.up;
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
        GameEvents.OnCommandPlayerFly -= OnChangeFlyState;
    }
}