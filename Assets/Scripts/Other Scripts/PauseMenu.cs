using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Required for changing scenes
using Unity.Netcode;               // Required for safely shutting down multiplayer

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public Slider volumeSlider;
    private bool isPaused = false;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Initialize volume slider
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // --- UPDATED EXIT METHOD ---
    public void QuitGame()
    {
        // 1. Unpause the game time so the next scene doesn't freeze
        Time.timeScale = 1f;
        isPaused = false;

        // 2. Shut down the multiplayer server/client connection safely
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // 3. Load the Main Menu scene
        SceneManager.LoadScene("TitleScreenManager");
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }
}