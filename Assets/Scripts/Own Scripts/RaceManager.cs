using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    [Header("UI Panel References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject inGameHUD;
    [SerializeField] private GameObject endGamePanel;

    [Header("Text References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text hudLeaderboardText;
    [SerializeField] private TMP_Text endGameLeaderboardText;

    [Header("Race Settings")]
    [SerializeField] private float totalRaceTime = 120f; // 2 minutes countdown

    // Synchronized Netcode Variables
    private NetworkVariable<float> networkTime = new NetworkVariable<float>(120f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isRaceActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private List<string> finishedPlayers = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            networkTime.Value = totalRaceTime;
        }

        if (endGamePanel != null) endGamePanel.SetActive(false);
        if (inGameHUD != null) inGameHUD.SetActive(false);
    }

    // Called when Host/Client connects successfully
    public void StartRace()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (inGameHUD != null) inGameHUD.SetActive(true);

        if (IsServer)
        {
            isRaceActive.Value = true;
            networkTime.Value = totalRaceTime;
        }
    }

    private void Update()
    {
        if (!isRaceActive.Value) return;

        // Server handles the timer logic
        if (IsServer)
        {
            if (networkTime.Value > 0)
            {
                networkTime.Value -= Time.deltaTime;
            }
            else
            {
                networkTime.Value = 0;
                EndRace();
            }
        }

        // Update local UI timer display on all clients
        UpdateTimerDisplay(networkTime.Value);
    }

    private void UpdateTimerDisplay(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        if (timerText != null)
        {
            timerText.text = string.Format("Timer: {0:0}:{1:00}", minutes, seconds);
        }
    }

    // Call this when a player steps on the Finish Line trigger box
    public void PlayerCrossedFinishLine(ulong clientId)
    {
        if (!IsServer || !isRaceActive.Value) return;

        string playerName = "Player " + (clientId + 1);
        if (!finishedPlayers.Contains(playerName))
        {
            finishedPlayers.Add(playerName);

            // FIX: Format the string on the Server, then send ONE string to clients
            string display = "LEADERBOARD\n";
            for (int i = 0; i < finishedPlayers.Count; i++)
            {
                display += (i + 1) + "st: " + finishedPlayers[i] + "\n";
            }

            UpdateHUDLeaderboardClientRpc(display);

            // End race when all testing players (e.g., 2 players) cross the finish line
            if (finishedPlayers.Count >= 2)
            {
                EndRace();
            }
        }
    }

    private void EndRace()
    {
        if (!IsServer) return;

        isRaceActive.Value = false;

        // FIX: Format the string on the Server, then send ONE string to clients
        string display = "END OF CLOCK AND RACE\nLeaderboard\n";
        if (finishedPlayers.Count == 0)
        {
            display += "Time's Up! No Winners!";
        }
        else
        {
            for (int i = 0; i < finishedPlayers.Count; i++)
            {
                display += (i + 1) + "st: " + finishedPlayers[i] + "\n";
            }
        }

        ShowEndGameOverlayClientRpc(display);
    }

    // FIX: Accepting a single standard string instead of a string array
    [ClientRpc]
    private void UpdateHUDLeaderboardClientRpc(string formattedLeaderboard)
    {
        if (hudLeaderboardText != null) hudLeaderboardText.text = formattedLeaderboard;
    }

    // FIX: Accepting a single standard string instead of a string array
    [ClientRpc]
    private void ShowEndGameOverlayClientRpc(string formattedEndScreen)
    {
        if (endGamePanel != null) endGamePanel.SetActive(true);
        if (endGameLeaderboardText != null) endGameLeaderboardText.text = formattedEndScreen;
    }
}