using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPointObjects.Length > 0)
        {
            // Automatically assign spawn point based on Player ID (0 for Host, 1 for Client)
            int spawnIndex = (int)(OwnerClientId % (ulong)spawnPointObjects.Length);
            Transform selectedSpawnPoint = spawnPointObjects[spawnIndex].transform;

            // Move the player safely
            transform.position = selectedSpawnPoint.position;
            transform.rotation = selectedSpawnPoint.rotation;
        }
    }
}