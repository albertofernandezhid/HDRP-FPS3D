using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private UIPowerUpDisplay uiDisplay;

    private Dictionary<string, PowerUpInstance> activePowerUps = new();

    private PlayerController playerController;
    private StaminaSystem staminaSystem;
    private PlayerHealth playerHealth;

    [System.Serializable]
    private class PowerUpInstance
    {
        public PowerUpData data;
        public Coroutine timerCoroutine;
        public Action onExpire;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FindPlayerReferences();
    }

    private void FindPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            staminaSystem = player.GetComponent<StaminaSystem>();
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    public bool ApplyPowerUp(PowerUpData data, GameObject target)
    {
        if (playerController == null || staminaSystem == null || playerHealth == null)
        {
            FindPlayerReferences();
            if (playerController == null) return false;
        }

        if (activePowerUps.ContainsKey(data.powerUpName))
        {
            ResetPowerUp(data.powerUpName, data);
            return true;
        }

        ApplyImmediateEffects(data);

        Action onExpire = ApplyTemporaryEffects(data);

        PowerUpInstance instance = new PowerUpInstance
        {
            data = data,
            onExpire = onExpire,
            timerCoroutine = StartCoroutine(PowerUpTimer(data.powerUpName, data.duration))
        };

        activePowerUps.Add(data.powerUpName, instance);

        if (uiDisplay != null)
        {
            uiDisplay.ShowPowerUp(data);
        }

        return true;
    }

    private void ResetPowerUp(string powerUpName, PowerUpData data)
    {
        if (activePowerUps.TryGetValue(powerUpName, out var instance))
        {
            if (instance.timerCoroutine != null)
                StopCoroutine(instance.timerCoroutine);

            instance.timerCoroutine = StartCoroutine(PowerUpTimer(powerUpName, data.duration));

            if (uiDisplay != null)
            {
                uiDisplay.ResetPowerUpTimer(powerUpName, data.duration);
            }
        }
    }

    private void ApplyImmediateEffects(PowerUpData data)
    {
        if (data.staminaBonus > 0 && staminaSystem != null)
        {
            staminaSystem.AddStamina(data.staminaBonus);
        }

        if (data.healthBonus > 0 && playerHealth != null)
        {
            playerHealth.Heal(data.healthBonus);
        }
    }

    private Action ApplyTemporaryEffects(PowerUpData data)
    {
        List<Action> revertActions = new List<Action>();

        if (data.speedMultiplier != 1f && playerController != null)
        {
            float originalWalk = playerController.walkSpeed;
            float originalRun = playerController.runSpeed;
            float originalSprint = playerController.sprintSpeed;

            playerController.walkSpeed *= data.speedMultiplier;
            playerController.runSpeed *= data.speedMultiplier;
            playerController.sprintSpeed *= data.speedMultiplier;

            revertActions.Add(() =>
            {
                playerController.walkSpeed = originalWalk;
                playerController.runSpeed = originalRun;
                playerController.sprintSpeed = originalSprint;
            });
        }

        if (data.staminaRegenMultiplier != 1f && staminaSystem != null)
        {
            float originalRegen = staminaSystem.regenPerSecond;
            staminaSystem.regenPerSecond *= data.staminaRegenMultiplier;

            revertActions.Add(() =>
            {
                staminaSystem.regenPerSecond = originalRegen;
            });
        }

        if (data.jumpMultiplier != 1f && playerController != null)
        {
            float originalJump = playerController.jumpHeight;
            playerController.jumpHeight *= data.jumpMultiplier;

            revertActions.Add(() =>
            {
                playerController.jumpHeight = originalJump;
            });
        }

        return () =>
        {
            foreach (var action in revertActions)
            {
                action();
            }
        };
    }

    private IEnumerator PowerUpTimer(string powerUpName, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemovePowerUp(powerUpName);
    }

    private void RemovePowerUp(string powerUpName)
    {
        if (activePowerUps.TryGetValue(powerUpName, out var instance))
        {
            instance.onExpire?.Invoke();

            if (instance.timerCoroutine != null)
                StopCoroutine(instance.timerCoroutine);

            if (uiDisplay != null)
            {
                uiDisplay.HidePowerUp(powerUpName);
            }

            activePowerUps.Remove(powerUpName);
        }
    }

    public void ClearAllPowerUps()
    {
        foreach (var kvp in activePowerUps)
        {
            if (kvp.Value.timerCoroutine != null)
                StopCoroutine(kvp.Value.timerCoroutine);

            kvp.Value.onExpire?.Invoke();
        }

        activePowerUps.Clear();

        if (uiDisplay != null)
        {
            uiDisplay.ClearAll();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            ClearAllPowerUps();
            Instance = null;
        }
    }
}