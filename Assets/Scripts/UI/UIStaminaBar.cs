using UnityEngine;
using UnityEngine.UI;

public class UIStaminaBar : MonoBehaviour, IStaminaObserver
{
    [SerializeField] private Slider slider;
    [SerializeField] private Graphic staminaFillGraphic;
    [SerializeField] private Text staminaText;

    private void Start()
    {
        InitializeStaminaBar();
        FindStaminaSystem();
    }

    private void InitializeStaminaBar()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (staminaFillGraphic == null && slider != null && slider.fillRect != null)
            staminaFillGraphic = slider.fillRect.GetComponent<Graphic>();

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
        }
    }

    private void FindStaminaSystem()
    {
        StaminaSystem system = Object.FindFirstObjectByType<StaminaSystem>();
        if (system != null)
        {
            system.RegisterObserver(this);
            system.NotifyManual();
        }
        else
        {
            Invoke(nameof(FindStaminaSystem), 0.5f);
        }
    }

    public void OnStaminaChanged(float current, float max)
    {
        if (slider != null)
        {
            slider.value = current / max;
        }

        if (staminaText != null)
        {
            staminaText.text = $"{current:F0}/{max:F0}";
        }
    }

    public void OnStaminaEmpty()
    {
    }

    private void OnDestroy()
    {
        StaminaSystem system = Object.FindFirstObjectByType<StaminaSystem>();
        if (system != null)
        {
            system.UnregisterObserver(this);
        }
    }
}