using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float totalRaceTime = 120f;

    private NetworkVariable<float> networkTime = new NetworkVariable<float>(120f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isRaceActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private List<string> finishedPlayers = new List<string>();
    private bool isWaitingForPlayers = false;

    private float waitTimer = 30f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (inGameHUD != null) inGameHUD.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(false);
        Time.timeScale = 1f;

        // the mouse is visible while on the main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            isWaitingForPlayers = true;
           
            waitTimer = 30f;
        }
    }

    private void Update()
    {
        if (IsServer && isWaitingForPlayers)
        {
            waitTimer -= Time.deltaTime;
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 2 || waitTimer <= 0f)
            {
                isWaitingForPlayers = false;
                isRaceActive.Value = true;
                networkTime.Value = totalRaceTime;
                StartRaceUIClientRpc();
            }
        }

        if (!isRaceActive.Value) return;

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

        UpdateTimerDisplay(networkTime.Value);
    }

    [ClientRpc]
    private void StartRaceUIClientRpc()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (inGameHUD != null) inGameHUD.SetActive(true);

        // Lock the mouse to the center of the screen when the race actually starts
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    [ServerRpc(RequireOwnership = false)]
    public void PlayerCrossedFinishLineServerRpc(ulong clientId)
    {
        if (!isRaceActive.Value) return;

        string playerName = "Player " + (clientId + 1);
        if (!finishedPlayers.Contains(playerName))
        {
            finishedPlayers.Add(playerName);

            string display = "LEADERBOARD\n";
            for (int i = 0; i < finishedPlayers.Count; i++)
            {
                display += (i + 1) + "st: " + finishedPlayers[i] + "\n";
            }

            UpdateHUDLeaderboardClientRpc(display);

            if (finishedPlayers.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                EndRace();
            }
        }
    }

    private void EndRace()
    {
        if (!IsServer) return;

        isRaceActive.Value = false;

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

    [ClientRpc]
    private void UpdateHUDLeaderboardClientRpc(string formattedLeaderboard)
    {
        if (hudLeaderboardText != null) hudLeaderboardText.text = formattedLeaderboard;
    }

    [ClientRpc]
    private void ShowEndGameOverlayClientRpc(string formattedEndScreen)
    {
        if (inGameHUD != null) inGameHUD.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(true);
        if (endGameLeaderboardText != null) endGameLeaderboardText.text = formattedEndScreen;

        Time.timeScale = 0f;

        StartCoroutine(ReturnToMenuSequence());
    }

    private IEnumerator ReturnToMenuSequence()
    {
        yield return new WaitForSecondsRealtime(15f);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("TitleScreenManager");
    }
}