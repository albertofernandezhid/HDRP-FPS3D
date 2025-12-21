using UnityEngine;

public interface IThrowable
{
    string ItemName { get; }
    Sprite Icon { get; }
    GameObject Prefab { get; }
    float ThrowForce { get; }
    float Damage { get; }

    void OnThrow(Vector3 position, Quaternion rotation, Vector3 direction);
    void OnPickup();
}