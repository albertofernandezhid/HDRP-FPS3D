using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Graphic healthFillGraphic;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private Text healthText;

    private PlayerHealth playerHealth;

    private void Start()
    {
        InitializeHealthBar();
        FindPlayerHealth();
    }

    private void InitializeHealthBar()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (healthFillGraphic == null && healthSlider != null && healthSlider.fillRect != null)
            healthFillGraphic = healthSlider.fillRect.GetComponent<Graphic>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            UpdateVisuals(healthSlider.value);
        }
    }

    private void FindPlayerHealth()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthBar;
                UpdateHealthBar(playerHealth.GetHealthPercentage());
            }
        }
        else
        {
            Invoke(nameof(FindPlayerHealth), 0.5f);
        }
    }

    private void UpdateHealthBar(float healthPercentage)
    {
        UpdateVisuals(healthPercentage);

        if (healthText != null && playerHealth != null)
        {
            healthText.text = $"{playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth:F0}";
        }
    }

    private void UpdateVisuals(float percentage)
    {
        if (healthSlider != null)
        {
            healthSlider.value = percentage;
        }

        if (healthFillGraphic != null && healthGradient != null)
        {
            healthFillGraphic.color = healthGradient.Evaluate(percentage);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (healthFillGraphic != null && healthGradient != null && healthSlider != null)
        {
            healthFillGraphic.color = healthGradient.Evaluate(healthSlider.value);
        }
    }
#endif
}