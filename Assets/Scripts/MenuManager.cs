using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject controlsPanel;

    void Start()
    {
        // Make sure game is running at normal speed
        Time.timeScale = 1f;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // Add a dark background so game world isn't visible
        HideGameWorld();
    }

    void HideGameWorld()
    {
        // Find and disable the main camera if it exists
        // so the game world doesn't show behind menu
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.backgroundColor = Color.black;
            mainCam.cullingMask = 0; // render nothing
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowControls()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}