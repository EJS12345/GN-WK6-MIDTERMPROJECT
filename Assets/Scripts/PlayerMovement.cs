using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public float groundCheckDistance = 1.1f; // Distance from player center down to floor
    public LayerMask groundLayer;           // Select the layer assigned to floor/obstacle blocks

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool isGrounded;
    private bool jumpRequested;

    void Start()
    {
        // Get the Rigidbody component attached to the player
        rb = GetComponent<Rigidbody>();

        // Freeze rotation so physics collisions don't tilt or flip the player
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. Gather movement input once per frame inside Update
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // Store the values and normalize them so diagonal movement isn't faster
        inputVector = new Vector2(moveX, moveZ).normalized;

        // 2. Check if the player is touching the ground (casts a short ray downward)
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // 3. Catch Spacebar input for Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        // 4. Apply physics movement inside FixedUpdate for smooth collisions
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        // Calculate target velocity while preserving the existing falling/gravity speed (Y axis)
        Vector3 targetVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // 5. Apply instant upward jump impulse if requested
        if (jumpRequested)
        {
            // Reset vertical velocity prior to jump for a consistent jump height
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    // Optional: Visualizes the ground check ray inside the Scene View for easy debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}