using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject playerMesh;

    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float gamepadSensitivity = 100f;
    [SerializeField] private float fpsFOV = 75f;
    [SerializeField] private float tpsFOV = 65f;
    [SerializeField] private float tpsDistance = 3f;
    [SerializeField] private float minZoomDistance = 1f;
    [SerializeField] private float maxZoomDistance = 10f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float collisionOffset = 0.3f;
    [SerializeField] private LayerMask collisionLayers = ~0;

    [Header("FPS Limits")]
    [SerializeField] private float fpsMinVerticalAngle = -26f;
    [SerializeField] private float fpsMaxVerticalAngle = 26f;

    [Header("TPS Limits (Minecraft Style)")]
    [SerializeField] private float tpsMinVerticalAngle = -60f;
    [SerializeField] private float tpsMaxVerticalAngle = 80f;

    [Header("First Person Options")]
    [SerializeField] private bool hideMeshInFPS = true;
    [SerializeField] private float fpsVerticalOffset = 0.6f;
    [SerializeField] private bool startInFirstPerson = true;

    [Header("Aiming Settings")]
    [SerializeField] private float aimingZoomMultiplier = 0.7f;
    [SerializeField] private float aimingDistanceReduction = 1.5f;
    [SerializeField] private float aimingSmoothTime = 0.15f;
    [SerializeField] private bool toggleAimMode = true;

    private float xRotation = 0f;
    private float currentDistance;
    private float targetDistance;
    private bool isFirstPerson = false;
    private bool isAiming = false;
    private Vector3 cameraVelocity;
    private Renderer playerRenderer;
    private Vector3 originalPivotPosition;

    public float MouseSens => mouseSensitivity;
    public float GamepadSens => gamepadSensitivity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentDistance = tpsDistance;
        targetDistance = tpsDistance;
        originalPivotPosition = cameraPivot.localPosition;

        if (playerMesh != null)
            playerRenderer = playerMesh.GetComponent<Renderer>();

        isFirstPerson = startInFirstPerson;
        SwitchCameraMode();
    }

    private void Update()
    {
        UpdateCameraDistance();

        if (isFirstPerson)
            UpdateFirstPerson();
        else
            UpdateThirdPerson();

        UpdateFieldOfView();
    }

    public void HandleMouseLook(float mouseX, float mouseY, bool isMouse)
    {
        float multiplier = isMouse ? 1f : Time.deltaTime;

        xRotation -= mouseY * multiplier;
        xRotation = Mathf.Clamp(xRotation, isFirstPerson ? fpsMinVerticalAngle : tpsMinVerticalAngle, isFirstPerson ? fpsMaxVerticalAngle : tpsMaxVerticalAngle);

        playerRoot.Rotate(Vector3.up * mouseX * multiplier);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    public void SetAiming(bool state)
    {
        if (toggleAimMode)
        {
            if (state) isAiming = !isAiming;
        }
        else
        {
            isAiming = state;
        }
    }

    public void HandleZoom(float zoomAmount)
    {
        if (!isFirstPerson && !isAiming)
        {
            targetDistance = Mathf.Clamp(targetDistance - zoomAmount * zoomSpeed, minZoomDistance, maxZoomDistance);
        }
    }

    private void UpdateCameraDistance()
    {
        if (isAiming && !isFirstPerson)
        {
            float aimingDistance = Mathf.Max(minZoomDistance, tpsDistance - aimingDistanceReduction);
            targetDistance = aimingDistance;
        }
        else if (!isFirstPerson && !isAiming)
        {
            targetDistance = tpsDistance;
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime / aimingSmoothTime);
    }

    public void ToggleCameraMode()
    {
        isFirstPerson = !isFirstPerson;
        SwitchCameraMode();
    }

    private void SwitchCameraMode()
    {
        if (isFirstPerson)
        {
            cameraPivot.localPosition = originalPivotPosition + new Vector3(0, fpsVerticalOffset, 0);

            if (hideMeshInFPS && playerRenderer != null)
                playerRenderer.enabled = false;
        }
        else
        {
            cameraPivot.localPosition = originalPivotPosition;

            if (hideMeshInFPS && playerRenderer != null)
                playerRenderer.enabled = true;
        }
    }

    private void UpdateFirstPerson()
    {
        playerCamera.transform.localPosition = Vector3.zero;
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void UpdateThirdPerson()
    {
        UpdateThirdPersonPosition();

        Vector3 lookTarget = cameraPivot.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - playerCamera.transform.position);
        playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    private void UpdateThirdPersonPosition()
    {
        Vector3 desiredPosition = cameraPivot.position - cameraPivot.forward * currentDistance;

        RaycastHit hit;
        if (Physics.Linecast(cameraPivot.position, desiredPosition, out hit, collisionLayers))
        {
            desiredPosition = hit.point + hit.normal * collisionOffset;
        }
        else
        {
            RaycastHit hitFromDesired;
            if (Physics.Linecast(desiredPosition, cameraPivot.position, out hitFromDesired, collisionLayers))
            {
                desiredPosition = hitFromDesired.point + hitFromDesired.normal * collisionOffset;
            }
        }

        playerCamera.transform.position = Vector3.SmoothDamp(
            playerCamera.transform.position,
            desiredPosition,
            ref cameraVelocity,
            0.1f
        );
    }

    private void UpdateFieldOfView()
    {
        float targetFOV = isFirstPerson ? fpsFOV : tpsFOV;
        if (isAiming)
            targetFOV *= aimingZoomMultiplier;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 10f);
    }

    public Transform CameraPivot => cameraPivot;
    public bool IsAiming => isAiming;
}