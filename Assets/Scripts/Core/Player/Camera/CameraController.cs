using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject playerMesh;

    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 300f;
    [SerializeField] private float fpsFOV = 75f;
    [SerializeField] private float tpsFOV = 65f;
    [SerializeField] private float tpsDistance = 3f;
    [SerializeField] private float minZoomDistance = 1f;
    [SerializeField] private float maxZoomDistance = 10f;
    [SerializeField] private float zoomSpeed = 5f;
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

    private float xRotation = 0f;
    private float currentDistance;
    private bool isFirstPerson = false;
    private bool isAiming = false;
    private Vector3 cameraVelocity;
    private Renderer playerRenderer;
    private Vector3 originalPivotPosition;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentDistance = tpsDistance;
        originalPivotPosition = cameraPivot.localPosition;

        if (playerMesh != null)
            playerRenderer = playerMesh.GetComponent<Renderer>();

        SwitchCameraMode();
    }

    private void Update()
    {
        HandleMouseLook();
        HandleInput();

        if (isFirstPerson)
            UpdateFirstPerson();
        else
            UpdateThirdPerson();

        UpdateFieldOfView();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;

        float minAngle = isFirstPerson ? fpsMinVerticalAngle : tpsMinVerticalAngle;
        float maxAngle = isFirstPerson ? fpsMaxVerticalAngle : tpsMaxVerticalAngle;
        xRotation = Mathf.Clamp(xRotation, minAngle, maxAngle);

        playerRoot.Rotate(Vector3.up * mouseX);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            SwitchCameraMode();
        }

        if (Input.GetMouseButtonDown(1))
        {
            isAiming = !isAiming;
        }

        if (!isFirstPerson)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed, minZoomDistance, maxZoomDistance);
            }
        }
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
            targetFOV *= 0.7f;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 10f);
    }

    public Transform CameraPivot => cameraPivot;
}