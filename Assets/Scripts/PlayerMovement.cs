using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector2 inputVector;

    void Start()
    {
        // Get the Rigidbody component attached to the player
        rb = GetComponent<Rigidbody>();

        // Freeze rotation so physics collisions don't tilt or flip the player
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. Gather input once per frame inside Update
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // Store the values and normalize them so diagonal movement isn't faster
        inputVector = new Vector2(moveX, moveZ).normalized;
    }

    void FixedUpdate()
    {
        // 2. Apply physics movement inside FixedUpdate for smooth collisions
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        // Calculate target velocity while preserving the existing falling/gravity speed (Y axis)
        Vector3 targetVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
}