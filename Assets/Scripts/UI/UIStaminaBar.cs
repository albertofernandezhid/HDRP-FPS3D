using UnityEngine;
using UnityEngine.UI;

public class UIStaminaBar : MonoBehaviour, IStaminaObserver
{
    public Slider slider;

    private void Start()
    {
        Object.FindFirstObjectByType<StaminaSystem>().RegisterObserver(this);
    }

    public void OnStaminaChanged(float current, float max)
    {
        slider.value = current / max;
    }

    public void OnStaminaEmpty()
    {
    }
}