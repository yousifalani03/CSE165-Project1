using UnityEngine;
using UnityEngine.InputSystem;

public class CustomInteractor : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference triggerAction;

    [Header("Raycast Settings")]
    public float rayMaxDistance = 10f;
    public LayerMask interactableLayerMask = ~0;

    [Header("Controller Forward Offset")]
    [Tooltip("Rotation offset applied to the controller's forward direction. Quest 2 controllers usually need about -45 degrees on X.")]
    public Vector3 forwardRotationOffset = new Vector3(-45f, 0f, 0f);

    [Header("Other Hand (for two-hand scale)")]
    public CustomInteractor otherHand;

    [Header("Visuals")]
    public LineRenderer lineRenderer;
    public Color defaultRayColor = Color.white;
    public Color hoverRayColor = Color.yellow;
    public Color selectRayColor = Color.green;

    [Header("Near Grab Conflict Prevention")]
    public NearGrabInteractor nearGrabInteractor;

    // Runtime state
    private GameObject hoveredObject;
    private GameObject selectedObject;
    private Color originalColor;
    private Rigidbody selectedRigidbody;
    private bool selectedWasKinematic;

    // Manipulation state
    private Vector3 grabOffset;
    private Quaternion grabRotationOffset;

    // Two-hand scale state
    private Vector3 scaleInitialScale;
    private float scaleInitialDistance;
    private bool isScaling;

    Vector3 GetRayOrigin() => transform.position;
    Vector3 GetRayDirection() => transform.rotation * Quaternion.Euler(forwardRotationOffset) * Vector3.forward;

    void Awake()
    {
        if (nearGrabInteractor == null)
        {
            nearGrabInteractor = GetComponent<NearGrabInteractor>();
        }
    }

    void OnEnable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.Enable();
            triggerAction.action.performed += OnTriggerPressed;
            triggerAction.action.canceled += OnTriggerReleased;
        }
    }

    void OnDisable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.performed -= OnTriggerPressed;
            triggerAction.action.canceled -= OnTriggerReleased;
        }
    }

    void Update()
    {
        if (nearGrabInteractor != null && nearGrabInteractor.IsNearGrabAvailable() && selectedObject == null)
        {
            ClearHover();

            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }

            return;
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
        if (selectedObject != null && otherHand != null && otherHand.selectedObject == selectedObject)
        {
            if (!isScaling) StartScaling();
            UpdateScaling();
        }
        else if (isScaling)
        {
            EndScaling();
        }

        if (selectedObject != null && !isScaling)
        {
            UpdateManipulation();
        }
        else if (selectedObject == null)
        {
            UpdateRaycast();
        }

        UpdateLineRenderer();
    }

    void UpdateRaycast()
    {
        Ray ray = new Ray(GetRayOrigin(), GetRayDirection());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayMaxDistance, interactableLayerMask))
        {
            Transform t = hit.collider.transform;
            GameObject interactableObj = null;
            while (t != null)
            {
                if (t.CompareTag("Interactable"))
                {
                    interactableObj = t.gameObject;
                    break;
                }
                t = t.parent;
            }

            if (interactableObj != null)
            {
                if (hoveredObject != interactableObj)
                {
                    ClearHover();
                    hoveredObject = interactableObj;
                    ApplyHoverHighlight(hoveredObject);
                }
                return;
            }
        }

        ClearHover();
    }

    void ApplyHoverHighlight(GameObject obj)
{
    Renderer rend = obj.GetComponentInChildren<Renderer>();
    if (rend != null)
    {
        if (rend.material.color != hoverRayColor && rend.material.color != selectRayColor)
        {
            originalColor = rend.material.color;
        }
        rend.material.color = hoverRayColor;
    }
}

    void ClearHover()
    {
        if (hoveredObject != null && hoveredObject != selectedObject)
        {
            Renderer rend = hoveredObject.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                rend.material.color = originalColor;
            }
        }
        hoveredObject = null;
    }

    public void ForceClearHover()
    {
        ClearHover();

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        if (nearGrabInteractor != null && nearGrabInteractor.IsNearGrabAvailable())
        {
            return;
        }

        if (hoveredObject != null && selectedObject == null)
        {
            SelectObject(hoveredObject);
        }
    }
    void OnTriggerReleased(InputAction.CallbackContext ctx)
    {
        if (selectedObject != null)
        {
            ReleaseObject();
        }
    }

    void SelectObject(GameObject obj)
    {
        selectedObject = obj;

        Renderer rend = selectedObject.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = selectRayColor;

        selectedRigidbody = selectedObject.GetComponent<Rigidbody>();
        if (selectedRigidbody != null)
        {
            selectedWasKinematic = selectedRigidbody.isKinematic;
            selectedRigidbody.isKinematic = true;
        }

        grabOffset = Quaternion.Inverse(transform.rotation) * (selectedObject.transform.position - transform.position);
        grabRotationOffset = Quaternion.Inverse(transform.rotation) * selectedObject.transform.rotation;
    }

    void ReleaseObject()
    {
        if (selectedObject == null) return;

        Renderer rend = selectedObject.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = originalColor;

        if (selectedRigidbody != null) selectedRigidbody.isKinematic = selectedWasKinematic;

        selectedObject = null;
        selectedRigidbody = null;
    }

    void UpdateManipulation()
    {
        selectedObject.transform.position = transform.position + transform.rotation * grabOffset;
        selectedObject.transform.rotation = transform.rotation * grabRotationOffset;
    }

    void StartScaling()
    {
        isScaling = true;
        scaleInitialScale = selectedObject.transform.localScale;
        scaleInitialDistance = Vector3.Distance(transform.position, otherHand.transform.position);
    }

    void UpdateScaling()
    {
        float currentDistance = Vector3.Distance(transform.position, otherHand.transform.position);
        if (scaleInitialDistance > 0.001f)
        {
            float scaleFactor = currentDistance / scaleInitialDistance;
            selectedObject.transform.localScale = scaleInitialScale * scaleFactor;
        }
    }

    void EndScaling()
    {
        isScaling = false;
    }

    void UpdateLineRenderer()
    {
        if (lineRenderer == null) return;

        Vector3 origin = GetRayOrigin();
        Vector3 direction = GetRayDirection();

        lineRenderer.SetPosition(0, origin);

        if (selectedObject != null)
        {
            lineRenderer.SetPosition(1, selectedObject.transform.position);
            lineRenderer.startColor = selectRayColor;
            lineRenderer.endColor = selectRayColor;
        }
        else if (hoveredObject != null)
        {
            lineRenderer.SetPosition(1, hoveredObject.transform.position);
            lineRenderer.startColor = hoverRayColor;
            lineRenderer.endColor = hoverRayColor;
        }
        else
        {
            Vector3 endPoint = origin + direction * rayMaxDistance;
            Ray ray = new Ray(origin, direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayMaxDistance))
            {
                endPoint = hit.point;
            }
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.startColor = defaultRayColor;
            lineRenderer.endColor = defaultRayColor;
        }
    }
}