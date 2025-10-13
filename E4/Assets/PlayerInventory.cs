using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Hand Slot (assign HandSlot under the right-hand bone)")]
    public Transform handSlot;

    [Header("Settings")]
    [Tooltip("Name of the layer used for first-person weapon rendering (copy from your gun prefab)")]
    public string weaponLayerName = "First Person View"; // change to your project layer name
    [Tooltip("Duration the item stays visible in hand after using (seconds)")]
    public float handShowDuration = 0.0f; // 0 => remains until used; we'll destroy when consumed

    // references
    private PlayerHealth playerHealth;
    private PlayerPoison playerPoison;

    // inventory counts
    public int medkitCount = 0;
    public int antidoteCount = 0;

    // hand prefab registry
    private Dictionary<PickupItem.ItemType, GameObject> handPrefabs =
        new Dictionary<PickupItem.ItemType, GameObject>();

    // current equipped in hand
    private PickupItem.ItemType? equippedItem = null;
    private GameObject currentHandInstance = null;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerPoison = GetComponent<PlayerPoison>();

        if (handSlot == null)
            Debug.LogWarning("PlayerInventory: handSlot not assigned.");
    }

    void Update()
    {
        // Equip (show) — H for medkit, P for antidote
        if (Input.GetKeyDown(KeyCode.H) && medkitCount > 0)
            Equip(PickupItem.ItemType.Medkit);

        if (Input.GetKeyDown(KeyCode.P) && antidoteCount > 0)
            Equip(PickupItem.ItemType.Antidote);

        // Use equipped (left click)
        if (Input.GetMouseButtonDown(0) && equippedItem != null)
            UseEquipped();
    }

    // Public API used by pickup script
    public void AddMedkit(int amount = 1) { medkitCount += amount; }
    public void AddAntidote(int amount = 1) { antidoteCount += amount; }

    public void RegisterHandItem(PickupItem.ItemType type, GameObject prefab)
    {
        if (prefab == null) return;
        handPrefabs[type] = prefab;
    }

    private void Equip(PickupItem.ItemType type)
    {
        // If nothing registered for this type, we cannot equip visually
        if (!handPrefabs.ContainsKey(type))
        {
            Debug.LogWarning("No hand prefab registered for " + type);
            equippedItem = null;
            return;
        }

        // remove old instance
        if (currentHandInstance != null)
            Destroy(currentHandInstance);

        GameObject prefab = handPrefabs[type];

        // Instantiate as child of handSlot with correct transform
        if (handSlot == null)
        {
            Debug.LogError("HandSlot not assigned on PlayerInventory!");
            return;
        }

        currentHandInstance = Instantiate(prefab, handSlot.position, handSlot.rotation, handSlot);
        // reset local transform so it sits correctly
        currentHandInstance.transform.localPosition = Vector3.zero;
        currentHandInstance.transform.localRotation = Quaternion.identity;
        currentHandInstance.transform.localScale = Vector3.one;

        // Set to the weapon layer (so FPS camera sees it)
        int layer = LayerMask.NameToLayer(weaponLayerName);
        if (layer == -1)
        {
            Debug.LogError($"PlayerInventory: Layer '{weaponLayerName}' not found. Set the 'weaponLayerName' to the same layer used by guns.");
        }
        else
        {
            SetLayerRecursively(currentHandInstance, layer);
        }

        equippedItem = type;

        Debug.Log($"Equipped {type} in hand. Instance pos (world): {currentHandInstance.transform.position} layer={layer}");
    }

    private void UseEquipped()
    {
        if (equippedItem == null) return;

        // apply effect
        if (equippedItem == PickupItem.ItemType.Medkit && medkitCount > 0)
        {
            if (playerHealth != null)
            {
                playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth + 30f, playerHealth.maxHealth);
                if (playerHealth.healthBarSlider != null) playerHealth.healthBarSlider.value = playerHealth.currentHealth;
            }
            medkitCount--;
        }
        else if (equippedItem == PickupItem.ItemType.Antidote && antidoteCount > 0)
        {
            if (playerPoison != null) playerPoison.ApplyPoison(-40f); // reduce poison
            antidoteCount--;
        }

        // optionally keep the item visible for a short duration (handShowDuration); here we just destroy immediately
        if (currentHandInstance != null)
        {
            if (handShowDuration > 0f)
            {
                Destroy(currentHandInstance, handShowDuration);
            }
            else
            {
                Destroy(currentHandInstance);
            }
        }

        Debug.Log($"Used {equippedItem}. Remaining medkits: {medkitCount}, antidotes: {antidoteCount}");

        equippedItem = null;
        currentHandInstance = null;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
}
