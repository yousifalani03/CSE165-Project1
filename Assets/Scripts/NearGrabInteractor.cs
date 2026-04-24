using UnityEngine;
using UnityEngine.InputSystem;

public class NearGrabInteractor : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference gripAction;

    [Header("Near Grab Settings")]
    public float grabRadius = 0.35f;
    public float indicatorSize = 0.08f;
    public LayerMask interactableLayerMask = ~0;

    [Header("Visual Indicator")]
    public GameObject grabIndicator;
    public Color idleColor = Color.cyan;
    public Color hoverColor = Color.yellow;
    public Color grabbedColor = Color.green;

    private GameObject hoveredObject;
    private GameObject grabbedObject;

    private Rigidbody grabbedRigidbody;
    private bool grabbedWasKinematic;

    private Vector3 grabPositionOffset;
    private Quaternion grabRotationOffset;

    private Color originalColor;
    private Renderer highlightedRenderer;

    private CustomInteractor rayInteractor;

    public bool IsNearGrabAvailable()
    {
        return hoveredObject != null || grabbedObject != null;
    }

    void Awake()
    {
        rayInteractor = GetComponent<CustomInteractor>();
    }

    void OnEnable()
    {
        if (gripAction != null)
        {
            gripAction.action.Enable();
            gripAction.action.performed += OnGripPressed;
            gripAction.action.canceled += OnGripReleased;
        }
    }

    void OnDisable()
    {
        if (gripAction != null)
        {
            gripAction.action.performed -= OnGripPressed;
            gripAction.action.canceled -= OnGripReleased;
        }
    }

    void Update()
    {
        if (grabbedObject != null)
        {
            UpdateGrabbedObject();
        }
        else
        {
            UpdateNearbyObject();
        }

        UpdateIndicator();
    }

    void UpdateNearbyObject()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRadius, interactableLayerMask);

        GameObject nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            Transform t = hit.transform;

            while (t != null)
            {
                if (t.CompareTag("Interactable"))
                {
                    float distance = Vector3.Distance(transform.position, t.position);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = t.gameObject;
                    }

                    break;
                }

                t = t.parent;
            }
        }

        if (nearest != hoveredObject)
        {
            ClearHighlight();
            hoveredObject = nearest;

            if (hoveredObject != null)
            {
                if (rayInteractor != null)
                {
                    rayInteractor.ForceClearHover();
                }

                ApplyHighlight(hoveredObject, hoverColor);
            }
        }
    }

    void OnGripPressed(InputAction.CallbackContext ctx)
    {
        if (grabbedObject == null && hoveredObject != null)
        {
            Rigidbody rb = hoveredObject.GetComponent<Rigidbody>();

            if (rb != null && rb.isKinematic) return;

            GrabObject(hoveredObject);
        }
    }

    void OnGripReleased(InputAction.CallbackContext ctx)
    {
        if (grabbedObject != null)
        {
            ReleaseObject();
        }
    }

    void GrabObject(GameObject obj)
    {
        if (rayInteractor != null)
        {
            rayInteractor.ForceClearHover();
        }

        grabbedObject = obj;

        if (highlightedRenderer != null)
        {
            highlightedRenderer.material.color = grabbedColor;
        }

        grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();
        if (grabbedRigidbody != null)
        {
            grabbedWasKinematic = grabbedRigidbody.isKinematic;
            grabbedRigidbody.isKinematic = true;
        }

        grabPositionOffset = Quaternion.Inverse(transform.rotation) * (grabbedObject.transform.position - transform.position);
        grabRotationOffset = Quaternion.Inverse(transform.rotation) * grabbedObject.transform.rotation;
    }

    void UpdateGrabbedObject()
    {
        grabbedObject.transform.position = transform.position + transform.rotation * grabPositionOffset;
        grabbedObject.transform.rotation = transform.rotation * grabRotationOffset;
    }

    void ReleaseObject()
    {
        if (highlightedRenderer != null)
        {
            highlightedRenderer.material.color = originalColor;
        }

        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.isKinematic = grabbedWasKinematic;
        }

        grabbedObject = null;
        grabbedRigidbody = null;
        hoveredObject = null;
        highlightedRenderer = null;
    }

    void ApplyHighlight(GameObject obj, Color highlightColor)
    {
        Renderer rend = obj.GetComponentInChildren<Renderer>();

        if (rend != null)
        {
            if (highlightedRenderer != rend)
            {
                if (highlightedRenderer != null)
                {
                    highlightedRenderer.material.color = originalColor;
                }

                if (rend.material.color != hoverColor && rend.material.color != grabbedColor)
                {
                    originalColor = rend.material.color;
                }

                highlightedRenderer = rend;
            }

            rend.material.color = highlightColor;
        }
    }

    void ClearHighlight()
    {
        if (grabbedObject != null) return;

        if (highlightedRenderer != null)
        {
            highlightedRenderer.material.color = originalColor;
            highlightedRenderer = null;
        }

        hoveredObject = null;
    }

    void UpdateIndicator()
    {
        if (grabIndicator == null) return;

        grabIndicator.transform.position = transform.position;
        grabIndicator.transform.localScale = Vector3.one * indicatorSize;

        Renderer rend = grabIndicator.GetComponent<Renderer>();
        if (rend == null) return;

        if (grabbedObject != null)
        {
            rend.material.color = grabbedColor;
        }
        else if (hoveredObject != null)
        {
            rend.material.color = hoverColor;
        }
        else
        {
            rend.material.color = idleColor;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}