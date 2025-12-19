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
    public float gravity = -9.81f;
    public float rotationSpeed = 5f; // Reducido para mejor control
    public float rotationThreshold = 0.15f;

    [Header("References")]
    public CharacterController characterController;
    public Transform playerCamera;
    public StaminaSystem staminaSystem;
    public PlayerAnimationController animationController;

    private PlayerState currentState;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool jumpRequested;
    private bool runPressed;
    private bool sprintPressed;

    private void Awake()
    {
        if (animationController == null)
            animationController = GetComponentInChildren<PlayerAnimationController>();

        characterController = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += _ => jumpRequested = true;

        inputActions.Player.Run.performed += _ => runPressed = true;
        inputActions.Player.Run.canceled += _ => runPressed = false;

        inputActions.Player.Sprint.performed += _ => sprintPressed = true;
        inputActions.Player.Sprint.canceled += _ => sprintPressed = false;

        if (!staminaSystem)
            staminaSystem = GetComponent<StaminaSystem>();
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

        // Actualizar animaciones
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

    public void Move(Vector3 motion)
    {
        characterController.Move(motion * Time.deltaTime);
    }

    public void ApplyGravity(ref Vector3 vel)
    {
        if (characterController.isGrounded && vel.y < 0f)
            vel.y = -2f;

        vel.y += gravity * Time.deltaTime;
    }

    // ?? Método de rotación CORREGIDO (sin parámetros)
    public void RotateTowardsMovement()
    {
        if (moveInput.magnitude < rotationThreshold || moveInput.y < -0.3f)
            return; // No rotamos para movimiento hacia atrás

        Vector3 moveDirection = GetCameraRelativeDirection(moveInput);
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }

    // ?? Método alternativo con parámetro
    public void RotateTowardsInput(Vector2 input)
    {
        if (input.magnitude < rotationThreshold || input.y < -0.3f)
            return;

        Vector3 moveDirection = GetCameraRelativeDirection(input);
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }

    public Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
            return Vector3.zero;

        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;

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
    public float GetRotationSpeed() => rotationSpeed;
}