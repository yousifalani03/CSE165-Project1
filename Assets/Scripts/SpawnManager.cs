using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableItem
    {
        public string displayName;
        public GameObject prefab;
    }

    public SpawnableItem[] items;
    public Transform spawnPoint;

    public void SpawnItem(int index)
    {
        if (index < 0 || index >= items.Length) return;
        if (items[index].prefab == null || spawnPoint == null) return;

        GameObject obj = Instantiate(items[index].prefab,
            spawnPoint.position, spawnPoint.rotation);

        obj.tag = "Interactable";

        if (obj.GetComponent<Rigidbody>() == null)
            obj.AddComponent<Rigidbody>();

        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
    }

    public int GetItemCount()
    {
        return items != null ? items.Length : 0;
    }

    public string GetItemName(int index)
    {
        if (index < 0 || index >= items.Length) return "";
        return items[index].displayName;
    }
}