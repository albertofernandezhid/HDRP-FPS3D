using UnityEngine;

public class ItemLightController : MonoBehaviour
{
    [SerializeField] private string projectileTag = "Projectile";
    [SerializeField] private string itemTag = "Item";
    private Light childLight;

    void Awake()
    {
        childLight = GetComponentInChildren<Light>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(projectileTag) || other.CompareTag(itemTag))
        {
            DisableLight();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(projectileTag) || collision.gameObject.CompareTag(itemTag))
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