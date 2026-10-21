using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text statusText;

    private const string WebGLConnectionType = "wss";

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            statusText.text = "Signed in locally. Ready to connect.";
        }
    }

    public async void StartHost()
    {
        try
        {
            statusText.text = "Creating Relay Session...";
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            joinCodeText.text = "Join Code: " + joinCode;

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.UseWebSockets = true;
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, WebGLConnectionType));

            NetworkManager.Singleton.StartHost();

            // UPDATED: Text now reflects the 30-second wait
            statusText.text = "Host started! Waiting up to 30s for Player 2...";
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            statusText.text = "Failed to start Host.";
        }
    }

    public async void StartClient()
    {
        if (joinCodeInput == null || string.IsNullOrWhiteSpace(joinCodeInput.text))
        {
            statusText.text = "Please enter a Join Code first!";
            return;
        }

        string cleanJoinCode = joinCodeInput.text.Trim().Replace("\u200B", "").Replace("\n", "").Replace("\r", "");

        try
        {
            statusText.text = "Joining Session...";
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(cleanJoinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.UseWebSockets = true;
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, WebGLConnectionType));

            NetworkManager.Singleton.StartClient();
            statusText.text = "Connected! Race starting...";
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            statusText.text = "Invalid Join Code.";
        }
    }
}