using UnityEngine;
using DG.Tweening;

public class PowerUpPickup : MonoBehaviour
{
    [Header("PowerUp Settings")]
    [SerializeField] private PowerUpData powerUpData;

    [Header("Bounce Animation")]
    [SerializeField] private bool enableBounce = true;
    [SerializeField] private float bounceHeight = 0.5f;
    [SerializeField] private float bounceDuration = 1f;
    [SerializeField] private Ease bounceEase = Ease.InOutSine;
    [SerializeField] private LoopType bounceLoopType = LoopType.Yoyo;
    [SerializeField] private int bounceLoops = -1;

    [Header("Rotation Animation")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationDuration = 2f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private Ease rotationEase = Ease.Linear;
    [SerializeField] private LoopType rotationLoopType = LoopType.Incremental;
    [SerializeField] private int rotationLoops = -1;

    [Header("Pulse Animation")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.8f;
    [SerializeField] private Ease pulseEase = Ease.InOutSine;
    [SerializeField] private LoopType pulseLoopType = LoopType.Yoyo;
    [SerializeField] private int pulseLoops = -1;

    [Header("Pickup Disappear Effect")]
    [SerializeField] private float disappearDuration = 0.6f;
    [SerializeField] private float shrinkScale = 0.2f;
    [SerializeField] private float floatUpDistance = 1f;
    [SerializeField] private Ease disappearEase = Ease.InOutSine;

    [Header("Components")]
    [SerializeField] private Renderer pickupRenderer;
    [SerializeField] private Collider pickupCollider;

    private Vector3 startPosition;
    private Vector3 originalScale;
    private Tween bounceTween;
    private Tween rotationTween;
    private Tween pulseTween;
    private bool isBeingPickedUp = false;

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
            if (mat != null && powerUpData.pickupColor.a > 0.05f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", powerUpData.pickupColor * 0.5f);
            }
        }
    }

    private void SetupAnimations()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;

        if (enableBounce)
        {
            bounceTween = transform.DOMoveY(startPosition.y + bounceHeight, bounceDuration)
                .SetEase(bounceEase)
                .SetLoops(bounceLoops, bounceLoopType);
        }

        if (enableRotation)
        {
            rotationTween = transform.DORotate(rotationAxis * 360f, rotationDuration, RotateMode.LocalAxisAdd)
                .SetEase(rotationEase)
                .SetLoops(rotationLoops, rotationLoopType);
        }

        if (enablePulse)
        {
            pulseTween = transform.DOScale(originalScale * pulseScale, pulseDuration)
                .SetEase(pulseEase)
                .SetLoops(pulseLoops, pulseLoopType);
        }
    }

    private void SetupPhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
        else
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isBeingPickedUp)
        {
            isBeingPickedUp = true;

            if (pickupCollider != null)
                pickupCollider.enabled = false;

            PowerUpManager powerUpManager = Object.FindFirstObjectByType<PowerUpManager>();
            if (powerUpManager != null)
            {
                if (powerUpManager.ApplyPowerUp(powerUpData, other.gameObject))
                {
                    PlayPickupEffects();
                    PlaySmoothDisappearAnimation();
                }
            }
        }
    }

    private void PlayPickupEffects()
    {
        if (powerUpData.pickupParticles != null)
        {
            ParticleSystem particles = Instantiate(powerUpData.pickupParticles, transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = particles.main;
            main.startColor = powerUpData.pickupColor;
            Destroy(particles.gameObject, 2f);
        }

        if (powerUpData.pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(powerUpData.pickupSound, transform.position);
        }
    }

    private void PlaySmoothDisappearAnimation()
    {
        if (bounceTween != null && bounceTween.IsActive())
            bounceTween.Kill();

        if (rotationTween != null && rotationTween.IsActive())
            rotationTween.Kill();

        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();

        Sequence disappearSequence = DOTween.Sequence();

        disappearSequence.Append(transform.DOScale(originalScale * shrinkScale, disappearDuration)
            .SetEase(disappearEase));

        disappearSequence.Join(transform.DOMoveY(transform.position.y + floatUpDistance, disappearDuration)
            .SetEase(Ease.OutQuad));

        if (pickupRenderer != null)
        {
            Material mat = pickupRenderer.material;
            if (mat != null)
            {
                disappearSequence.Join(mat.DOFade(0f, disappearDuration * 0.8f)
                    .SetEase(Ease.InQuad));
            }
        }

        disappearSequence.OnComplete(() => Destroy(gameObject));
        disappearSequence.SetUpdate(true);
    }

    private void OnDestroy()
    {
        if (bounceTween != null && bounceTween.IsActive())
            bounceTween.Kill();

        if (rotationTween != null && rotationTween.IsActive())
            rotationTween.Kill();

        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
    }
}