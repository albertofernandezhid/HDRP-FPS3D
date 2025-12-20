using UnityEngine;
using DG.Tweening;

public class PowerUpPickup : MonoBehaviour
{
    [Header("PowerUp Settings")]
    [SerializeField] private PowerUpData powerUpData;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bounceHeight = 0.5f;
    [SerializeField] private float bounceDuration = 1f;

    [Header("Components")]
    [SerializeField] private Renderer pickupRenderer;
    [SerializeField] private Light pickupLight;

    private Vector3 startPosition;
    private Tween bounceTween;
    private Tween rotationTween;

    private void Start()
    {
        SetupVisuals();
        SetupAnimations();
        SetupPhysics();
    }

    private void SetupVisuals()
    {
        if (pickupRenderer != null)
        {
            Material mat = pickupRenderer.material;
            if (mat != null)
            {
                mat.color = powerUpData.pickupColor;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", powerUpData.pickupColor * 0.5f);
            }
        }

        if (pickupLight != null)
        {
            pickupLight.color = powerUpData.pickupColor;
        }
    }

    private void SetupAnimations()
    {
        startPosition = transform.position;

        // Bouncing (como tu PickupItem)
        bounceTween = transform.DOMoveY(startPosition.y + bounceHeight, bounceDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Rotación
        rotationTween = transform.DORotate(new Vector3(0, 360, 0), rotationSpeed, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    private void SetupPhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PowerUpManager powerUpManager = Object.FindFirstObjectByType<PowerUpManager>();
            if (powerUpManager != null)
            {
                if (powerUpManager.ApplyPowerUp(powerUpData, other.gameObject))
                {
                    PlayPickupEffects();
                    DestroyPickup();
                }
            }
        }
    }

    private void PlayPickupEffects()
    {
        // Partículas
        if (powerUpData.pickupParticles != null)
        {
            ParticleSystem particles = Instantiate(powerUpData.pickupParticles,
                transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = particles.main;
            main.startColor = powerUpData.pickupColor;
        }

        // Sonido
        if (powerUpData.pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(powerUpData.pickupSound, transform.position);
        }
    }

    private void DestroyPickup()
    {
        if (bounceTween != null && bounceTween.IsActive())
        {
            bounceTween.Kill();
        }

        if (rotationTween != null && rotationTween.IsActive())
        {
            rotationTween.Kill();
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (bounceTween != null && bounceTween.IsActive())
            bounceTween.Kill();

        if (rotationTween != null && rotationTween.IsActive())
            rotationTween.Kill();
    }
}