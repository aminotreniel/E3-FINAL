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
            // Calculate the actual heal amount (don't exceed max health)
            float actualHeal = Mathf.Min(healAmount, playerHealth.maxHealth - playerHealth.currentHealth);
            
            // Increase player health (but clamp to max)
            playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth + healAmount, playerHealth.maxHealth);

            Debug.Log("Picked up health! Healed: " + actualHeal + " | Current Health: " + playerHealth.currentHealth);

            Destroy(gameObject); // remove pickup
        }
    }
}
