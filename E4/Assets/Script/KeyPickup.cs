using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyName = "GoldenKey";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Give key to all doors that require it
            var doors = UnityEngine.Object.FindObjectsByType<DoorController>(FindObjectsSortMode.None);
            foreach (DoorController door in doors)
                door.AcquireKey(keyName);

            Debug.Log("Picked up " + keyName + "!");

            // Update the objective text
            var objManager = UnityEngine.Object.FindAnyObjectByType<ObjectiveManager>();
            if (objManager != null)
            {
                var mgrType = objManager.GetType();
                var completeById = mgrType.GetMethod("CompleteTaskById");
                if (completeById != null)
                {
                    completeById.Invoke(objManager, new object[] { keyName });
                }
                else
                {
                    var onKey = mgrType.GetMethod("OnKeyPickedUp");
                    if (onKey != null)
                        onKey.Invoke(objManager, null);
                }
            }

            // Destroy the key object
            Destroy(gameObject);
        }
    }
}
