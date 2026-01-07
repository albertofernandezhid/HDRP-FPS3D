using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    [System.Serializable]
    public struct MovementSettings
    {
        public float walkSpeed;
        public float runSpeed;
        public float sprintSpeed;
        public float jumpHeight;
        public float jumpHeightMultiplier;
        public float gravity;
        public float lookSensitivity;
        public float zoomSensitivity;
    }

    [System.Serializable]
    public struct VibrationSettings
    {
        public bool hapticsEnabled;
        [Header("Jump Effect")]
        public float jumpDuration;
        public float jumpLowFreq;
        public float jumpHighFreq;
        [Header("Land Effect")]
        public float landDuration;
        public float landLowFreq;
        public float landHighFreq;
        [Header("Attack Effect")]
        public float attackDuration;
        public float attackLowFreq;
        public float attackHighFreq;
        [Header("Damage Effect")]
        public float damageDuration;
        public float damageLowFreq;
        public float damageHighFreq;
    }

    public MovementSettings moveSettings = new MovementSettings { walkSpeed = 3f, runSpeed = 6f, sprintSpeed = 9f, jumpHeight = 2f, jumpHeightMultiplier = 1f, gravity = -9.81f, lookSensitivity = 1f, zoomSensitivity = 0.1f };
    public VibrationSettings vibrationSettings;

    [HideInInspector] public float originalWalkSpeed;
    [HideInInspector] public float originalRunSpeed;
    [HideInInspector] public float originalSprintSpeed;
    [HideInInspector] public float originalJumpHeight;
    [HideInInspector] public float originalJumpHeightMultiplier;

    public CharacterController characterController;
    public CameraController cameraController;
    public Transform playerCamera;
    public StaminaSystem staminaSystem;
    public WeaponManager weaponManager;
    public PlayerAnimationController animationController;

    public PlayerState currentState;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;
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
    private bool wasGrounded;

    private void Awake()
    {
        InitializeComponents();
        InitializeInputSystem();
        SaveOriginalValues();
    }

    private void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraController == null) cameraController = GetComponentInChildren<CameraController>();
        if (staminaSystem == null) staminaSystem = GetComponent<StaminaSystem>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();
        if (animationController == null) animationController = GetComponent<PlayerAnimationController>();
    }

    private void InitializeInputSystem()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(this);
    }

    private void SaveOriginalValues()
    {
        originalWalkSpeed = moveSettings.walkSpeed;
        originalRunSpeed = moveSettings.runSpeed;
        originalSprintSpeed = moveSettings.sprintSpeed;
        originalJumpHeight = moveSettings.jumpHeight;
        originalJumpHeightMultiplier = moveSettings.jumpHeightMultiplier;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable()
    {
        StopAllMotors();
        inputActions.Disable();
    }
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

        bool isGroundedNow = IsGrounded();
        if (isGroundedNow && !wasGrounded && velocity.y < -5f)
        {
            TriggerVibration(vibrationSettings.landDuration, vibrationSettings.landLowFreq, vibrationSettings.landHighFreq);
        }
        wasGrounded = isGroundedNow;

        currentState.HandleMovement(moveInput, ref velocity, jumpRequested);
        PlayerState nextState = currentState.UpdateState();
        if (nextState != currentState)
        {
            currentState.Exit();
            currentState = nextState;
            currentState.Enter();
        }
        if (animationController != null)
        {
            animationController.UpdateAnimations(moveInput, GetCurrentSpeed(), velocity, isGroundedNow);
        }
        ResetFrameInputs();
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
            if (!isPaused) StopAllMotors();
            pausePressed = false;
        }
    }

    private void ResetFrameInputs() => jumpRequested = false;

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            jumpRequested = true;
            animationController?.TriggerJump();
            TriggerVibration(vibrationSettings.jumpDuration, vibrationSettings.jumpLowFreq, vibrationSettings.jumpHighFreq);
        }
    }

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

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && weaponManager != null && animationController != null)
        {
            if (weaponManager.CanThrow() && !animationController.IsAnimationPlaying("ThrowObject"))
            {
                animationController.TriggerThrow();
                TriggerVibration(vibrationSettings.attackDuration, vibrationSettings.attackLowFreq, vibrationSettings.attackHighFreq);
            }
        }
    }

    public void OnNextWeapon(InputAction.CallbackContext context) { }
    public void OnPreviousWeapon(InputAction.CallbackContext context) { }

    public void NotifyTakeDamage()
    {
        TriggerVibration(vibrationSettings.damageDuration, vibrationSettings.damageLowFreq, vibrationSettings.damageHighFreq);
    }

    public void TriggerVibration(float duration, float lowFreq, float highFreq)
    {
        if (!vibrationSettings.hapticsEnabled) return;
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null) StartCoroutine(VibrationCoroutine(gamepad, duration, lowFreq, highFreq));
    }

    private IEnumerator VibrationCoroutine(Gamepad gamepad, float duration, float low, float high)
    {
        gamepad.SetMotorSpeeds(low, high);
        yield return new WaitForSecondsRealtime(duration);
        gamepad.SetMotorSpeeds(0f, 0f);
    }

    private void StopAllMotors() => Gamepad.current?.SetMotorSpeeds(0f, 0f);

    public void ChangeState(PlayerState newState)
    {
        if (currentState != null) currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Move(Vector3 motion) => characterController.Move(motion * Time.deltaTime);

    public void ApplyGravity(ref Vector3 vel)
    {
        if (characterController.isGrounded && vel.y < 0f)
        {
            vel.y = -2f;
        }
        else
        {
            float gravityFallMultiplier = (vel.y < 0) ? 2.5f : 1f;
            vel.y += moveSettings.gravity * gravityFallMultiplier * Time.deltaTime;
        }
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