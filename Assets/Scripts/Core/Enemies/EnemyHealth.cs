using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace HDRP_FPS3D.Enemy
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        public float MaxHealth = 100f;
        public GameObject DeathEffect;
        public float CanvasDisableDelay = 1.5f;

        private float _currentHealth;
        private bool _isDead;
        private EnemyHealthBar _healthBar;
        private Animator _animator;

        private MeleeStateMachine _meleeSM;
        private RangeStateMachine _rangeSM;

        public float CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        private void Start()
        {
            _currentHealth = MaxHealth;
            _animator = GetComponent<Animator>();
            _meleeSM = GetComponent<MeleeStateMachine>();
            _rangeSM = GetComponent<RangeStateMachine>();
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

            if (_meleeSM != null)
            {
                _meleeSM.PlayRandomSound(_meleeSM.DamageSounds, 0.8f);
            }
            else if (_rangeSM != null)
            {
                _rangeSM.PlayRandomSound(_rangeSM.DamageSounds, 0.8f);
            }

            if (_animator != null)
            {
                _animator.SetTrigger("TakeDamage");
            }

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
            if (_isDead) return;
            _isDead = true;

            StartCoroutine(DisableCanvasDelayed());

            if (_meleeSM != null)
            {
                _meleeSM.PlayDeathSound();
            }
            else if (_rangeSM != null)
            {
                _rangeSM.PlayDeathSound();
            }

            if (_animator != null)
            {
                _animator.SetBool("IsDead", true);
            }

            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            Collider mainCollider = GetComponent<Collider>();
            if (mainCollider != null)
            {
                mainCollider.enabled = false;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
            }

            MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in allScripts)
            {
                if (script == null) continue;
                System.Type type = script.GetType();
                if (type != typeof(Animator) && type != typeof(EnemyHealth) && type != typeof(AudioSource))
                {
                    script.enabled = false;
                }
            }

            if (DeathEffect != null)
            {
                Instantiate(DeathEffect, transform.position, Quaternion.identity);
            }
        }

        private IEnumerator DisableCanvasDelayed()
        {
            yield return new WaitForSeconds(CanvasDisableDelay);
            Canvas childCanvas = GetComponentInChildren<Canvas>();
            if (childCanvas != null)
            {
                childCanvas.enabled = false;
            }
        }
    }
}