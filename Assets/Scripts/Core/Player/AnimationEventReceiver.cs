using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private WeaponManager weaponManager;

    private void Awake()
    {
        weaponManager = GetComponentInParent<WeaponManager>();
    }

    public void AnimationEvent_ShootProjectile()
    {
        if (weaponManager != null)
        {
            weaponManager.ExecuteThrow();
        }
    }
}