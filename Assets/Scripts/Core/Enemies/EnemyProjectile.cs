using UnityEngine;

namespace HDRP_FPS3D.Enemy
{
    public class EnemyProjectile : MonoBehaviour
    {
        [HideInInspector] public float Damage;
        public float LifeTime = 5f;
        public GameObject ImpactEffect;

        private void Start()
        {
            Destroy(gameObject, LifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy") || other.isTrigger) return;

            if (other.CompareTag("Player"))
            {
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(Damage);
                }
            }

            if (ImpactEffect != null)
            {
                Instantiate(ImpactEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}