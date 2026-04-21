using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnController : MonoBehaviour
{
    [Header("References")]
    public SpawnManager spawnManager;

    [Header("Input Actions")]
    public InputActionReference spawnAction;     // Button to spawn current item
    public InputActionReference switchAction;    // Button to cycle to next item

    void OnEnable()
    {
        if (spawnAction != null)
        {
            spawnAction.action.Enable();
            spawnAction.action.performed += OnSpawn;
        }
        if (switchAction != null)
        {
            switchAction.action.Enable();
            switchAction.action.performed += OnSwitch;
        }
    }

    void OnDisable()
    {
        if (spawnAction != null)
            spawnAction.action.performed -= OnSpawn;
        if (switchAction != null)
            switchAction.action.performed -= OnSwitch;
    }

    void OnSpawn(InputAction.CallbackContext ctx)
    {
        if (spawnManager != null)
            spawnManager.SpawnCurrentItem();
    }

    void OnSwitch(InputAction.CallbackContext ctx)
    {
        if (spawnManager != null)
            spawnManager.NextItem();
    }
}