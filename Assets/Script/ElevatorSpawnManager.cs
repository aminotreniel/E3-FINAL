using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Persistent spawn manager that handles player spawning across scenes.
/// This GameObject persists between scenes and automatically spawns the player.
/// </summary>
public class ElevatorSpawnManager : MonoBehaviour
{
    private static ElevatorSpawnManager instance;
    private static bool shouldSpawn = false;
    private static Vector3 spawnPosition = Vector3.zero;
    private static string targetScene = "";
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        shouldSpawn = false;
        spawnPosition = Vector3.zero;
        targetScene = "";
    }
    
    void Awake()
    {
        // Singleton pattern - only one instance should exist
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldSpawn && scene.name == targetScene)
        {
            Debug.Log($"ElevatorSpawnManager: Scene '{scene.name}' loaded, spawning player at {spawnPosition}");
            StartCoroutine(SpawnPlayerAtPosition());
        }
    }
    
    private IEnumerator SpawnPlayerAtPosition()
    {
        // Wait for scene to fully initialize
        yield return new WaitForSeconds(0.2f);
        
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("ElevatorSpawnManager: Cannot find player with 'Player' tag!");
            shouldSpawn = false;
            yield break;
        }
        
        Debug.Log($"ElevatorSpawnManager: Found player at {player.transform.position}, moving to {spawnPosition}");
        
        // Disable all movement-related components
        var movementScripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in movementScripts)
        {
            if (script.GetType().Name.Contains("Movement") || 
                script.GetType().Name.Contains("Controller") ||
                script.GetType().Name.Contains("Player"))
            {
                script.enabled = false;
            }
        }
        
        CharacterController cc = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();
        
        if (cc != null) cc.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Wait for physics
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        
        // Set position multiple times
        for (int i = 0; i < 3; i++)
        {
            player.transform.position = spawnPosition;
            yield return null;
        }
        
        Debug.Log($"ElevatorSpawnManager: Player position set to {player.transform.position}");
        
        // Re-enable components
        if (rb != null) rb.isKinematic = false;
        if (cc != null) cc.enabled = true;
        
        foreach (var script in movementScripts)
        {
            if (script.GetType().Name.Contains("Movement") || 
                script.GetType().Name.Contains("Controller") ||
                script.GetType().Name.Contains("Player"))
            {
                script.enabled = true;
            }
        }
        
        Debug.Log($"✓ ElevatorSpawnManager: Player successfully spawned at {player.transform.position}");
        
        // Reset spawn flag
        shouldSpawn = false;
    }
    
    /// <summary>
    /// Call this method to set where the player should spawn in the next scene
    /// </summary>
    public static void SetSpawnPosition(Vector3 position, string sceneName)
    {
        shouldSpawn = true;
        spawnPosition = position;
        targetScene = sceneName;
        
        Debug.Log($"ElevatorSpawnManager: Will spawn player at {position} when scene '{sceneName}' loads");
        
        // Create manager if it doesn't exist
        if (instance == null)
        {
            GameObject managerObj = new GameObject("ElevatorSpawnManager");
            managerObj.AddComponent<ElevatorSpawnManager>();
        }
    }
}
