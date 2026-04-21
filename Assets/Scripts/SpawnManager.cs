using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] spawnablePrefabs;
    public Transform spawnPoint;
    private int currentIndex = 0;

    public void SpawnCurrentItem()
    {
        if (spawnablePrefabs.Length == 0 || spawnPoint == null) return;

        GameObject obj = Instantiate(spawnablePrefabs[currentIndex], 
            spawnPoint.position, spawnPoint.rotation);

        obj.tag = "Interactable";

        if (obj.GetComponent<Rigidbody>() == null)
            obj.AddComponent<Rigidbody>();

        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
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

    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}