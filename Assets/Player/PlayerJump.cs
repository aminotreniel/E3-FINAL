using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 7f; // How high the player jumps
    public KeyCode jumpKey = KeyCode.Space; // Jump input key

    [Header("Ground Check")]
    public Transform groundCheck; // Empty GameObject under player
    public float groundDistance = 0.2f;
    public LayerMask groundMask; // What counts as ground

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Jump when key is pressed and player is grounded
        if (isGrounded && Input.GetKeyDown(jumpKey))
        {
            Jump();
        }
    }

    void Jump()
    {
        // Reset Y velocity to avoid stacking forces
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Apply upward force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
