using UnityEngine;
using DG.Tweening;

public class PickupItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private ThrowableData weaponData;
    [SerializeField] private int ammoAmount = 5;

    [Header("Bounce Animation")]
    [SerializeField] private bool enableBounce = true;
    [SerializeField] private float bounceHeight = 0.5f;
    [SerializeField] private float bounceDuration = 1f;
    [SerializeField] private Ease bounceEase = Ease.InOutSine;
    [SerializeField] private bool customBounceStart = false;
    [SerializeField] private float bounceStartDelay = 0f;
    [SerializeField][Range(0f, 10f)] private float bounceOvershoot = 1.70158f;

    [Header("Rotation Animation")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private RotateMode rotateMode = RotateMode.LocalAxisAdd;
    [SerializeField] private Ease rotationEase = Ease.Linear;
    [SerializeField] private bool customRotationStart = false;
    [SerializeField] private float rotationStartDelay = 0f;

    [Header("Scale Animation (Pulse)")]
    [SerializeField] private bool enableScalePulse = false;
    [SerializeField] private Vector3 scalePulseTo = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private float scalePulseDuration = 0.5f;
    [SerializeField] private Ease scalePulseEase = Ease.InOutSine;
    [SerializeField] private int scalePulseLoops = -1;
    [SerializeField] private LoopType scalePulseLoopType = LoopType.Yoyo;

    [Header("Hover Animation")]
    [SerializeField] private bool enableHover = false;
    [SerializeField] private float hoverHeight = 0.3f;
    [SerializeField] private float hoverDuration = 2f;
    [SerializeField] private Ease hoverEase = Ease.InOutSine;

    [Header("Effects")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPosition;
    private Vector3 originalScale;
    private Tween bounceTween;
    private Tween rotationTween;
    private Tween scalePulseTween;
    private Tween hoverTween;

    private void Start()
    {
        SetupPickupState();
        InitializeAnimations();
    }

    private void SetupPickupState()
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

        ProjectileController proj = GetComponent<ProjectileController>();
        if (proj != null)
        {
            proj.enabled = false;
        }
    }

    private void InitializeAnimations()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;

        SetupBounceAnimation();
        SetupRotationAnimation();
        SetupScalePulseAnimation();
        SetupHoverAnimation();
    }

    private void SetupBounceAnimation()
    {
        if (!enableBounce) return;

        bounceTween = transform.DOMoveY(startPosition.y + bounceHeight, bounceDuration)
            .SetEase(bounceEase, bounceOvershoot)
            .SetLoops(-1, LoopType.Yoyo);

        if (customBounceStart)
        {
            bounceTween.SetDelay(bounceStartDelay);
        }
    }

    private void SetupRotationAnimation()
    {
        if (!enableRotation) return;

        float rotationDuration = 360f / rotationSpeed;
        rotationTween = transform.DORotate(rotationAxis * 360f, rotationDuration, rotateMode)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(rotationEase);

        if (customRotationStart)
        {
            rotationTween.SetDelay(rotationStartDelay);
        }
    }

    private void SetupScalePulseAnimation()
    {
        if (!enableScalePulse) return;

        scalePulseTween = transform.DOScale(scalePulseTo, scalePulseDuration)
            .SetEase(scalePulseEase)
            .SetLoops(scalePulseLoops, scalePulseLoopType);
    }

    private void SetupHoverAnimation()
    {
        if (!enableHover) return;

        hoverTween = transform.DOMoveY(startPosition.y + hoverHeight, hoverDuration)
            .SetEase(hoverEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    [ContextMenu("Play Animations")]
    public void PlayAnimations()
    {
        StopAllAnimations();
        InitializeAnimations();
    }

    [ContextMenu("Stop Animations")]
    public void StopAllAnimations()
    {
        KillTweenIfActive(ref bounceTween);
        KillTweenIfActive(ref rotationTween);
        KillTweenIfActive(ref scalePulseTween);
        KillTweenIfActive(ref hoverTween);
    }

    [ContextMenu("Pause Animations")]
    public void PauseAllAnimations()
    {
        PauseTweenIfActive(bounceTween);
        PauseTweenIfActive(rotationTween);
        PauseTweenIfActive(scalePulseTween);
        PauseTweenIfActive(hoverTween);
    }

    [ContextMenu("Resume Animations")]
    public void ResumeAllAnimations()
    {
        ResumeTweenIfActive(bounceTween);
        ResumeTweenIfActive(rotationTween);
        ResumeTweenIfActive(scalePulseTween);
        ResumeTweenIfActive(hoverTween);
    }

    [ContextMenu("Reset Position and Scale")]
    public void ResetTransform()
    {
        StopAllAnimations();
        transform.position = startPosition;
        transform.localScale = originalScale;
    }

    private void KillTweenIfActive(ref Tween tween)
    {
        if (tween != null && tween.IsActive())
        {
            tween.Kill();
            tween = null;
        }
    }

    private void PauseTweenIfActive(Tween tween)
    {
        if (tween != null && tween.IsActive())
        {
            tween.Pause();
        }
    }

    private void ResumeTweenIfActive(Tween tween)
    {
        if (tween != null && tween.IsActive())
        {
            tween.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponManager weaponManager = other.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                if (weaponManager.PickupWeapon(weaponData, ammoAmount))
                {
                    PlayPickupEffects();
                    DestroyPickup();
                }
            }
        }
    }

    private void PlayPickupEffects()
    {
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
    }

    private void DestroyPickup()
    {
        StopAllAnimations();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        StopAllAnimations();
    }

    private void OnValidate()
    {
        bounceDuration = Mathf.Max(0.1f, bounceDuration);
        rotationSpeed = Mathf.Max(0.1f, rotationSpeed);
        scalePulseDuration = Mathf.Max(0.1f, scalePulseDuration);
        hoverDuration = Mathf.Max(0.1f, hoverDuration);
    }
}