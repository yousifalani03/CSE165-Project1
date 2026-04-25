using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GroupSelectionManager : MonoBehaviour
{
    [Header("Interactors")]
    public CustomInteractor leftInteractor;
    public CustomInteractor rightInteractor;

    [Header("Input Actions")]
    public InputActionReference leftToggleAction;
    public InputActionReference rightToggleAction;
    public InputActionReference duplicateAction;
    public InputActionReference clearAction;

    [Header("Visuals")]
    public Color groupSelectedColor = Color.blue;

    [Header("Duplication")]
    public Vector3 duplicateOffset = new Vector3(0.35f, 0f, 0.35f);

    private List<GameObject> selectedObjects = new List<GameObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    private Dictionary<Transform, Dictionary<GameObject, Vector3>> handPositionOffsets =
        new Dictionary<Transform, Dictionary<GameObject, Vector3>>();

    private Dictionary<Transform, Dictionary<GameObject, Quaternion>> handRotationOffsets =
        new Dictionary<Transform, Dictionary<GameObject, Quaternion>>();

    private bool isScaling = false;
    private float initialHandDistance;
    private Vector3 initialPivot;

    private Dictionary<GameObject, Vector3> initialObjectOffsets = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Vector3> initialObjectScales = new Dictionary<GameObject, Vector3>();

    void OnEnable()
    {
        if (leftToggleAction != null)
        {
            leftToggleAction.action.Enable();
            leftToggleAction.action.performed += OnLeftToggle;
        }

        if (rightToggleAction != null)
        {
            rightToggleAction.action.Enable();
            rightToggleAction.action.performed += OnRightToggle;
        }

        if (duplicateAction != null)
        {
            duplicateAction.action.Enable();
            duplicateAction.action.performed += OnDuplicate;
        }

        if (clearAction != null)
        {
            clearAction.action.Enable();
            clearAction.action.performed += OnClear;
        }
    }

    void OnDisable()
    {
        if (leftToggleAction != null)
        {
            leftToggleAction.action.performed -= OnLeftToggle;
        }

        if (rightToggleAction != null)
        {
            rightToggleAction.action.performed -= OnRightToggle;
        }

        if (duplicateAction != null)
        {
            duplicateAction.action.performed -= OnDuplicate;
        }

        if (clearAction != null)
        {
            clearAction.action.performed -= OnClear;
        }
    }

    void OnLeftToggle(InputAction.CallbackContext ctx)
    {
        ToggleFromInteractor(leftInteractor);
    }

    void OnRightToggle(InputAction.CallbackContext ctx)
    {
        ToggleFromInteractor(rightInteractor);
    }

    void OnDuplicate(InputAction.CallbackContext ctx)
    {
        DuplicateGroup();
    }

    void OnClear(InputAction.CallbackContext ctx)
    {
        ClearSelection();
    }

    void ToggleFromInteractor(CustomInteractor interactor)
    {
        if (interactor == null) return;

        GameObject obj = interactor.CurrentHoveredObject;
        if (obj == null) return;

        ToggleObject(obj);
    }

    void ToggleObject(GameObject obj)
    {
        if (selectedObjects.Contains(obj))
        {
            selectedObjects.Remove(obj);
            RestoreColor(obj);
        }
        else
        {
            selectedObjects.Add(obj);
            ApplyGroupColor(obj);
        }
    }

    public bool IsGroupSelected(GameObject obj)
    {
        return obj != null && selectedObjects.Contains(obj);
    }

    public bool ShouldUseGroupManipulation(GameObject obj)
    {
        return selectedObjects.Count > 1 && IsGroupSelected(obj);
    }

    public void BeginGroupManipulation(Transform hand)
    {
        Dictionary<GameObject, Vector3> positionOffsets = new Dictionary<GameObject, Vector3>();
        Dictionary<GameObject, Quaternion> rotationOffsets = new Dictionary<GameObject, Quaternion>();

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null) continue;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            positionOffsets[obj] = Quaternion.Inverse(hand.rotation) * (obj.transform.position - hand.position);
            rotationOffsets[obj] = Quaternion.Inverse(hand.rotation) * obj.transform.rotation;
        }

        handPositionOffsets[hand] = positionOffsets;
        handRotationOffsets[hand] = rotationOffsets;

        if (handPositionOffsets.Count >= 2)
        {
            StartScaling();
        }
    }

    public void UpdateGroupManipulation(Transform hand)
    {
        if (!handPositionOffsets.ContainsKey(hand)) return;

        if (handPositionOffsets.Count >= 2)
        {
            UpdateScaling();
            return;
        }

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null) continue;

            obj.transform.position =
                hand.position + hand.rotation * handPositionOffsets[hand][obj];

            obj.transform.rotation =
                hand.rotation * handRotationOffsets[hand][obj];
        }
    }

    public void EndGroupManipulation(Transform hand)
    {
        if (handPositionOffsets.ContainsKey(hand))
        {
            handPositionOffsets.Remove(hand);
        }

        if (handRotationOffsets.ContainsKey(hand))
        {
            handRotationOffsets.Remove(hand);
        }

        if (handPositionOffsets.Count < 2)
        {
            isScaling = false;
        }

        if (handPositionOffsets.Count == 0)
        {
            foreach (GameObject obj in selectedObjects)
            {
                if (obj == null) continue;

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }

                ApplyGroupColor(obj);
            }
        }
    }

    void StartScaling()
    {
        List<Transform> hands = new List<Transform>(handPositionOffsets.Keys);

        if (hands.Count < 2) return;

        isScaling = true;

        initialHandDistance = Vector3.Distance(hands[0].position, hands[1].position);
        initialPivot = GetPivot();

        initialObjectOffsets.Clear();
        initialObjectScales.Clear();

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null) continue;

            initialObjectOffsets[obj] = obj.transform.position - initialPivot;
            initialObjectScales[obj] = obj.transform.localScale;
        }
    }

    void UpdateScaling()
    {
        List<Transform> hands = new List<Transform>(handPositionOffsets.Keys);

        if (hands.Count < 2) return;

        if (!isScaling)
        {
            StartScaling();
        }

        float currentDistance = Vector3.Distance(hands[0].position, hands[1].position);

        if (initialHandDistance < 0.001f) return;

        float scaleFactor = currentDistance / initialHandDistance;
        Vector3 currentPivot = (hands[0].position + hands[1].position) * 0.5f;

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null) continue;

            obj.transform.position = currentPivot + initialObjectOffsets[obj] * scaleFactor;
            obj.transform.localScale = initialObjectScales[obj] * scaleFactor;
        }
    }

    Vector3 GetPivot()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null) continue;

            sum += obj.transform.position;
            count++;
        }

        if (count == 0) return Vector3.zero;

        return sum / count;
    }

    void DuplicateGroup()
    {
        if (selectedObjects.Count == 0) return;

        List<GameObject> copies = new List<GameObject>();

        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null) continue;

            GameObject copy = Instantiate(
                obj,
                obj.transform.position + duplicateOffset,
                obj.transform.rotation
            );

            copy.transform.localScale = obj.transform.localScale;
            copy.tag = obj.tag;

            copies.Add(copy);
        }

        ClearSelection();

        foreach (GameObject copy in copies)
        {
            selectedObjects.Add(copy);
            ApplyGroupColor(copy);
        }
    }

    public void ClearSelection()
    {
        foreach (GameObject obj in selectedObjects)
        {
            if (obj == null) continue;
            RestoreColor(obj);
        }

        selectedObjects.Clear();
        handPositionOffsets.Clear();
        handRotationOffsets.Clear();
        isScaling = false;
    }

    void ApplyGroupColor(GameObject obj)
    {
        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend == null) return;

        if (!originalColors.ContainsKey(obj))
        {
            originalColors[obj] = rend.material.color;
        }

        rend.material.color = groupSelectedColor;
    }

    void RestoreColor(GameObject obj)
    {
        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend == null) return;

        if (originalColors.ContainsKey(obj))
        {
            rend.material.color = originalColors[obj];
            originalColors.Remove(obj);
        }
    }
}