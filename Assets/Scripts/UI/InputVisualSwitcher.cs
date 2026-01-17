using UnityEngine;
using UnityEngine.InputSystem;

public class InputVisualSwitcher : MonoBehaviour
{
    [Header("Visual Containers")]
    [Tooltip("Objeto que contiene la imagen de Teclado y su TMP")]
    [SerializeField] private GameObject keyboardRoot;

    [Tooltip("Objeto que contiene la imagen de Mando y su TMP")]
    [SerializeField] private GameObject gamepadRoot;

    private bool? isUsingGamepad = null;

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        // Solo actuamos cuando se realiza una acción (tecla pulsada o stick movido)
        if (change != InputActionChange.ActionPerformed) return;

        InputAction action = (InputAction)obj;

        // Verificamos si hay un control activo para evitar errores de referencia nula
        if (action.activeControl == null) return;

        bool currentlyUsingGamepad = action.activeControl.device is Gamepad;

        // Si es el primer input o el dispositivo ha cambiado, hacemos el swap
        if (isUsingGamepad == null || currentlyUsingGamepad != isUsingGamepad)
        {
            isUsingGamepad = currentlyUsingGamepad;
            ToggleVisuals(currentlyUsingGamepad);
        }
    }

    private void ToggleVisuals(bool useGamepad)
    {
        if (keyboardRoot != null)
            keyboardRoot.SetActive(!useGamepad);

        if (gamepadRoot != null)
            gamepadRoot.SetActive(useGamepad);
    }
}