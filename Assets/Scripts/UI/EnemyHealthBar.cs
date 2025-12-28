using UnityEngine;
using UnityEngine.UI;

namespace HDRP_FPS3D.Enemy
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float heightOffset = 2f;

        private Camera mainCamera;

        private void Start()
        {
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (enemyHealth == null) enemyHealth = GetComponentInParent<EnemyHealth>();

            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
            }

            if (healthSlider != null && enemyHealth != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                UpdateHealthDisplay(enemyHealth.CurrentHealth);
            }
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }

            UpdatePosition();
        }

        public void Initialize(EnemyHealth healthComponent)
        {
            enemyHealth = healthComponent;
            if (healthSlider != null && enemyHealth != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                UpdateHealthDisplay(enemyHealth.CurrentHealth);
            }
        }

        public void UpdateHealth(float currentHealth)
        {
            UpdateHealthDisplay(currentHealth);
        }

        private void UpdateHealthDisplay(float currentHealth)
        {
            if (healthSlider != null && enemyHealth != null)
            {
                float normalizedHealth = currentHealth / enemyHealth.MaxHealth;
                healthSlider.value = normalizedHealth;
            }
        }

        private void UpdatePosition()
        {
            if (enemyHealth != null)
            {
                Vector3 worldPosition = enemyHealth.transform.position + Vector3.up * heightOffset;
                transform.position = worldPosition;
            }
        }
    }
}