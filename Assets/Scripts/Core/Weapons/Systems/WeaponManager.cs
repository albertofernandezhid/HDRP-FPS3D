using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private WeaponInventory inventory;
    [SerializeField] private GameObject emptyHandModel;

    [Header("Settings")]
    [SerializeField] private float throwCooldown = 0.5f;
    [SerializeField] private float throwArcHeight = 0.3f;

    private int currentWeaponIndex = -1;
    private float lastThrowTime;
    private GameObject currentWeaponModel;
    private bool isReloadingModel = false;

    public WeaponInventory Inventory => inventory;
    public int CurrentWeaponIndex => currentWeaponIndex;
    public bool IsArmed => currentWeaponIndex >= 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (inventory == null)
            inventory = GetComponent<WeaponInventory>();

        inventory.OnInventoryChanged += OnInventoryChanged;
        UpdateWeaponModel();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged()
    {
        if (currentWeaponIndex >= inventory.Slots.Count)
        {
            currentWeaponIndex = -1;
            isReloadingModel = false;
            UpdateWeaponModel();
        }
    }

    private void Update()
    {
        HandleInput();
        HandleModelReload();
    }

    private void HandleModelReload()
    {
        if (isReloadingModel && Time.time >= lastThrowTime + throwCooldown)
        {
            isReloadingModel = false;
            if (currentWeaponIndex >= 0 && currentWeaponIndex < inventory.Slots.Count)
            {
                InventorySlot slot = inventory.Slots[currentWeaponIndex];
                if (slot.CanUse())
                {
                    UpdateWeaponModel();
                }
            }
        }
    }

    private void HandleInput()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectWeapon(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectWeapon(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectWeapon(2);

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0)
            NextWeapon();
        else if (scroll < 0)
            PreviousWeapon();

        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.left.wasPressedThisFrame)
                PreviousWeapon();
            else if (Gamepad.current.dpad.right.wasPressedThisFrame)
                NextWeapon();

            if (Gamepad.current.buttonNorth.wasPressedThisFrame)
                ToggleUnarmed();
        }

        bool shootInput = Mouse.current.leftButton.wasPressedThisFrame ||
                         (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame);

        if (shootInput && IsArmed && Time.time >= lastThrowTime + throwCooldown)
        {
            ThrowCurrentWeapon();
        }
    }

    public void SelectWeapon(int slotIndex)
    {
        if (slotIndex < inventory.Slots.Count)
        {
            if (slotIndex == currentWeaponIndex)
            {
                currentWeaponIndex = -1;
            }
            else
            {
                currentWeaponIndex = slotIndex;
            }
        }
        else
        {
            currentWeaponIndex = -1;
        }

        isReloadingModel = false;
        UpdateWeaponModel();
    }

    public void NextWeapon()
    {
        if (inventory.Slots.Count == 0)
        {
            currentWeaponIndex = -1;
        }
        else
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % (inventory.Slots.Count + 1);
            if (currentWeaponIndex == inventory.Slots.Count)
                currentWeaponIndex = -1;
        }

        isReloadingModel = false;
        UpdateWeaponModel();
    }

    public void PreviousWeapon()
    {
        if (inventory.Slots.Count == 0)
        {
            currentWeaponIndex = -1;
        }
        else
        {
            currentWeaponIndex = (currentWeaponIndex - 1 + inventory.Slots.Count + 1) % (inventory.Slots.Count + 1);
            if (currentWeaponIndex == inventory.Slots.Count)
                currentWeaponIndex = inventory.Slots.Count - 1;
        }

        isReloadingModel = false;
        UpdateWeaponModel();
    }

    public void ToggleUnarmed()
    {
        currentWeaponIndex = currentWeaponIndex >= 0 ? -1 : 0;
        isReloadingModel = false;
        UpdateWeaponModel();
    }

    private void UpdateWeaponModel()
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
            currentWeaponModel = null;
        }

        if (currentWeaponIndex >= 0 && currentWeaponIndex < inventory.Slots.Count)
        {
            ThrowableData weapon = inventory.Slots[currentWeaponIndex].weapon;
            if (weapon != null && weapon.Prefab != null && weaponHoldPoint != null)
            {
                currentWeaponModel = Instantiate(weapon.Prefab, weaponHoldPoint);
                currentWeaponModel.transform.localPosition = Vector3.zero;
                currentWeaponModel.transform.localRotation = Quaternion.identity;

                SetupWeaponInHand(currentWeaponModel);
            }
        }
        else
        {
            if (emptyHandModel != null && weaponHoldPoint != null)
            {
                currentWeaponModel = Instantiate(emptyHandModel, weaponHoldPoint);
                currentWeaponModel.transform.localPosition = Vector3.zero;
                currentWeaponModel.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private void SetupWeaponInHand(GameObject weaponObject)
    {
        Rigidbody rb = weaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider col = weaponObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            col.isTrigger = false;
        }

        ProjectileController proj = weaponObject.GetComponent<ProjectileController>();
        if (proj != null)
            proj.enabled = false;

        PickupItem pickup = weaponObject.GetComponent<PickupItem>();
        if (pickup != null)
            Destroy(pickup);
    }

    private void ThrowCurrentWeapon()
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= inventory.Slots.Count)
            return;

        InventorySlot slot = inventory.Slots[currentWeaponIndex];
        if (!slot.CanUse())
            return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100f))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(50f);

        Vector3 direction = (targetPoint - weaponHoldPoint.position).normalized;
        Vector3 throwPosition = weaponHoldPoint.position + direction * 0.5f;

        GameObject thrownProjectile = Instantiate(
            slot.weapon.Prefab,
            throwPosition,
            Quaternion.LookRotation(direction)
        );

        SetupThrownWeapon(thrownProjectile, direction, slot);

        inventory.UseAmmo(currentWeaponIndex);
        lastThrowTime = Time.time;

        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
            currentWeaponModel = null;
        }

        isReloadingModel = true;
    }

    private void SetupThrownWeapon(GameObject projectile, Vector3 direction, InventorySlot slot)
    {
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            Vector3 forceDirection = direction + (Vector3.up * throwArcHeight);
            rb.AddForce(forceDirection * slot.weapon.ThrowForce, ForceMode.Impulse);
        }

        Collider col = projectile.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        ProjectileController proj = projectile.GetComponent<ProjectileController>();
        if (proj != null)
        {
            proj.enabled = true;
            proj.Initialize(slot.weapon.Damage);
        }

        PickupItem pickup = projectile.GetComponent<PickupItem>();
        if (pickup != null)
            Destroy(pickup);
    }

    public bool PickupWeapon(ThrowableData weaponData, int ammoAmount = 1)
    {
        bool success = inventory.AddWeapon(weaponData, ammoAmount);
        if (success)
            weaponData.OnPickup();

        if (success && currentWeaponIndex < 0)
        {
            currentWeaponIndex = inventory.Slots.Count - 1;
            UpdateWeaponModel();
        }

        return success;
    }
}