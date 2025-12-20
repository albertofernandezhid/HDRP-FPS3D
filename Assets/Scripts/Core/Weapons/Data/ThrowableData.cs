using UnityEngine;

public abstract class ThrowableData : ScriptableObject, IThrowable
{
    [Header("Base Settings")]
    [SerializeField] protected string itemName = "Item";
    [SerializeField] protected Sprite icon;
    [SerializeField] protected GameObject prefab;
    [SerializeField] protected float throwForce = 10f;
    [SerializeField] protected float damage = 10f;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
    public float ThrowForce => throwForce;
    public float Damage => damage;

    public abstract void OnThrow(Vector3 position, Quaternion rotation, Vector3 direction);
    public virtual void OnPickup() { }
}