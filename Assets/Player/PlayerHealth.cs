
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBarSlider;

    [Header("Respawn Settings")]
    private Vector3 currentCheckpoint;
    public bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentCheckpoint = transform.position; // First checkpoint = spawn position

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }

        // Move health slider to top-left of the screen (runtime safety)
        if (healthBarSlider != null)
        {
            RectTransform rt = healthBarSlider.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                // Set a small offset from the top-left corner (x: 10, y: -10)
                rt.anchoredPosition = new Vector2(10f, -10f);
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log("Player took " + damageAmount + " damage! Health: " + currentHealth);

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Died!");
        Respawn();
    }

    public void SetCheckpoint(Vector3 checkpointPos)
    {
        currentCheckpoint = checkpointPos;
        Debug.Log("Checkpoint updated: " + checkpointPos);
    }

    private void Respawn()
    {
        transform.position = currentCheckpoint; // move to last checkpoint

        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        isDead = false;
        Debug.Log("Respawned at checkpoint!");
    }
}
