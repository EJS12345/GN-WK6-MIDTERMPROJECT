using UnityEngine;
using Unity.Netcode;
public class LocalPlayerCameraTarget : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        //if thsi game instance does NOT own this specific player.
        //return or stop reading the next line
        if (!IsOwner) return;

        TopDownCameraFollow cameraFollow = Camera.main.GetComponent<TopDownCameraFollow>();
        cameraFollow.SetTarget(transform);
    }
}
