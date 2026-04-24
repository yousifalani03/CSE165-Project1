using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SpawnMenu : MonoBehaviour
{
    [Header("References")]
    public SpawnManager spawnManager;
    public Transform playerHead;
    public GameObject menuDisplay;

    [Header("Text Displays")]
    public TMP_Text itemNameText;
    public TMP_Text counterText;

    [Header("Input Actions")]
    public InputActionReference toggleMenuAction;
    public InputActionReference cycleAction;
    public InputActionReference spawnAction;

    [Header("Placement")]
    public float menuDistance = 1.5f;
    public float menuHeightOffset = 0f;

    [Header("Cycle Settings")]
    public float cycleDeadzone = 0.6f;
    public float cycleCooldown = 0.25f;

    private bool menuVisible = false;
    private int currentIndex = 0;
    private float lastCycleTime = 0f;

    void OnEnable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnToggleMenu;
        }

        if (cycleAction != null)
            cycleAction.action.Enable();

        if (spawnAction != null)
        {
            spawnAction.action.Enable();
            spawnAction.action.performed += OnSpawn;
        }
    }

    void OnDisable()
    {
        if (toggleMenuAction != null)
            toggleMenuAction.action.performed -= OnToggleMenu;

        if (spawnAction != null)
            spawnAction.action.performed -= OnSpawn;
    }

    void Start()
    {
        SetMenuVisible(false);
        UpdateDisplay();
    }

    void Update()
    {
        if (!menuVisible) return;

        PositionMenuInFrontOfPlayer();

        if (cycleAction != null)
        {
            Vector2 stick = cycleAction.action.ReadValue<Vector2>();
            if (Time.time - lastCycleTime > cycleCooldown)
            {
                if (stick.x > cycleDeadzone)
                {
                    CycleNext();
                    lastCycleTime = Time.time;
                }
                else if (stick.x < -cycleDeadzone)
                {
                    CyclePrevious();
                    lastCycleTime = Time.time;
                }
            }
        }
    }

    void OnToggleMenu(InputAction.CallbackContext ctx)
    {
        SetMenuVisible(!menuVisible);
    }

    void OnSpawn(InputAction.CallbackContext ctx)
    {
        if (!menuVisible) return;
        if (spawnManager == null) return;

        spawnManager.SpawnItem(currentIndex);
        SetMenuVisible(false);
    }

    void SetMenuVisible(bool visible)
    {
        menuVisible = visible;

        if (menuDisplay != null)
            menuDisplay.SetActive(visible);

        if (visible)
        {
            PositionMenuInFrontOfPlayer();
            UpdateDisplay();
        }
    }

    void PositionMenuInFrontOfPlayer()
    {
        if (menuDisplay == null || playerHead == null) return;

        Vector3 forward = playerHead.forward;
        forward.y = 0;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 pos = playerHead.position + forward * menuDistance;
        pos.y = playerHead.position.y + menuHeightOffset;

        menuDisplay.transform.position = pos;
        menuDisplay.transform.rotation = Quaternion.LookRotation(pos - playerHead.position);
    }

    void CycleNext()
    {
        if (spawnManager == null || spawnManager.GetItemCount() == 0) return;
        currentIndex = (currentIndex + 1) % spawnManager.GetItemCount();
        UpdateDisplay();
    }

    void CyclePrevious()
    {
        if (spawnManager == null || spawnManager.GetItemCount() == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = spawnManager.GetItemCount() - 1;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (spawnManager == null) return;

        if (itemNameText != null)
            itemNameText.text = spawnManager.GetItemName(currentIndex);

        if (counterText != null)
            counterText.text = (currentIndex + 1) + " / " + spawnManager.GetItemCount();
    }
}