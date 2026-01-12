using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

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
    private Outline currentOutline;
    private bool isUsingGamepad = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        DetectInputMode();

        if (EventSystem.current == null) return;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

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

    private void DetectInputMode()
    {
        float mouseX = Mathf.Abs(Input.GetAxis("Mouse X"));
        float mouseY = Mathf.Abs(Input.GetAxis("Mouse Y"));
        bool mouseClick = Input.GetMouseButtonDown(0);

        if ((mouseX > 0.05f || mouseY > 0.05f || mouseClick) && isUsingGamepad)
        {
            isUsingGamepad = false;
            if (lastSelected != null) ResetLastObject(lastSelected);
        }
        else if (!isUsingGamepad)
        {
            bool h = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f;
            bool v = Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;
            bool anyButton = Input.anyKeyDown && !mouseClick;

            if (h || v || anyButton)
            {
                isUsingGamepad = true;

                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                {
                    SetupNewObject(EventSystem.current.currentSelectedGameObject);
                    lastSelected = EventSystem.current.currentSelectedGameObject;
                }
            }
        }
    }

    private void SetupNewObject(GameObject target)
    {
        if (target == null || target.GetComponent<Selectable>() == null) return;

        target.transform.DOScale(scaleMultiplier, animationDuration).SetEase(easeType).SetUpdate(true);

        currentOutline = target.GetComponent<Outline>();
        if (currentOutline == null)
        {
            currentOutline = target.AddComponent<Outline>();
        }

        currentOutline.effectColor = outlineColor;
        currentOutline.effectDistance = outlineDistance;
        currentOutline.enabled = true;
    }

    private void ResetLastObject(GameObject target)
    {
        if (target != null)
        {
            target.transform.DOScale(1f, animationDuration).SetEase(easeType).SetUpdate(true);

            Outline oldOutline = target.GetComponent<Outline>();
            if (oldOutline != null)
            {
                oldOutline.enabled = false;
            }
        }
    }
}