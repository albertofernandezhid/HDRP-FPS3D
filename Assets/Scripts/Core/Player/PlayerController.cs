using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 2f;
    public float jumpHeightMultiplier = 1f;
    public float gravity = -9.81f;
    public float lookSensitivity = 1f;
    public float zoomSensitivity = 0.1f;

    [Header("Original Values")]
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
    public WeaponManager weaponManager;

    public PlayerState currentState;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float zoomInput;
    private Vector3 velocity;
    private bool jumpRequested;
    private bool runPressed;
    private bool sprintPressed;
    private bool toggleCameraPressed;
    private bool pausePressed;
    private bool runBlocked;
    private bool sprintBlocked;
    private float lastRunTime;
    private float lastSprintTime;

    private void Awake()
    {
        InitializeComponents();
        InitializeInputSystem();
        SaveOriginalValues();
    }

    private void InitializeComponents()
    {
        if (animationController == null) animationController = GetComponentInChildren<PlayerAnimationController>();
        characterController = GetComponent<CharacterController>();
        if (cameraController == null) cameraController = GetComponentInChildren<CameraController>();
        if (staminaSystem == null) staminaSystem = GetComponent<StaminaSystem>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();
    }

    private void InitializeInputSystem()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(this);
    }

    private void SaveOriginalValues()
    {
        originalWalkSpeed = walkSpeed;
        originalRunSpeed = runSpeed;
        originalSprintSpeed = sprintSpeed;
        originalJumpHeight = jumpHeight;
        originalJumpHeightMultiplier = jumpHeightMultiplier;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
    private void OnDestroy() => inputActions.Dispose();

    private void Start()
    {
        currentState = new IdleState(this);
        currentState.Enter();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        ProcessCameraToggle();
        ProcessPauseInput();
        currentState.HandleMovement(moveInput, ref velocity, jumpRequested);
        PlayerState nextState = currentState.UpdateState();
        if (nextState != currentState)
        {
            currentState.Exit();
            currentState = nextState;
            currentState.Enter();
        }
        ResetFrameInputs();
        if (animationController != null)
        {
            animationController.UpdateAnimations(moveInput, GetCurrentSpeed(), IsGrounded(), velocity.y);
        }
    }

    private void LateUpdate() => ProcessCameraInput();

    private void ProcessCameraInput()
    {
        if (cameraController == null) return;
        if (lookInput.sqrMagnitude > 0.01f)
        {
            bool isMouse = (inputActions.Player.Look.activeControl?.device is Mouse);
            float sensitivity = isMouse ? cameraController.MouseSens : cameraController.GamepadSens;
            cameraController.HandleMouseLook(lookInput.x * sensitivity, lookInput.y * sensitivity, isMouse);
        }
    }

    private void ProcessCameraToggle()
    {
        if (toggleCameraPressed && cameraController != null)
        {
            cameraController.ToggleCameraMode();
            toggleCameraPressed = false;
        }
    }

    private void ProcessPauseInput()
    {
        if (pausePressed)
        {
            bool isPaused = Time.timeScale == 0f;
            Time.timeScale = isPaused ? 1f : 0f;
            Cursor.lockState = isPaused ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isPaused;
            pausePressed = false;
        }
    }

    private void ResetFrameInputs() => jumpRequested = false;

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context) { if (context.performed) jumpRequested = true; }
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed) { runPressed = true; lastRunTime = Time.time; }
        else if (context.canceled) { runPressed = false; runBlocked = false; }
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed) { sprintPressed = true; lastSprintTime = Time.time; }
        else if (context.canceled) { sprintPressed = false; sprintBlocked = false; }
    }
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
    public void OnToggleCamera(InputAction.CallbackContext context) { if (context.performed) toggleCameraPressed = true; }
    public void OnAim(InputAction.CallbackContext context)
    {
        if (cameraController == null) return;
        cameraController.SetAiming(context.performed);
    }
    public void OnZoom(InputAction.CallbackContext context)
    {
        if (context.performed && cameraController != null)
        {
            cameraController.ApplyZoomTick(context.ReadValue<float>());
        }
    }
    public void OnPause(InputAction.CallbackContext context) { if (context.performed) pausePressed = true; }
    public void OnAttack(InputAction.CallbackContext context) { }
    public void OnNextWeapon(InputAction.CallbackContext context) { }
    public void OnPreviousWeapon(InputAction.CallbackContext context) { }
    public void ChangeState(PlayerState newState)
    {
        if (currentState != null) currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
    public void Move(Vector3 motion) => characterController.Move(motion * Time.deltaTime);
    public void ApplyGravity(ref Vector3 vel)
    {
        vel.y += gravity * Time.deltaTime;
        if (characterController.isGrounded && vel.y < 0f) vel.y = -2f;
    }
    public Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f) return Vector3.zero;
        Transform cameraTransform = cameraController?.CameraPivot ?? playerCamera;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();
        Vector3 direction = forward * input.y + right * input.x;
        if (direction.sqrMagnitude > 0.01f) direction.Normalize();
        return direction;
    }
    public bool IsGrounded() => characterController.isGrounded;
    public float GetCurrentSpeed() => currentState.GetSpeed();
    public bool IsRunPressed() => runPressed;
    public bool IsSprintPressed() => sprintPressed;
    public Vector2 GetMoveInput() => moveInput;
    public void SetRunBlocked(bool b) => runBlocked = b;
    public bool IsRunBlocked() => runBlocked;
    public void SetSprintBlocked(bool b) => sprintBlocked = b;
    public bool IsSprintBlocked() => sprintBlocked;
    public float GetLastRunTime() => lastRunTime;
    public float GetLastSprintTime() => lastSprintTime;
}