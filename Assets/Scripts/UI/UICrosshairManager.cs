using UnityEngine;
using UnityEngine.UI;

public class UICrosshairManager : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private float normalCrosshairSize = 24f;
    [SerializeField] private float aimingCrosshairSize = 36f;
    [SerializeField] private float crosshairChangeSpeed = 8f;

    [Header("References")]
    [SerializeField] private CameraController cameraController;

    private RectTransform crosshairRect;
    private float currentCrosshairSize;
    private float targetCrosshairSize;

    private void Start()
    {
        if (crosshairImage != null)
        {
            crosshairRect = crosshairImage.GetComponent<RectTransform>();
            currentCrosshairSize = normalCrosshairSize;
            targetCrosshairSize = normalCrosshairSize;
        }
    }

    private void Update()
    {
        UpdateCrosshair();
    }

    private void UpdateCrosshair()
    {
        if (crosshairRect == null || cameraController == null)
            return;

        bool isAiming = cameraController.IsAiming;
        targetCrosshairSize = isAiming ? aimingCrosshairSize : normalCrosshairSize;

        currentCrosshairSize = Mathf.Lerp(currentCrosshairSize, targetCrosshairSize, Time.deltaTime * crosshairChangeSpeed);

        crosshairRect.sizeDelta = new Vector2(currentCrosshairSize, currentCrosshairSize);
    }

    public void SetCrosshairVisibility(bool visible)
    {
        if (crosshairImage != null)
            crosshairImage.enabled = visible;
    }
}