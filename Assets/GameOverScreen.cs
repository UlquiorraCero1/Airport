using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;
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

        // Pause the game
        Time.timeScale = 0f;
    }

    // Called by Retry button
    public void Retry()
    {
        totalKills = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Called by Main Menu button
    public void GoToMainMenu()
    {
        totalKills = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}