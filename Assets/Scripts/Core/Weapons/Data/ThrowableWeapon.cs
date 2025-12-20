using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowableWeapon", menuName = "Weapons/Throwable Weapon")]
public class ThrowableWeapon : ThrowableData
{
    [Header("Throw Settings")]
    [SerializeField] private float arcHeight = 0.5f;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private GameObject impactEffect;

    public override void OnThrow(Vector3 position, Quaternion rotation, Vector3 direction)
    {
        if (prefab == null) return;

        GameObject projectile = Instantiate(prefab, position, rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb == null)
            rb = projectile.AddComponent<Rigidbody>();

        Vector3 throwDirection = direction.normalized;
        Vector3 arc = Vector3.up * arcHeight;

        rb.AddForce((throwDirection + arc) * throwForce, ForceMode.Impulse);

        ProjectileController projController = projectile.GetComponent<ProjectileController>();
        if (projController == null)
            projController = projectile.AddComponent<ProjectileController>();

        projController.Initialize(damage, impactEffect);

        if (throwSound != null)
            AudioSource.PlayClipAtPoint(throwSound, position);
    }

    public override void OnPickup()
    {

    }
}