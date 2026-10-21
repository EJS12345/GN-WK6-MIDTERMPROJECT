using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button startButton;

    void Start()
    {
        // Ensure the button is not null and add a listener to it
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {

        }
    }

    void OnStartButtonClicked()
    {
        // Load the "SampleScene" when the button is clicked
        SceneManager.LoadScene("SampleScene");
    }
}

