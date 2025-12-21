using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 2f;
    public float jumpHeightMultiplier = 1f;
    public float gravity = -9.81f;

    [Header("Original Values (no modificar)")]
    public float originalWalkSpeed = 3f;
    public float originalRunSpeed = 6f;
    public float originalSprintSpeed = 9f;
    public float originalJumpHeight = 2f;
    public float originalJumpHeightMultiplier = 1f;

    [Header("References")]
    public CharacterController characterController;
    public CameraController cameraController;
    public Transform playerCamera;
    public StaminaSystem staminaSystem;
    public PlayerAnimationController animationController;

    [HideInInspector] public PlayerState currentState;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool jumpRequested;

    private bool runPressed;
    private bool sprintPressed;

    private bool runBlocked;
    private bool sprintBlocked;

    private float lastRunTime;
    private float lastSprintTime;

    private void Awake()
    {
        if (animationController == null)
            animationController = GetComponentInChildren<PlayerAnimationController>();

        characterController = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        if (cameraController == null)
            cameraController = GetComponentInChildren<CameraController>();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed += _ => jumpRequested = true;

        inputActions.Player.Run.performed += _ =>
        {
            runPressed = true;
            lastRunTime = Time.time;
        };
        inputActions.Player.Run.canceled += _ =>
        {
            runPressed = false;
            runBlocked = false;
        };

        inputActions.Player.Sprint.performed += _ =>
        {
            sprintPressed = true;
            lastSprintTime = Time.time;
        };
        inputActions.Player.Sprint.canceled += _ =>
        {
            sprintPressed = false;
            sprintBlocked = false;
        };

        if (!staminaSystem)
            staminaSystem = GetComponent<StaminaSystem>();

        SaveOriginalValues();
    }

    private void SaveOriginalValues()
    {
        originalWalkSpeed = walkSpeed;
        originalRunSpeed = runSpeed;
        originalSprintSpeed = sprintSpeed;
        originalJumpHeight = jumpHeight;
        originalJumpHeightMultiplier = jumpHeightMultiplier;
    }

    public void ResetToOriginalValues()
    {
        walkSpeed = originalWalkSpeed;
        runSpeed = originalRunSpeed;
        sprintSpeed = originalSprintSpeed;
        jumpHeight = originalJumpHeight;
        jumpHeightMultiplier = originalJumpHeightMultiplier;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        currentState = new IdleState(this);
        currentState.Enter();
    }

    private void Update()
    {
        currentState.HandleMovement(moveInput, ref velocity, jumpRequested);

        PlayerState nextState = currentState.UpdateState();
        if (nextState != currentState)
        {
            currentState.Exit();
            currentState = nextState;
            currentState.Enter();
        }

        jumpRequested = false;

        if (animationController != null)
        {
            animationController.UpdateAnimations(
                moveInput,
                GetCurrentSpeed(),
                IsGrounded(),
                velocity.y
            );
        }
    }

    public void ChangeState(PlayerState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void Move(Vector3 motion)
    {
        characterController.Move(motion * Time.deltaTime);
    }

    public void ApplyGravity(ref Vector3 vel)
    {
        vel.y += gravity * Time.deltaTime;

        if (characterController.isGrounded && vel.y < 0f)
        {
            vel.y = -2f;
        }
    }

    public Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
            return Vector3.zero;

        Transform cameraTransform = cameraController?.CameraPivot ?? playerCamera;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;

        if (direction.sqrMagnitude > 0.01f)
            direction.Normalize();

        return direction;
    }

    public bool IsGrounded() => characterController.isGrounded;
    public float GetCurrentSpeed() => currentState.GetSpeed();
    public bool IsRunPressed() => runPressed;
    public bool IsSprintPressed() => sprintPressed;
    public Vector2 GetMoveInput() => moveInput;

    public void SetRunBlocked(bool blocked) => runBlocked = blocked;
    public bool IsRunBlocked() => runBlocked;

    public void SetSprintBlocked(bool blocked) => sprintBlocked = blocked;
    public bool IsSprintBlocked() => sprintBlocked;

    public float GetLastRunTime() => lastRunTime;
    public float GetLastSprintTime() => lastSprintTime;
}