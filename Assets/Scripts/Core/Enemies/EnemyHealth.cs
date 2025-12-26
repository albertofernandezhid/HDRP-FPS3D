using UnityEngine;

namespace HDRP_FPS3D.Enemy
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        public float MaxHealth = 100f;
        public GameObject DeathEffect;

        private float _currentHealth;
        private bool _isDead;

        public float CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        private void Start()
        {
            _currentHealth = MaxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            _currentHealth -= damage;
            Debug.Log($"Enemigo impactado. Vida restante: {_currentHealth}"); // Para debugear en consola

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            _isDead = true;

            if (DeathEffect != null)
            {
                Instantiate(DeathEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}