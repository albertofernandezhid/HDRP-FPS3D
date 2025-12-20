using System.Collections.Generic;
using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    public float maxStamina = 100f;
    public float drainPerSecond = 25f;
    public float regenPerSecond = 15f;

    private float currentStamina;
    private readonly List<IStaminaObserver> observers = new();

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    public void RegisterObserver(IStaminaObserver observer)
    {
        if (!observers.Contains(observer))
            observers.Add(observer);
    }

    public void UnregisterObserver(IStaminaObserver observer)
    {
        observers.Remove(observer);
    }

    public void Consume(float deltaTime)
    {
        currentStamina -= drainPerSecond * deltaTime;
        currentStamina = Mathf.Max(currentStamina, 0f);
        Notify();

        if (currentStamina <= 0f)
            NotifyEmpty();
    }

    public void Regenerate(float deltaTime)
    {
        currentStamina += regenPerSecond * deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        Notify();
    }

    public void AddStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        Notify();
    }

    public void SetRegenerationMultiplier(float multiplier)
    {
        regenPerSecond *= multiplier;
    }

    public void ResetRegenerationMultiplier(float originalValue)
    {
        regenPerSecond = originalValue;
    }

    public bool HasStamina()
    {
        return currentStamina > 0f;
    }

    private void Notify()
    {
        foreach (var observer in observers)
            observer.OnStaminaChanged(currentStamina, maxStamina);
    }

    private void NotifyEmpty()
    {
        foreach (var observer in observers)
            observer.OnStaminaEmpty();
    }
}