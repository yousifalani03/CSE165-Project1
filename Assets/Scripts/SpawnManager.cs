using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] spawnablePrefabs;
    public Transform spawnPoint;
    private int currentIndex = 0;

    public void SpawnCurrentItem()
    {
        if (spawnablePrefabs.Length == 0) return;

        GameObject obj = Instantiate(spawnablePrefabs[currentIndex], 
            spawnPoint.position, spawnPoint.rotation);

        // Make sure spawned objects have physics and are grabbable
        if (obj.GetComponent<Rigidbody>() == null)
            obj.AddComponent<Rigidbody>();

        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();

        if (obj.GetComponent<XRGrabInteractable>() == null)
            obj.AddComponent<XRGrabInteractable>();

        if (obj.GetComponent<SelectionHighlight>() == null)
            obj.AddComponent<SelectionHighlight>();
    }

    public void NextItem()
    {
        currentIndex = (currentIndex + 1) % spawnablePrefabs.Length;
    }

    public void PreviousItem()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = spawnablePrefabs.Length - 1;
    }
}