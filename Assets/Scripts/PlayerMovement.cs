using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f; 

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;

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
        if (!IsOwner) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // Make movement relative to where the camera is looking
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredDirection = (camForward * moveZ + camRight * moveX).normalized;

        // Pass the calculated world direction to the Server
        Vector2 inputVector = new Vector2(desiredDirection.x, desiredDirection.z);

        if (inputVector != currentInput)
        {
            currentInput = inputVector;
            UpdateInputServerRpc(inputVector);
        }

        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.RightShift)) && isGrounded)
        {
            JumpServerRpc();
        }
    }

    void FixedUpdate()
    {
        if (!IsServer || rb == null || rb.isKinematic) return;

        Vector3 moveDirection = new Vector3(currentInput.x, 0f, currentInput.y);
        Vector3 targetVelocity = moveDirection * moveSpeed;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // Smoothly rotate the 3D character to face the direction they are walking
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    [ServerRpc]
    private void UpdateInputServerRpc(Vector2 input)
    {
        currentInput = input;
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        if (rb == null || rb.isKinematic) return;

        bool serverGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        if (serverGrounded)
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