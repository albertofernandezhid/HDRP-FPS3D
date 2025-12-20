using UnityEngine;
using DG.Tweening;

public class PickupItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private ThrowableData weaponData;
    [SerializeField] private int ammoAmount = 5;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bounceHeight = 0.5f;
    [SerializeField] private float bounceDuration = 1f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPosition;
    private Tween bounceTween;
    private Tween rotationTween;

    private void Start()
    {
        SetupPickupState();
        SetupAnimations();
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

    private void SetupAnimations()
    {
        startPosition = transform.position;

        bounceTween = transform.DOMoveY(startPosition.y + bounceHeight, bounceDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        rotationTween = transform.DORotate(new Vector3(0, 360, 0), rotationSpeed, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
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
                    if (pickupEffect != null)
                        Instantiate(pickupEffect, transform.position, Quaternion.identity);

                    if (pickupSound != null)
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position);

                    DestroyPickup();
                }
            }
        }
    }

    private void DestroyPickup()
    {
        if (bounceTween != null && bounceTween.IsActive())
        {
            bounceTween.Kill();
            bounceTween = null;
        }

        if (rotationTween != null && rotationTween.IsActive())
        {
            rotationTween.Kill();
            rotationTween = null;
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