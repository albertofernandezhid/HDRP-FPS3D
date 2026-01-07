using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class InputDeviceDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private string keyboardMessage = "Keyboard Input";
    [SerializeField] private string gamepadMessage = "Gamepad Input";

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    // Usamos un bool nullable para saber si es la primera vez que detectamos algo
    private bool? isUsingGamepad = null;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

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
        if (change != InputActionChange.ActionPerformed) return;

        InputAction action = (InputAction)obj;
        InputDevice device = action.activeControl.device;

        bool currentlyUsingGamepad = device is Gamepad;

        // PRIMERA DETECCIÓN: Solo guardamos el estado sin mostrar el panel
        if (isUsingGamepad == null)
        {
            isUsingGamepad = currentlyUsingGamepad;
            return;
        }

        // DETECCIONES POSTERIORES: Solo si el dispositivo ha cambiado
        if (currentlyUsingGamepad != isUsingGamepad)
        {
            isUsingGamepad = currentlyUsingGamepad;
            ShowPanel(isUsingGamepad.Value ? gamepadMessage : keyboardMessage);
        }
    }

    private void ShowPanel(string message)
    {
        statusText.text = message;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        canvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(displayDuration);

        float elapsed = 0f;
        float fadeTime = 0.5f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}