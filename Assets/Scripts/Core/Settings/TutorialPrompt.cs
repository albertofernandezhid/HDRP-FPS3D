using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Localization;
using System.Linq;

public class TutorialPrompt : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string actionName;
        public LocalizedString tutorialText;
        public bool requireInputConfirmation;
        [HideInInspector] public bool completed;
    }

    [Header("Tutorial Configuration")]
    [SerializeField] private TutorialStep[] tutorialSteps;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private bool destroyAfterCompletion = true;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TextMeshProUGUI buttonPromptText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Input Device Icons")]
    [SerializeField] private bool useDeviceSpecificIcons = true;
    [SerializeField] private string keyboardIconFormat = "[{0}]";
    [SerializeField] private string gamepadIconFormat = "<sprite name=\"{0}\">";

    [Header("Visual Feedback")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.5f;

    private PlayerInputActions inputActions;
    private int currentStepIndex = -1;
    private bool isUsingGamepad = false;
    private Coroutine displayCoroutine;
    private bool playerInside = false;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        inputActions = new PlayerInputActions();

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
        EnableCurrentInputAction();
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
        DisableCurrentInputAction();
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        var action = obj as InputAction;
        if (action == null) return;

        var device = action.activeControl?.device;
        if (device == null) return;

        bool newIsUsingGamepad = device is Gamepad;

        if (newIsUsingGamepad != isUsingGamepad)
        {
            isUsingGamepad = newIsUsingGamepad;
            UpdateCurrentPrompt();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        StartTutorial();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        HideTutorial();
    }

    public void StartTutorial()
    {
        if (tutorialSteps == null || tutorialSteps.Length == 0) return;

        currentStepIndex = 0;
        ShowStep(currentStepIndex);
    }

    private void ShowStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= tutorialSteps.Length) return;

        var step = tutorialSteps[stepIndex];
        string bindingDisplay = GetCurrentBindingDisplay(step.actionName);

        string formattedText = string.Format(
            step.tutorialText.GetLocalizedString(),
            bindingDisplay
        );

        tutorialText.text = formattedText;

        if (step.requireInputConfirmation && buttonPromptText != null)
        {
            string confirmBinding = GetCurrentBindingDisplay(step.actionName, true);
            buttonPromptText.text = string.Format("Press {0} to continue", confirmBinding);
            buttonPromptText.gameObject.SetActive(true);
            EnableCurrentInputAction();
        }
        else if (buttonPromptText != null)
        {
            buttonPromptText.gameObject.SetActive(false);
        }

        ShowPanel();
    }

    private string GetCurrentBindingDisplay(string actionName, bool forConfirmation = false)
    {
        var action = GetActionByName(actionName);
        if (action == null) return "[Action not found]";

        var binding = GetRelevantBinding(action);
        if (binding == null) return "[No binding]";

        if (forConfirmation)
        {
            var confirmBinding = action.bindings.FirstOrDefault(b =>
                b.isPartOfComposite == false &&
                b.path.Contains(isUsingGamepad ? "Gamepad" : "Keyboard"));

            if (confirmBinding != null)
                binding = confirmBinding;
        }

        if (useDeviceSpecificIcons)
        {
            return FormatBindingForDisplay(binding);
        }
        else
        {
            return InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
    }

    private InputAction GetActionByName(string actionName)
    {
        foreach (var actionMap in inputActions.asset.actionMaps)
        {
            var action = actionMap.FindAction(actionName);
            if (action != null) return action;
        }
        return null;
    }

    private InputBinding GetRelevantBinding(InputAction action)
    {
        var deviceType = isUsingGamepad ? "Gamepad" : "Keyboard";

        var relevantBinding = action.bindings.FirstOrDefault(b =>
            b.effectivePath.Contains(deviceType) &&
            !b.isPartOfComposite);

        if (relevantBinding == null)
        {
            relevantBinding = action.bindings.FirstOrDefault(b => !b.isPartOfComposite);
        }

        return relevantBinding;
    }

    private string FormatBindingForDisplay(InputBinding binding)
    {
        string path = binding.effectivePath;

        if (isUsingGamepad)
        {
            var buttonName = path.Split('/').LastOrDefault();
            return string.Format(gamepadIconFormat, GetGamepadSpriteName(buttonName));
        }
        else
        {
            var keyName = path.Split('/').LastOrDefault()
                .Replace("<Keyboard>/", "")
                .Replace("Arrow", "");

            return string.Format(keyboardIconFormat, keyName.ToUpper());
        }
    }

    private string GetGamepadSpriteName(string buttonPath)
    {
        switch (buttonPath.ToLower())
        {
            case "buttonnorth": return "Gamepad_Y";
            case "buttonsouth": return "Gamepad_A";
            case "buttoneast": return "Gamepad_B";
            case "buttonwest": return "Gamepad_X";
            case "leftshoulder": return "Gamepad_LB";
            case "rightshoulder": return "Gamepad_RB";
            case "lefttrigger": return "Gamepad_LT";
            case "righttrigger": return "Gamepad_RT";
            case "start": return "Gamepad_Start";
            case "select": return "Gamepad_Select";
            case "leftstick": return "Gamepad_LeftStick";
            case "rightstick": return "Gamepad_RightStick";
            default: return buttonPath;
        }
    }

    private void UpdateCurrentPrompt()
    {
        if (currentStepIndex >= 0 && playerInside)
        {
            ShowStep(currentStepIndex);
        }
    }

    private void ShowPanel()
    {
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        displayCoroutine = StartCoroutine(FadeIn());
    }

    private void HideTutorial()
    {
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        displayCoroutine = StartCoroutine(FadeOut());

        currentStepIndex = -1;
        DisableCurrentInputAction();
    }

    private IEnumerator FadeIn()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        if (currentStepIndex >= 0 && !tutorialSteps[currentStepIndex].requireInputConfirmation)
        {
            yield return new WaitForSeconds(displayDuration);
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (currentStepIndex >= tutorialSteps.Length - 1 && destroyAfterCompletion)
        {
            Destroy(gameObject);
        }
    }

    private void EnableCurrentInputAction()
    {
        if (currentStepIndex < 0 || currentStepIndex >= tutorialSteps.Length) return;

        var step = tutorialSteps[currentStepIndex];
        if (!step.requireInputConfirmation) return;

        var action = GetActionByName(step.actionName);
        if (action != null)
        {
            action.performed += OnRequiredActionPerformed;
            action.Enable();
        }
    }

    private void DisableCurrentInputAction()
    {
        if (currentStepIndex < 0 || currentStepIndex >= tutorialSteps.Length) return;

        var step = tutorialSteps[currentStepIndex];
        if (!step.requireInputConfirmation) return;

        var action = GetActionByName(step.actionName);
        if (action != null)
        {
            action.performed -= OnRequiredActionPerformed;
            action.Disable();
        }
    }

    private void OnRequiredActionPerformed(InputAction.CallbackContext context)
    {
        if (currentStepIndex < 0 || currentStepIndex >= tutorialSteps.Length) return;

        tutorialSteps[currentStepIndex].completed = true;

        currentStepIndex++;

        if (currentStepIndex < tutorialSteps.Length)
        {
            ShowStep(currentStepIndex);
        }
        else
        {
            HideTutorial();
        }
    }
}