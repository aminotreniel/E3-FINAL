using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum ItemType { Medkit, Antidote }
    public ItemType itemType;

    [Tooltip("Prefab to spawn into the player's hand (optimized for hand).")]
    public GameObject handPrefab;

    private void Reset()
    {
        // Optional: ensure collider & trigger exist
        Collider c = GetComponent<Collider>();
        if (c == null) gameObject.AddComponent<BoxCollider>().isTrigger = true;
        else c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null)
            return;

        // Add count
        if (itemType == ItemType.Medkit) inventory.AddMedkit();
        else inventory.AddAntidote();

        // Register the hand prefab (handPrefab may be null)
        if (handPrefab != null)
            inventory.RegisterHandItem(itemType, handPrefab);

        Debug.Log($"Picked up {itemType} - now {inventory.medkitCount} medkits / {inventory.antidoteCount} antidotes");

        Destroy(gameObject);
    }
}
