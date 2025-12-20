using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "Game/PowerUps/PowerUp Data")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName = "PowerUp";
    public Sprite icon;
    public float duration = 10f;

    [Header("Effects")]
    public float speedMultiplier = 1f;
    public float jumpMultiplier = 1f;
    public float staminaRegenMultiplier = 1f;
    public float staminaBonus = 0f;
    public int healthBonus = 0;

    [Header("Visuals")]
    public Color pickupColor = Color.clear;
    public GameObject pickupPrefab;
    public ParticleSystem pickupParticles;
    public AudioClip pickupSound;
}