using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileWeapon", menuName = "Weapons/Projectile Weapon")]
public class ProjectileWeapon : ThrowableData
{
    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private bool useGravity = false;
    [SerializeField] private GameObject trailEffect;

    public override void OnThrow(Vector3 position, Quaternion rotation, Vector3 direction)
    {
        if (prefab == null) return;

        GameObject projectile = Instantiate(prefab, position, rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb == null)
            rb = projectile.AddComponent<Rigidbody>();

        rb.useGravity = useGravity;
        rb.linearVelocity = direction.normalized * projectileSpeed;

        // A�adir trail si existe
        if (trailEffect != null)
        {
            GameObject trail = Instantiate(trailEffect, projectile.transform);
        }

        ProjectileController projController = projectile.GetComponent<ProjectileController>();
        if (projController == null)
            projController = projectile.AddComponent<ProjectileController>();

        projController.Initialize(Damage, null);
    }
}