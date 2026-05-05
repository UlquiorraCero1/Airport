using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class GameOverScreen : MonoBehaviour
{
    [Header("Panels")]
    public GameObject gameOverPanel;

    [Header("Stats")]
    public TextMeshProUGUI killCountText;

    private static int totalKills = 0;
    public static GameOverScreen Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public static void AddKill()
    {
        totalKills++;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (killCountText != null)
            killCountText.text = "KILLS: " + totalKills;

        var inputModule = FindObjectOfType<InputSystemUIInputModule>();
        if (inputModule != null)
            inputModule.enabled = true;

            Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        totalKills = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        totalKills = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}