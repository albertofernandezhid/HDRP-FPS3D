using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class UIPowerUpDisplay : MonoBehaviour
{
    [System.Serializable]
    private class PowerUpDisplay
    {
        public string powerUpName;
        public Image icon;
        public Slider timerSlider;
        public GameObject container;
        public RectTransform rectTransform;
        public float duration;
        public float timeRemaining;
    }

    [SerializeField] private Transform powerUpContainer;
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private float verticalSpacing = 65f;

    private Dictionary<string, PowerUpDisplay> activeDisplays = new();
    private List<string> displayOrder = new();

    private void Update()
    {
        UpdateAllTimers();
        RepositionDisplays();
    }

    private void UpdateAllTimers()
    {
        List<string> toRemove = new List<string>();

        foreach (var displayName in displayOrder.ToList())
        {
            if (activeDisplays.TryGetValue(displayName, out var display))
            {
                display.timeRemaining -= Time.deltaTime;

                if (display.timeRemaining <= 0)
                {
                    toRemove.Add(displayName);
                }
                else if (display.timerSlider != null && display.duration > 0)
                {
                    float normalizedValue = display.timeRemaining / display.duration;
                    display.timerSlider.value = Mathf.Clamp01(normalizedValue);
                }
            }
        }

        foreach (var powerUpName in toRemove)
        {
            RemovePowerUpInternal(powerUpName);
        }
    }

    private void RepositionDisplays()
    {
        float startX = 30f;
        float startY = -45f;
        float currentY = startY;

        foreach (var powerUpName in displayOrder)
        {
            if (activeDisplays.TryGetValue(powerUpName, out var display))
            {
                if (display.rectTransform != null)
                {
                    display.rectTransform.anchoredPosition = new Vector2(startX, currentY);
                    display.rectTransform.localPosition = new Vector3(display.rectTransform.localPosition.x, display.rectTransform.localPosition.y, 0f);
                    currentY -= verticalSpacing;
                }
            }
        }
    }

    public void ShowPowerUp(PowerUpData data)
    {
        if (data == null || activeDisplays.ContainsKey(data.powerUpName))
            return;

        if (powerUpContainer == null || powerUpPrefab == null)
            return;

        GameObject displayObj = Instantiate(powerUpPrefab, powerUpContainer);
        displayObj.transform.localScale = Vector3.one;

        Transform iconTransform = displayObj.transform.Find("Icon");
        Transform timerTransform = displayObj.transform.Find("Timer");

        if (iconTransform == null || timerTransform == null)
        {
            Destroy(displayObj);
            return;
        }

        Image icon = iconTransform.GetComponent<Image>();
        Slider slider = timerTransform.GetComponent<Slider>();
        RectTransform rect = displayObj.GetComponent<RectTransform>();

        if (icon == null || slider == null || rect == null)
        {
            Destroy(displayObj);
            return;
        }

        PowerUpDisplay display = new PowerUpDisplay
        {
            powerUpName = data.powerUpName,
            container = displayObj,
            icon = icon,
            timerSlider = slider,
            rectTransform = rect,
            duration = data.duration,
            timeRemaining = data.duration
        };

        display.icon.sprite = data.icon;
        display.icon.color = Color.white;

        slider.maxValue = 1f;
        slider.minValue = 0f;
        slider.value = 1f;

        activeDisplays.Add(data.powerUpName, display);
        displayOrder.Add(data.powerUpName);

        RepositionDisplays();
    }

    private void RemovePowerUpInternal(string powerUpName)
    {
        if (activeDisplays.TryGetValue(powerUpName, out var display))
        {
            if (display.container != null)
                Destroy(display.container);

            activeDisplays.Remove(powerUpName);
            displayOrder.Remove(powerUpName);
        }
    }

    public void HidePowerUp(string powerUpName)
    {
        RemovePowerUpInternal(powerUpName);
        RepositionDisplays();
    }

    public void ClearAll()
    {
        foreach (var display in activeDisplays.Values)
        {
            if (display.container != null)
                Destroy(display.container);
        }

        activeDisplays.Clear();
        displayOrder.Clear();
    }

    public void ResetPowerUpTimer(string powerUpName, float duration)
    {
        if (activeDisplays.TryGetValue(powerUpName, out var display))
        {
            display.duration = duration;
            display.timeRemaining = duration;
            display.timerSlider.value = 1f;
        }
    }

    public void UpdatePowerUpTimer(string powerUpName, float timeRemaining, float totalDuration)
    {
        if (activeDisplays.TryGetValue(powerUpName, out var display))
        {
            display.timeRemaining = timeRemaining;

            if (display.timerSlider != null && totalDuration > 0)
            {
                float normalizedValue = timeRemaining / totalDuration;
                display.timerSlider.value = Mathf.Clamp01(normalizedValue);
            }
        }
    }
}