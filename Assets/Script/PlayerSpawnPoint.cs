using UnityEngine;

/// <summary>
/// Place this on an empty GameObject in your scene to mark where the player should spawn
/// when coming from an elevator. The Elevator script can reference this.
/// </summary>
public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Automatically move player to this position on scene load")]
    public bool autoSpawnPlayerHere = true;
    
    void Start()
    {
        if (autoSpawnPlayerHere)
        {
            // Find the player
            PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
            if (player != null)
            {
                // Move player to this spawn point
                player.transform.position = transform.position;
                player.transform.rotation = transform.rotation;
                
                // Update checkpoint to this position
                player.SetCheckpoint(transform.position);
                
                Debug.Log("Player spawned at: " + transform.position);
            }
            else
            {
                Debug.LogWarning("PlayerSpawnPoint: Could not find PlayerHealth in scene!");
            }
        }
    }
    
    // Draw gizmo in editor to visualize spawn point
    private void OnDrawGizmos()
    {
        // Draw a player-sized capsule
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        Gizmos.DrawLine(transform.position + Vector3.right * 0.5f, transform.position + Vector3.up + Vector3.right * 0.5f);
        Gizmos.DrawLine(transform.position - Vector3.right * 0.5f, transform.position + Vector3.up - Vector3.right * 0.5f);
        
        // Draw forward direction arrow
        Gizmos.color = Color.blue;
        Vector3 forward = transform.forward * 1f;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, forward);
        
        // Draw arrow head
        Vector3 arrowTip = transform.position + Vector3.up * 0.5f + forward;
        Gizmos.DrawLine(arrowTip, arrowTip - forward.normalized * 0.3f + Vector3.right * 0.15f);
        Gizmos.DrawLine(arrowTip, arrowTip - forward.normalized * 0.3f - Vector3.right * 0.15f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "Player Spawn Point", new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = Color.green },
            fontSize = 14,
            fontStyle = FontStyle.Bold
        });
        #endif
    }
}
