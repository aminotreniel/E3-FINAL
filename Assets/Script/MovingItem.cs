using UnityEngine;

public class MovingItem : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 50f; // Degrees per second
    
    [Header("Bobbing Settings")]
    [SerializeField] private float bobbingHeight = 0.5f; // How high/low it moves
    [SerializeField] private float bobbingSpeed = 2f; // How fast it bobs
    
    private Vector3 startPosition;
    
    void Start()
    {
        // Store the initial position
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate the item around the Y-axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        
        // Create bobbing motion using a sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
