using Unity.Netcode; 
using UnityEngine;
public class PlayerColorManager : NetworkBehaviour
{

    [SerializeField] private Renderer playerRenderer;
    // An array to hold our different colored materials
    [SerializeField] private Material[] playerMaterials;

    private NetworkVariable<int> colorIndex = new NetworkVariable<int>();
    public override void OnNetworkSpawn()
    {

        colorIndex.OnValueChanged += HandleColorChanged;

        if (IsServer && playerMaterials.Length > 0)
        {

            colorIndex.Value = (int)(OwnerClientId % (ulong)playerMaterials.Length);
        }
        // Apply the color immediately for the local client
        ApplyColor(colorIndex.Value);
    }
    public override void OnNetworkDespawn()
    {

        colorIndex.OnValueChanged -= HandleColorChanged;
    }

    private void HandleColorChanged(int oldColorIndex, int newColorIndex)
    {
        ApplyColor(newColorIndex);
    }

    private void ApplyColor(int index)
    {
        if (playerRenderer == null || playerMaterials.Length == 0) return;

        int safeIndex = index % playerMaterials.Length;
        playerRenderer.material = playerMaterials[safeIndex];
    }
}