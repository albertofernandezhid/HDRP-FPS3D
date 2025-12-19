using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animator Reference")]
    public Animator animator;

    [Header("Parameter Names")]
    public string speedParam = "Speed";
    public string isGroundedParam = "IsGrounded";
    public string verticalVelocityParam = "VerticalVelocity";
    public string isMovingParam = "IsMoving";
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat(speedParam, speed);
    }

    public void SetGrounded(bool isGrounded)
    {
        if (animator != null)
            animator.SetBool(isGroundedParam, isGrounded);
    }

    public void SetVerticalVelocity(float velocity)
    {
        if (animator != null)
            animator.SetFloat(verticalVelocityParam, velocity);
    }

    public void SetMoving(bool isMoving)
    {
        if (animator != null)
            animator.SetBool(isMovingParam, isMoving);
    }

    public void SetMovementDirection(Vector2 moveInput)
    {
        if (animator != null)
        {
            animator.SetFloat(moveXParam, moveInput.x);
            animator.SetFloat(moveYParam, moveInput.y);
        }
    }

    // Métodos simplificados para los estados
    public void SetIdle()
    {
        SetSpeed(0f);
        SetMoving(false);
        SetMovementDirection(Vector2.zero);
    }

    public void SetWalking()
    {
        SetSpeed(1f);
        SetMoving(true);
    }

    public void SetRunning()
    {
        SetSpeed(2f);
        SetMoving(true);
    }

    public void SetSprinting()
    {
        SetSpeed(3f);
        SetMoving(true);
    }

    public void SetJumping()
    {
        SetGrounded(false);
        SetVerticalVelocity(1f);
    }

    public void UpdateAnimations(Vector2 moveInput, float speed, bool isGrounded, float verticalVelocity)
    {
        // Actualizar parámetros básicos
        SetGrounded(isGrounded);
        SetVerticalVelocity(verticalVelocity);
        SetMovementDirection(moveInput);

        // Normalizar speed para parámetro animator (0-1)
        float normalizedSpeed = Mathf.Clamp01(speed / 9f); // Dividir por sprintSpeed
        animator.SetFloat(speedParam, normalizedSpeed);

        // Actualizar si está moviéndose
        SetMoving(moveInput.magnitude > 0.1f);
    }
}