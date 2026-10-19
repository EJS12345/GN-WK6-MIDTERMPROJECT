using UnityEngine;
using Unity.Netcode; // Required for NetworkBehaviour and ServerRpc

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public float groundCheckDistance = 1.1f; // Distance from player center down to floor
    public LayerMask groundLayer;           // Select the layer assigned to floor/obstacle blocks

    private Rigidbody rb;
    private Vector2 currentInput;
    private bool isGrounded;

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
        // 1. Only gather inputs on the client instance that OWNS this player
        if (!IsOwner) return;

        // Reads both WASD and Arrow Keys automatically
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector2 inputVector = new Vector2(moveX, moveZ).normalized;

        // Send input to the server only when directional state changes (prevents network RPC flooding)
        if (inputVector != currentInput)
        {
            currentInput = inputVector;
            UpdateInputServerRpc(inputVector);
        }

        // Check grounded status locally for immediate jump responsiveness
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // 2. Jump Input: Triggers on Spacebar OR Right Shift
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.RightShift)) && isGrounded)
        {
            JumpServerRpc();
        }
    }

    void FixedUpdate()
    {
        // 3. Physics movement executes exclusively on the Server authority
        if (!IsServer || rb == null || rb.isKinematic) return;

        Vector3 moveDirection = new Vector3(currentInput.x, 0f, currentInput.y);
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // Preserve gravity (Y-axis velocity) while applying horizontal movement
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    // Sends movement direction to the server
    [ServerRpc]
    private void UpdateInputServerRpc(Vector2 input)
    {
        currentInput = input;
    }

    // Sends jump action to the server
    [ServerRpc]
    private void JumpServerRpc()
    {
        if (rb == null || rb.isKinematic) return;

        // Double check ground state on server before executing jump impulse
        bool serverGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        if (serverGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // Visualizes ground check distance in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}