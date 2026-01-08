using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Localization;

public class InputDeviceDetector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private float displayDuration = 2f;

    [Header("Localization")]
    [SerializeField] private LocalizedString keyboardInputText;
    [SerializeField] private LocalizedString gamepadInputText;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

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

        if (isUsingGamepad == null)
        {
            isUsingGamepad = currentlyUsingGamepad;
            return;
        }

        if (currentlyUsingGamepad != isUsingGamepad)
        {
            isUsingGamepad = currentlyUsingGamepad;
            ShowPanel(isUsingGamepad.Value ? gamepadInputText : keyboardInputText);
        }
    }

    private void ShowPanel(LocalizedString localizedText)
    {
        statusText.text = localizedText.GetLocalizedString();

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