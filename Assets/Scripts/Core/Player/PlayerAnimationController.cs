using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void UpdateAnimations(Vector2 moveInput, float speed)
    {
        if (animator == null) return;
        float moveX = moveInput.x;
        float moveY = moveInput.y;
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
            moveX = moveInput.x;
            moveY = moveInput.y;
        }
        animator.SetFloat("MoveX", moveX, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveY, 0.1f, Time.deltaTime);
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    public void TriggerJump()
    {
        animator?.SetTrigger("jump");
    }

    public void TriggerTakeDamage()
    {
        if (animator == null) return;
        animator.ResetTrigger("TakeDamage");
        animator.SetTrigger("TakeDamage");
    }

    public void TriggerAttack()
    {
        if (animator == null) return;
        if (!IsAnimationPlaying("Attack"))
        {
            animator.SetTrigger("Attack");
        }
    }

    public void TriggerThrow()
    {
        if (animator == null) return;
        if (!IsAnimationPlaying("ThrowObject"))
        {
            animator.SetTrigger("ThrowObject");
        }
    }

    public void TriggerRespawn()
    {
        if (animator == null) return;
        animator.SetBool("IsDead", false);
        animator.SetTrigger("Respawn");
    }

    public void SetDead(bool isDead)
    {
        if (animator == null) return;
        animator.SetBool("IsDead", isDead);
        if (isDead)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    public bool IsAnimationPlaying(string stateName)
    {
        if (animator == null) return false;
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }
}