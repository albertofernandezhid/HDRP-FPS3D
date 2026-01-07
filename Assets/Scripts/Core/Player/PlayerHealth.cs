using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [System.Serializable]
    public struct HealthVibrationSettings
    {
        public float damageDuration;
        public float damageLowFreq;
        public float damageHighFreq;
        [Header("Death Effect")]
        public float deathDuration;
        public float deathLowFreq;
        public float deathHighFreq;
    }

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private ParticleSystem hurtParticles;
    [SerializeField] private ParticleSystem deathParticles;
    public HealthVibrationSettings vibrationSettings;

    [Header("Damage Overlay Settings")]
    [SerializeField] private Image damageOverlayImage;
    [SerializeField] private float overlayDuration = 0.5f;
    [SerializeField] private float maxOverlayAlpha = 0.5f;

    private float currentHealth;
    private PlayerController playerController;
    private PlayerAnimationController animationController;
    private CameraController cameraController;
    private Coroutine overlayCoroutine;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event Action<float> OnHealthChanged;
    public event Action<float, float> OnHealthChangedDetailed;
    public event Action OnDeath;
    public event Action<float> OnDamageTaken;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        animationController = GetComponentInChildren<PlayerAnimationController>();
        cameraController = GetComponentInChildren<CameraController>();
        currentHealth = maxHealth;

        if (damageOverlayImage != null)
        {
            Color c = damageOverlayImage.color;
            c.a = 0;
            damageOverlayImage.color = c;
        }

        NotifyHealthChange();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        OnDamageTaken?.Invoke(damage);
        NotifyHealthChange();

        PlayHurtEffects();

        if (playerController != null)
        {
            playerController.TriggerVibration(vibrationSettings.damageDuration, vibrationSettings.damageLowFreq, vibrationSettings.damageHighFreq);
        }

        if (animationController != null && currentHealth > 0)
        {
            animationController.TriggerTakeDamage();
        }

        if (currentHealth <= 0)
            Die();
    }

    public void Die()
    {
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        if (deathParticles != null)
            Instantiate(deathParticles, transform.position, Quaternion.identity);

        if (playerController != null)
        {
            playerController.TriggerVibration(vibrationSettings.deathDuration, vibrationSettings.deathLowFreq, vibrationSettings.deathHighFreq);
        }

        OnDeath?.Invoke();

        if (animationController != null)
        {
            animationController.SetDead(true);
        }

        if (playerController != null)
        {
            playerController.ChangeState(new DeathState(playerController));
        }
    }

    private void PlayHurtEffects()
    {
        if (animationController != null && hurtSounds != null && hurtSounds.Length > 0)
        {
            animationController.PlayRandomSound(hurtSounds, 0.8f);
        }

        if (damageOverlayImage != null && cameraController != null && cameraController.IsFirstPerson)
        {
            if (overlayCoroutine != null) StopCoroutine(overlayCoroutine);
            overlayCoroutine = StartCoroutine(FadeOverlay());
        }

        if (hurtParticles != null)
        {
            ParticleSystem particles = Instantiate(hurtParticles, transform.position, Quaternion.identity);
            Destroy(particles.gameObject, 2f);
        }
    }

    private IEnumerator FadeOverlay()
    {
        float elapsedTime = 0f;
        Color c = damageOverlayImage.color;

        while (elapsedTime < overlayDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(maxOverlayAlpha, 0, elapsedTime / overlayDuration);
            c.a = alpha;
            damageOverlayImage.color = c;
            yield return null;
        }

        c.a = 0;
        damageOverlayImage.color = c;
    }

    public void Heal(float amount)
    {
        if (currentHealth >= maxHealth) return;

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
        float healthPercent = currentHealth / maxHealth;
        OnHealthChanged?.Invoke(healthPercent);
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