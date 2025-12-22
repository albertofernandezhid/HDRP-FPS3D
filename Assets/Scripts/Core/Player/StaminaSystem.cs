using System.Collections.Generic;
using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    [Header("Base Settings")]
    public float maxStamina = 100f;
    public float drainPerSecond = 25f;
    public float regenPerSecond = 15f;

    [Header("Threshold Settings")]
    [Range(0f, 1f)] public float minStaminaPercentage = 0.1f;

    [Header("Multipliers (Consumption)")]
    [Range(0f, 2f)] public float sprintDrainMultiplier = 1f;
    [Range(0f, 2f)] public float runDrainMultiplier = 0.5f;

    [Header("Multipliers (Regeneration)")]
    [Range(0f, 2f)] public float idleRegenMultiplier = 1f;
    [Range(0f, 2f)] public float walkRegenMultiplier = 0.5f;

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

    public void Consume(float deltaTime, float multiplier)
    {
        currentStamina -= drainPerSecond * multiplier * deltaTime;
        currentStamina = Mathf.Max(currentStamina, 0f);
        Notify();

        if (currentStamina <= 0f)
            NotifyEmpty();
    }

    public void Regenerate(float deltaTime, float multiplier)
    {
        currentStamina += regenPerSecond * multiplier * deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        Notify();
    }

    public void AddStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        Notify();
    }

    public bool HasStamina()
    {
        return currentStamina > 0.1f;
    }

    public bool CanEnterStaminaState()
    {
        return currentStamina >= maxStamina * minStaminaPercentage;
    }

    private void Notify()
    {
        for (int i = observers.Count - 1; i >= 0; i--)
            observers[i].OnStaminaChanged(currentStamina, maxStamina);
    }

    public void NotifyManual()
    {
        Notify();
    }

    private void NotifyEmpty()
    {
        for (int i = observers.Count - 1; i >= 0; i--)
            observers[i].OnStaminaEmpty();
    }
}