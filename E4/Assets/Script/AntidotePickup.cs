using UnityEngine;

public class AntidotePickup : MonoBehaviour
{
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddAntidote(amount);
            Destroy(gameObject);
        }
    }
}
