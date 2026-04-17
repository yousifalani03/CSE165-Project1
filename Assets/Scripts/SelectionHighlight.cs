using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SelectionHighlight : MonoBehaviour
{
    private Color originalColor;
    private Color hoverColor = Color.yellow;
    private Color selectColor = Color.green;
    private Renderer objectRenderer;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }

        if (grabInteractable != null)
        {
            grabInteractable.hoverEntered.AddListener(OnHoverEnter);
            grabInteractable.hoverExited.AddListener(OnHoverExit);
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (objectRenderer != null)
            objectRenderer.material.color = hoverColor;
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        if (objectRenderer != null)
            objectRenderer.material.color = originalColor;
    }

    void OnSelectEnter(SelectEnterEventArgs args)
    {
        if (objectRenderer != null)
            objectRenderer.material.color = selectColor;
    }

    void OnSelectExit(SelectExitEventArgs args)
    {
        if (objectRenderer != null)
            objectRenderer.material.color = originalColor;
    }
}