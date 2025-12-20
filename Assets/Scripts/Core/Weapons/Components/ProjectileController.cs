using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject impactEffect;
    private float currentLifetime;
    private float damage;

    public void Initialize(float dmg)
    {
        damage = dmg;
        currentLifetime = lifetime;
    }

    public void Initialize(float dmg, GameObject impactPrefab)
    {
        damage = dmg;
        impactEffect = impactPrefab;
        currentLifetime = lifetime;
    }

    private void Update()
    {
        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0f)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(damage);

        Destroy(gameObject);
    }
}