using UnityEngine;

public class ItemLightController : MonoBehaviour
{
    [SerializeField] private string projectileTag = "Projectile";
    private Light childLight;

    void Awake()
    {
        childLight = GetComponentInChildren<Light>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(projectileTag))
        {
            DisableLight();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(projectileTag))
        {
            DisableLight();
        }
    }

    private void DisableLight()
    {
        if (childLight != null)
        {
            childLight.enabled = false;
        }
    }
}