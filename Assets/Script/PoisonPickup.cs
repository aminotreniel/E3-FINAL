using UnityEngine;

public class PoisonPickup : MonoBehaviour
{
    [Tooltip("How much poison this pickup adds")]
    public float poisonAmount = 20f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerPoison playerPoison = other.GetComponent<PlayerPoison>();

        if (playerPoison != null)
        {
            playerPoison.ApplyPoison(poisonAmount);

            Debug.Log("Picked up poison! Current Poison: " + playerPoison.currentPoison);

            Destroy(gameObject); // remove pickup
        }
    }
}
