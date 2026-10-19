using UnityEngine;
using Unity.Netcode; // Required for multiplayer authority

public class PlayerRespawn : NetworkBehaviour
{
    private Vector3 currentRespawnPosition;
    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        // When the player spawns in, set their initial starting position as the first checkpoint
        if (IsServer)
        {
            currentRespawnPosition = transform.position;
        }
    }

    // Handles touching invisible trigger boxes (like a giant fall zone under the map)
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Only the server is allowed to teleport players
        HandleTouch(other.gameObject);
    }

    // Handles touching solid objects (like solid platform floors or solid trap walls)
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        HandleTouch(collision.gameObject);
    }

    // Central logic for both Triggers and Collisions
    private void HandleTouch(GameObject touchedObject)
    {
        // 1. Did we touch a Checkpoint?
        if (touchedObject.CompareTag("Checkpoint"))
        {
            // Save the player's exact current safe position
            currentRespawnPosition = transform.position;
        }

        // 2. Did we touch a hazard or fall off the map?
        else if (touchedObject.CompareTag("KillZone"))
        {
            // Teleport the player back to their last saved checkpoint
            transform.position = currentRespawnPosition;

            // Instantly stop their falling momentum so they don't glitch through the floor upon respawning
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}