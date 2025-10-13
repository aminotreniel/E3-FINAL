using UnityEngine;

public class MedkitPickup : MonoBehaviour
{
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddMedkit(amount);
            Destroy(gameObject);
        }
    }
}
