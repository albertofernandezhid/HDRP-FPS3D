using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    [Header("Audio Settings")]
    public AudioSource PlayerAudioSource;
    public AudioClip[] FootstepSounds;
    public AudioClip[] AttackSounds;
    public AudioClip[] JumpSounds;
    public AudioClip[] LandSounds;
    [Range(0, 1)] public float MasterVolume = 1f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (PlayerAudioSource == null)
        {
            PlayerAudioSource = GetComponent<AudioSource>();
        }
    }

    public void UpdateAnimatorReference(Animator newAnimator)
    {
        animator = newAnimator;
    }

    public void UpdateAnimations(Vector2 moveInput, float speed, Vector3 velocity, bool isGrounded)
    {
        if (animator == null || !animator.gameObject.activeInHierarchy) return;
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

        if (!isGrounded && velocity.y < -0.1f)
        {
            animator.SetBool("landing", true);
        }
        else if (isGrounded)
        {
            animator.SetBool("landing", false);
        }
    }

    public void TriggerJump()
    {
        if (animator != null && animator.gameObject.activeInHierarchy)
        {
            animator.SetTrigger("jump");
            animator.SetBool("landing", false);
            PlayRandomSound(JumpSounds, 0.7f);
        }
    }

    public void TriggerTakeDamage()
    {
        if (animator == null || !animator.gameObject.activeInHierarchy) return;
        animator.ResetTrigger("TakeDamage");
        animator.SetTrigger("TakeDamage");
    }

    public void TriggerAttack()
    {
        if (animator == null || !animator.gameObject.activeInHierarchy) return;
        if (!IsAnimationPlaying("Attack"))
        {
            animator.SetTrigger("Attack");
        }
    }

    public void TriggerThrow()
    {
        if (animator == null || !animator.gameObject.activeInHierarchy) return;
        if (!IsAnimationPlaying("ThrowObject"))
        {
            animator.SetTrigger("ThrowObject");
        }
    }

    public void TriggerRespawn()
    {
        if (animator == null || !animator.gameObject.activeInHierarchy) return;
        animator.SetBool("IsDead", false);
        animator.SetTrigger("Respawn");
    }

    public void SetDead(bool isDead)
    {
        if (animator == null || !animator.gameObject.activeInHierarchy) return;
        animator.SetBool("IsDead", isDead);
        if (isDead)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    public bool IsAnimationPlaying(string stateName)
    {
        if (animator == null || !animator.gameObject.activeInHierarchy) return false;
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

    public void PlayRandomSound(AudioClip[] clips, float volumeMultiplier = 1f)
    {
        if (clips == null || clips.Length == 0 || PlayerAudioSource == null) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayerAudioSource.pitch = Random.Range(0.9f, 1.1f);
        PlayerAudioSource.PlayOneShot(clip, MasterVolume * volumeMultiplier);
    }
}