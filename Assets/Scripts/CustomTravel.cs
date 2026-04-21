using UnityEngine;
using UnityEngine.InputSystem;

public class CustomTravel : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference moveAction;      // Left joystick 2D vector
    public InputActionReference turnAction;      // Right joystick 2D vector

    [Header("References")]
    public Transform headTransform; 

    [Header("Movement Settings")]
    public float moveSpeed = 2.5f;
    public float turnSpeed = 60f;
    public float deadzone = 0.15f;

    [Header("Travel Indicator")]
    public GameObject travelIndicator;

    void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (turnAction != null) turnAction.action.Enable();
    }

    void Update()
    {
        HandleMovement();
        HandleTurning();
        UpdateIndicator();
    }

    void HandleMovement()
    {
        if (moveAction == null) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        if (input.magnitude < deadzone) return;

        Vector3 forward = headTransform != null ? headTransform.forward : transform.forward;
        Vector3 right = headTransform != null ? headTransform.right : transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * input.y + right * input.x);

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void HandleTurning()
    {
        if (turnAction == null) return;

        Vector2 input = turnAction.action.ReadValue<Vector2>();
        if (Mathf.Abs(input.x) < deadzone) return;

        Vector3 pivot = headTransform != null ? headTransform.position : transform.position;
        pivot.y = transform.position.y;
        transform.RotateAround(pivot, Vector3.up, input.x * turnSpeed * Time.deltaTime);
    }

    void UpdateIndicator()
    {
        if (travelIndicator == null || moveAction == null) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        bool moving = input.magnitude >= deadzone;
        travelIndicator.SetActive(moving);

        if (moving && headTransform != null)
        {
            Vector3 forward = headTransform.forward;
            Vector3 right = headTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * input.y + right * input.x).normalized;
            Vector3 indicatorPos = headTransform.position + moveDirection * 1.5f;
            indicatorPos.y = transform.position.y + 0.02f;

            travelIndicator.transform.position = indicatorPos;
            travelIndicator.transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        }
    }
}