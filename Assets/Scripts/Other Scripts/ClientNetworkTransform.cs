using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    // "Let the client control their own position, not the Server."
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}