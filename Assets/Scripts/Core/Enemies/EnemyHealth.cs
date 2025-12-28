using UnityEngine;

namespace HDRP_FPS3D.Enemy
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        public float MaxHealth = 100f;
        public GameObject DeathEffect;

        private float _currentHealth;
        private bool _isDead;
        private EnemyHealthBar _healthBar;

        public float CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        private void Start()
        {
            _currentHealth = MaxHealth;
            _healthBar = GetComponentInChildren<EnemyHealthBar>();

            if (_healthBar != null)
            {
                _healthBar.Initialize(this);
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            _currentHealth -= damage;

            if (_healthBar != null)
            {
                _healthBar.UpdateHealth(_currentHealth);
            }

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