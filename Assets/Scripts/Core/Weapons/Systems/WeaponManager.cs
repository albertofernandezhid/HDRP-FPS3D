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
    [SerializeField] private PlayerAnimationController animationController;

    [Header("Settings")]
    [SerializeField] private float throwCooldown = 0.5f;
    [SerializeField] private float maxThrowDistance = 50f;
    [SerializeField] private float minSpawnDistance = 0.8f;
    [SerializeField] private LayerMask throwLayerMask = ~0;

    [Header("Trajectory Predictor")]
    [SerializeField] private bool showTrajectory = true;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.1f;
    [SerializeField] private Color trajectoryColor = Color.yellow;

    private int currentWeaponIndex = -1;
    private float lastThrowTime;
    private GameObject currentWeaponModel;
    private bool isReloadingModel = false;
    private PlayerInputActions inputActions;

    public WeaponInventory Inventory => inventory;
    public int CurrentWeaponIndex => currentWeaponIndex;
    public bool IsArmed => currentWeaponIndex >= 0;
    public GameObject CurrentWeapon => currentWeaponModel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.NextWeapon.performed += _ => NextWeapon();
        inputActions.Player.PreviousWeapon.performed += _ => PreviousWeapon();
    }

    private void OnDisable()
    {
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Disable();
    }

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (inventory == null) inventory = GetComponent<WeaponInventory>();
        inventory.OnInventoryChanged += OnInventoryChanged;
        SetupTrajectoryLine();
        UpdateWeaponModel();
        if (animationController == null) animationController = GetComponent<PlayerAnimationController>();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.OnInventoryChanged -= OnInventoryChanged;
    }

    public void SetWeaponHoldPoint(Transform newPoint)
    {
        weaponHoldPoint = newPoint;
        UpdateWeaponModel();
    }

    private void SetupTrajectoryLine()
    {
        if (trajectoryLine == null)
        {
            GameObject lineObj = new GameObject("TrajectoryLine");
            lineObj.transform.SetParent(transform);
            trajectoryLine = lineObj.AddComponent<LineRenderer>();
        }
        trajectoryLine.startWidth = 0.05f;
        trajectoryLine.endWidth = 0.05f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = new Color(trajectoryColor.r, trajectoryColor.g, trajectoryColor.b, 0.3f);
        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.enabled = false;
        trajectoryLine.useWorldSpace = true;
    }

    private void OnInventoryChanged()
    {
        int count = inventory.Slots.Count;
        if (count == 0) currentWeaponIndex = -1;
        else if (currentWeaponIndex >= count) currentWeaponIndex = count - 1;
        UpdateWeaponModel();
    }

    private void Update()
    {
        HandleModelReload();
        UpdateTrajectoryPredictor();
    }

    public bool CanThrow()
    {
        return IsArmed && Time.time >= lastThrowTime + throwCooldown;
    }

    public void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (CanThrow())
        {
            lastThrowTime = Time.time;
            isReloadingModel = true;
            if (animationController != null)
            {
                animationController.TriggerThrow();
            }
        }
    }

    public void ExecuteThrow()
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= inventory.Slots.Count) return;
        InventorySlot slot = inventory.Slots[currentWeaponIndex];
        if (!slot.CanUse()) return;
        Vector3 targetPoint = GetAimTarget();
        Vector3 throwVelocity = CalculateThrowVelocity(weaponHoldPoint.position, targetPoint, slot.weapon.ThrowForce);
        Vector3 spawnDirection = throwVelocity.normalized;
        Vector3 throwPosition = weaponHoldPoint.position + spawnDirection * minSpawnDistance;
        if (Physics.Raycast(weaponHoldPoint.position, spawnDirection, out RaycastHit spawnCheck, minSpawnDistance, throwLayerMask))
        {
            throwPosition = weaponHoldPoint.position + spawnDirection * (spawnCheck.distance * 0.5f);
        }
        GameObject thrownProjectile = Instantiate(slot.weapon.Prefab, throwPosition, Quaternion.LookRotation(spawnDirection));
        SetupThrownWeapon(thrownProjectile, throwVelocity, slot);
        inventory.UseAmmo(currentWeaponIndex);
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
            currentWeaponModel = null;
        }
        trajectoryLine.enabled = false;
    }

    private void UpdateTrajectoryPredictor()
    {
        if (!showTrajectory || !IsArmed || trajectoryLine == null || isReloadingModel)
        {
            if (trajectoryLine != null) trajectoryLine.enabled = false;
            return;
        }
        if (currentWeaponIndex < 0 || currentWeaponIndex >= inventory.Slots.Count)
        {
            trajectoryLine.enabled = false;
            return;
        }
        InventorySlot slot = inventory.Slots[currentWeaponIndex];
        if (!slot.CanUse())
        {
            trajectoryLine.enabled = false;
            return;
        }
        Vector3 targetPoint = GetAimTarget();
        Vector3 throwVelocity = CalculateThrowVelocity(weaponHoldPoint.position, targetPoint, slot.weapon.ThrowForce);
        DrawTrajectory(weaponHoldPoint.position, throwVelocity);
        trajectoryLine.enabled = true;
    }

    private void DrawTrajectory(Vector3 origin, Vector3 velocity)
    {
        trajectoryLine.positionCount = trajectoryPoints;
        Vector3 position = origin;
        Vector3 currentVelocity = velocity;
        float timeStep = trajectoryTimeStep;
        int pointsDrawn = 0;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            trajectoryLine.SetPosition(i, position);
            pointsDrawn = i + 1;
            currentVelocity += Physics.gravity * timeStep;
            Vector3 nextPosition = position + currentVelocity * timeStep;
            if (Physics.Linecast(position, nextPosition, out RaycastHit hit, throwLayerMask))
            {
                if (pointsDrawn < trajectoryLine.positionCount)
                {
                    trajectoryLine.SetPosition(pointsDrawn, hit.point);
                    pointsDrawn++;
                }
                trajectoryLine.positionCount = pointsDrawn;
                return;
            }
            position = nextPosition;
            if (position.y < origin.y - 20f)
            {
                trajectoryLine.positionCount = pointsDrawn;
                return;
            }
        }
        trajectoryLine.positionCount = pointsDrawn;
    }

    private void HandleModelReload()
    {
        if (isReloadingModel && Time.time >= lastThrowTime + throwCooldown)
        {
            isReloadingModel = false;
            if (currentWeaponIndex >= 0 && currentWeaponIndex < inventory.Slots.Count)
            {
                InventorySlot slot = inventory.Slots[currentWeaponIndex];
                if (slot.CanUse()) UpdateWeaponModel();
            }
        }
    }

    public void SelectWeapon(int slotIndex)
    {
        if (inventory.Slots.Count == 0) return;
        currentWeaponIndex = Mathf.Clamp(slotIndex, 0, inventory.Slots.Count - 1);
        isReloadingModel = false;
        UpdateWeaponModel();
    }

    public void NextWeapon()
    {
        int count = inventory.Slots.Count;
        if (count <= 1) return;
        currentWeaponIndex = (currentWeaponIndex + 1) % count;
        isReloadingModel = false;
        UpdateWeaponModel();
    }

    public void PreviousWeapon()
    {
        int count = inventory.Slots.Count;
        if (count <= 1) return;
        currentWeaponIndex--;
        if (currentWeaponIndex < 0) currentWeaponIndex = count - 1;
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
        else if (emptyHandModel != null && weaponHoldPoint != null)
        {
            currentWeaponModel = Instantiate(emptyHandModel, weaponHoldPoint);
            currentWeaponModel.transform.localPosition = Vector3.zero;
            currentWeaponModel.transform.localRotation = Quaternion.identity;
        }
    }

    private void SetupWeaponInHand(GameObject weaponObject)
    {
        Rigidbody rb = weaponObject.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.detectCollisions = false; }
        Collider col = weaponObject.GetComponent<Collider>();
        if (col != null) { col.enabled = false; col.isTrigger = false; }
        ProjectileController proj = weaponObject.GetComponent<ProjectileController>();
        if (proj != null) proj.enabled = false;
        PickupItem pickup = weaponObject.GetComponent<PickupItem>();
        if (pickup != null) Destroy(pickup);
    }

    private Vector3 GetAimTarget()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxThrowDistance, throwLayerMask)) return hit.point;
        return ray.GetPoint(maxThrowDistance);
    }

    private Vector3 CalculateThrowVelocity(Vector3 origin, Vector3 target, float throwForce)
    {
        Vector3 toTarget = target - origin;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
        float horizontalDistance = toTargetXZ.magnitude;
        float verticalDistance = toTarget.y;
        float gravity = Mathf.Abs(Physics.gravity.y);
        float speed = throwForce;
        float speedSquared = speed * speed;
        float underRoot = speedSquared * speedSquared - gravity * (gravity * horizontalDistance * horizontalDistance + 2 * verticalDistance * speedSquared);
        if (underRoot < 0) return toTarget.normalized * speed;
        float root = Mathf.Sqrt(underRoot);
        float angle1 = Mathf.Atan((speedSquared - root) / (gravity * horizontalDistance));
        Vector3 horizontalDirection = toTargetXZ.normalized;
        return horizontalDirection * Mathf.Cos(angle1) * speed + Vector3.up * Mathf.Sin(angle1) * speed;
    }

    private void SetupThrownWeapon(GameObject projectile, Vector3 velocity, InventorySlot slot)
    {
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.linearVelocity = velocity;
        }
        Collider col = projectile.GetComponent<Collider>();
        if (col != null) { col.enabled = true; col.isTrigger = false; }
        ProjectileController proj = projectile.GetComponent<ProjectileController>();
        if (proj != null) { proj.enabled = true; proj.Initialize(slot.weapon.Damage); }
        PickupItem pickup = projectile.GetComponent<PickupItem>();
        if (pickup != null) Destroy(pickup);
    }

    public bool PickupWeapon(ThrowableData weaponData, int ammoAmount = 1)
    {
        bool success = inventory.AddWeapon(weaponData, ammoAmount);
        if (success)
        {
            weaponData.OnPickup();
            if (currentWeaponIndex == -1) { currentWeaponIndex = 0; UpdateWeaponModel(); }
        }
        return success;
    }
}