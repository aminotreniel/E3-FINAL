using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Tooltip("How much health this pickup restores")]
    public float healAmount = 25f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // Increase player health (but clamp to max)
            playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth + healAmount, playerHealth.maxHealth);

            if (playerHealth.healthBarSlider != null)
                playerHealth.healthBarSlider.value = playerHealth.currentHealth;

            Debug.Log("Picked up health! Current Health: " + playerHealth.currentHealth);

            Destroy(gameObject); // remove pickup
        }
    }
}
