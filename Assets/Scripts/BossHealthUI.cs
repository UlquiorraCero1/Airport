using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance;

    public Slider healthSlider;
    public GameObject healthBarPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Force hide immediately in Awake not Start
        if (healthBarPanel != null)
            healthBarPanel.SetActive(false);
    }

    void Start()
    {
        // Double make sure it's hidden
        if (healthBarPanel != null)
            healthBarPanel.SetActive(false);
    }

    public void ShowBossHealth(int maxHits)
    {
        if (healthBarPanel != null)
            healthBarPanel.SetActive(true);

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHits;
            healthSlider.value = maxHits;
        }
    }

    public void UpdateBossHealth(int hitsRemaining)
    {
        if (healthSlider != null)
            healthSlider.value = hitsRemaining;
    }

    public void HideBossHealth()
    {
        if (healthBarPanel != null)
            healthBarPanel.SetActive(false);
    }
}