using Unity.Netcode;
using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.PlayerCrossedFinishLine(netObj.OwnerClientId);
            }
        }
    }
}