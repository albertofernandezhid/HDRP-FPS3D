using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private Text healthText;

    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(playerHealth.GetHealthPercentage());
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 1;
        }
    }

    private void UpdateHealthBar(float healthPercentage)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthPercentage;
        }

        if (healthFill != null && healthGradient != null)
        {
            healthFill.color = healthGradient.Evaluate(healthPercentage);
        }

        if (healthText != null && playerHealth != null)
        {
            healthText.text = $"{playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth:F0}";
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }
}