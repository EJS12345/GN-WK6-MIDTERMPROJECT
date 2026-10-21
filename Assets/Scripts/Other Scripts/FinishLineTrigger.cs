using Unity.Netcode;
using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CheckFinish(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckFinish(collision.gameObject);
    }

    private void CheckFinish(GameObject touchedObject)
    {
        // Grab the NetworkObject even if the collider is on a child object
        NetworkObject netObj = touchedObject.GetComponentInParent<NetworkObject>();

        // Ensure it has a network identity AND is tagged as a Player
        if (netObj != null && (touchedObject.CompareTag("Player") || netObj.gameObject.CompareTag("Player")))
        {
            if (RaceManager.Instance != null)
            {
                // Trigger the ServerRpc so the server registers the finish
                RaceManager.Instance.PlayerCrossedFinishLineServerRpc(netObj.OwnerClientId);
            }
        }
    }
}