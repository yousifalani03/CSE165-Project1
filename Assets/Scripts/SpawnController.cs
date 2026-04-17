using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnController : MonoBehaviour
{
    public SpawnManager spawnManager;
    public InputActionReference spawnAction;
    public InputActionReference switchAction;

    void OnEnable()
    {
        if (spawnAction != null)
            spawnAction.action.performed += OnSpawn;
        if (switchAction != null)
            switchAction.action.performed += OnSwitch;
    }

    void OnDisable()
    {
        if (spawnAction != null)
            spawnAction.action.performed -= OnSpawn;
        if (switchAction != null)
            switchAction.action.performed -= OnSwitch;
    }

    void OnSpawn(InputAction.CallbackContext context)
    {
        spawnManager.SpawnCurrentItem();
    }

    void OnSwitch(InputAction.CallbackContext context)
    {
        spawnManager.NextItem();
    }
}