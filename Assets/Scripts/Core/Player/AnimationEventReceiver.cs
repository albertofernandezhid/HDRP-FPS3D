using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private WeaponManager weaponManager;
    private PlayerAnimationController animationController;
    private PlayerController playerController;

    private void Awake()
    {
        weaponManager = GetComponentInParent<WeaponManager>();
        playerController = GetComponentInParent<PlayerController>();
        animationController = GetComponent<PlayerAnimationController>();
        if (animationController == null)
        {
            animationController = GetComponentInParent<PlayerAnimationController>();
        }
    }

    public void AnimationEvent_ShootProjectile()
    {
        if (weaponManager != null)
        {
            weaponManager.ExecuteThrow();
        }

        if (playerController != null)
        {
            playerController.TriggerVibration(playerController.vibrationSettings.attackDuration, playerController.vibrationSettings.attackLowFreq, playerController.vibrationSettings.attackHighFreq);
        }

        if (animationController != null)
        {
            animationController.PlayRandomSound(animationController.AttackSounds, 1f);
        }
    }

    public void AnimationEvent_Footstep()
    {
        if (animationController != null)
        {
            animationController.PlayRandomSound(animationController.FootstepSounds, 0.4f);
        }
    }
}