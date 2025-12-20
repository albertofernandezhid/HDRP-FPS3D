using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private ParticleSystem hurtParticles;
    [SerializeField] private ParticleSystem deathParticles;

    private float currentHealth;
    private PlayerController playerController;
    private PlayerAnimationController animationController;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event System.Action<float> OnHealthChanged;
    public event System.Action<float, float> OnHealthChangedDetailed;
    public event System.Action OnDeath;
    public event System.Action<float> OnDamageTaken;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        animationController = GetComponentInChildren<PlayerAnimationController>();
        currentHealth = maxHealth;
        NotifyHealthChange();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        OnDamageTaken?.Invoke(damage);
        NotifyHealthChange();

        PlayHurtEffects();

        if (currentHealth <= 0)
            Die();
    }

    public void Die()
    {
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        if (deathParticles != null)
            Instantiate(deathParticles, transform.position, Quaternion.identity);

        OnDeath?.Invoke();

        if (playerController != null)
        {
            playerController.ChangeState(new DeathState(playerController));
        }
    }

    private void PlayHurtEffects()
    {
        if (hurtSound != null)
            AudioSource.PlayClipAtPoint(hurtSound, transform.position);

        if (hurtParticles != null)
        {
            ParticleSystem particles = Instantiate(hurtParticles, transform.position, Quaternion.identity);
            Destroy(particles.gameObject, 2f);
        }
    }

    public void Heal(float amount)
    {
        if (currentHealth >= maxHealth) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        NotifyHealthChange();
    }

    public void HealToFull()
    {
        if (currentHealth >= maxHealth) return;

        currentHealth = maxHealth;
        NotifyHealthChange();
    }

    private void NotifyHealthChange()
    {
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        OnHealthChangedDetailed?.Invoke(currentHealth, maxHealth);
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsFullHealth() => Mathf.Approximately(currentHealth, maxHealth);
    public bool IsAlive() => currentHealth > 0;

    public void SetMaxHealth(float newMax, bool healToNewMax = false)
    {
        maxHealth = newMax;
        if (healToNewMax)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        NotifyHealthChange();
    }

    public void Respawn(float healthPercent = 1f)
    {
        currentHealth = maxHealth * Mathf.Clamp01(healthPercent);

        if (playerController != null)
        {
            playerController.ChangeState(new IdleState(playerController));
        }

        if (animationController != null)
        {
            animationController.TriggerRespawn();
        }

        NotifyHealthChange();
    }
}