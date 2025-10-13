using UnityEngine;

public class DoorKeyLock : MonoBehaviour
{
    [Header("Door Settings")]
    public DoorController doorController; // Reference to your existing door script
    public string requiredKey = "GoldenKey"; // Name or ID of the key

    private bool playerHasKey = false;

    // Call this method when player picks up a key
    public void AcquireKey(string keyName)
    {
        if (keyName == requiredKey)
            playerHasKey = true;
    }

    // This method should be called when player tries to open the door
    public void TryOpenDoor()
    {
        if (playerHasKey)
        {
            doorController.OpenDoor(); // Call your existing open logic
        }
        else
        {
            Debug.Log("You need the key to open this door!");
            // Optionally, show UI message to player
        }
    }
}
