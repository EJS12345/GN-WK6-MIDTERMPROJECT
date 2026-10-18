using UnityEngine;
using Unity.Netcode; // Required for NetworkBehaviour and ServerRpc

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // 1. Only gather key input on the client instance that OWNS this character
        if (!IsOwner) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector2 inputVector = new Vector2(moveX, moveZ).normalized;

        // Check if grounded on the local client for responsive jump checks
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        bool jumpRequested = Input.GetButtonDown("Jump") && isGrounded;

        // 2. Pass movement commands to the Server so physics run on the host authority
        MoveServerRpc(inputVector, jumpRequested);
    }

    // ServerRpc executes exclusively on the Host/Server machine
    [ServerRpc]
    private void MoveServerRpc(Vector2 input, bool jump)
    {
        if (rb == null) return;

        // Calculate and apply target velocity on the Server
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
        Vector3 targetVelocity = moveDirection * moveSpeed;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // Apply instant jump impulse on the Server
        if (jump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}