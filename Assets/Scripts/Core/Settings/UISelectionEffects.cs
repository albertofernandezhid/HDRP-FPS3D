using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem; // Importante: Usar el nuevo sistema

public class UISelectionEffects : MonoBehaviour
{
    public static UISelectionEffects Instance { get; private set; }

    [Header("Configuración del Borde")]
    public Color outlineColor = Color.yellow;
    public Vector2 outlineDistance = new Vector2(3, -3);

    [Header("Configuración DOTween")]
    public float scaleMultiplier = 1.1f;
    public float animationDuration = 0.2f;
    public Ease easeType = Ease.OutBack;

    private GameObject lastSelected;
    private bool isUsingGamepad = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        // Suscribirse al mismo evento que tu primer script
        InputSystem.onActionChange += OnInputDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnInputDeviceChange;
    }

    private void OnInputDeviceChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        InputAction action = (InputAction)obj;
        bool currentlyUsingGamepad = action.activeControl.device is Gamepad;

        // Si cambiamos de dispositivo
        if (currentlyUsingGamepad != isUsingGamepad)
        {
            isUsingGamepad = currentlyUsingGamepad;

            // Si pasamos a teclado/ratón, limpiamos los efectos actuales
            if (!isUsingGamepad && lastSelected != null)
            {
                ResetLastObject(lastSelected);
            }
            // Si pasamos a mando, activamos el efecto en lo que esté seleccionado
            else if (isUsingGamepad && EventSystem.current?.currentSelectedGameObject != null)
            {
                SetupNewObject(EventSystem.current.currentSelectedGameObject);
            }
        }
    }

    void Update()
    {
        if (EventSystem.current == null) return;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // Solo aplicamos efectos si el cambio de selección ocurre mientras usamos mando
        if (currentSelected != lastSelected)
        {
            ResetLastObject(lastSelected);

            if (currentSelected != null && isUsingGamepad)
            {
                SetupNewObject(currentSelected);
            }

            lastSelected = currentSelected;
        }
    }

    private void SetupNewObject(GameObject target)
    {
        if (target == null || target.GetComponent<Selectable>() == null) return;

        target.transform.DOScale(scaleMultiplier, animationDuration).SetEase(easeType).SetUpdate(true);

        var outline = target.GetComponent<Outline>() ?? target.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = outlineDistance;
        outline.enabled = true;
    }

    private void ResetLastObject(GameObject target)
    {
        if (target != null)
        {
            target.transform.DOScale(1f, animationDuration).SetEase(easeType).SetUpdate(true);
            if (target.TryGetComponent<Outline>(out var oldOutline))
            {
                oldOutline.enabled = false;
            }
        }
    }
}