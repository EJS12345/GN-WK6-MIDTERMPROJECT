using UnityEngine;
using Unity.Netcode;

public class PlayerRespawn : NetworkBehaviour
{
    private Vector3 currentRespawnPosition;
    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        // 1. CRITICAL FIX: The Server must track the positions, not the Owner.
        if (IsServer)
        {
            currentRespawnPosition = transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 2. CRITICAL FIX: Only the Server is allowed to detect traps and teleport bodies
        if (!IsServer) return;
        HandleTouch(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 2. CRITICAL FIX: Only the Server is allowed to detect traps and teleport bodies
        if (!IsServer) return;
        HandleTouch(collision.gameObject);
    }

    private void HandleTouch(GameObject touchedObject)
    {
        if (touchedObject.CompareTag("Checkpoint"))
        {
            currentRespawnPosition = transform.position;
        }
        else if (touchedObject.CompareTag("KillZone"))
        {
            // Stop momentum so they don't slide off the checkpoint after respawning
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // The Server teleports the player safely
            transform.position = currentRespawnPosition;
        }
    }
}