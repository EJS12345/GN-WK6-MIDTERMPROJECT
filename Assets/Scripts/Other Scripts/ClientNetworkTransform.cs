using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    // This simple override tells Unity Netcode: 
    // "Let the client control their own position, not the Server."
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}