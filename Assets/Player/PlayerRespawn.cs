

using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    private Vector3 currentCheckpoint;

    void Start()
    {
        currentHealth = maxHealth;
        currentCheckpoint = transform.position; // Start position is first checkpoint
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            Respawn();
        }

        // Just for testing: Press R to take damage
        if (Input.GetKeyDown(KeyCode.R))
        {
            TakeDamage(50f);
        }
    }

    public void SetCheckpoint(Vector3 checkpointPos)
    {
        currentCheckpoint = checkpointPos;
        Debug.Log("Checkpoint updated: " + checkpointPos);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    private void Respawn()
    {
        transform.position = currentCheckpoint;
        currentHealth = maxHealth;
        Debug.Log("Respawned at checkpoint!");
    }
}
