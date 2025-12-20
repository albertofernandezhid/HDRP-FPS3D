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
    public string takeDamageParam = "TakeDamage";
    public string dieParam = "Die";
    public string respawnParam = "Respawn";
    public string isDeadParam = "IsDead";

    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnDeath += OnPlayerDeath;
            playerHealth.OnDamageTaken += OnPlayerDamaged;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= OnPlayerDeath;
            playerHealth.OnDamageTaken -= OnPlayerDamaged;
        }
    }

    private void OnPlayerDamaged(float damage)
    {
        if (animator != null)
            animator.SetTrigger(takeDamageParam);
    }

    private void OnPlayerDeath()
    {
        if (animator != null)
        {
            animator.SetTrigger(dieParam);
            animator.SetBool(isDeadParam, true);
        }
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
        if (animator == null) return;

        bool isDead = animator.GetBool(isDeadParam);
        if (isDead)
        {
            animator.SetFloat(speedParam, 0f);
            animator.SetBool(isMovingParam, false);
            return;
        }

        SetGrounded(isGrounded);
        SetVerticalVelocity(verticalVelocity);
        SetMovementDirection(moveInput);

        float normalizedSpeed = Mathf.Clamp01(speed / 9f);
        animator.SetFloat(speedParam, normalizedSpeed);
        animator.SetBool(isMovingParam, moveInput.magnitude > 0.1f);
    }

    public void TriggerRespawn()
    {
        if (animator != null)
        {
            animator.SetTrigger(respawnParam);
            animator.SetBool(isDeadParam, false);
        }
    }
}