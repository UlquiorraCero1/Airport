using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("UI References")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI weaponNameText;

    private int killCombo = 0;
    private float comboTimer = 0f;
    private float comboWindow = 3f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        HideAll();
    }

    void HideAll()
    {
        if (ammoText != null)       ammoText.gameObject.SetActive(false);
        if (weaponNameText != null) weaponNameText.gameObject.SetActive(false);
        if (comboText != null)      comboText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (killCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }
    }

    // For guns — show ammo and name
    public void UpdateAmmo(int current, int max, string weaponName)
    {
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(true);
            ammoText.text = current + " / " + max;
        }
        if (weaponNameText != null)
        {
            weaponNameText.gameObject.SetActive(true);
            weaponNameText.text = weaponName.ToUpper();
        }
    }

    // For melee — show name only, hide ammo
    public void ShowMeleeWeapon(string weaponName)
    {
        if (ammoText != null)
            ammoText.gameObject.SetActive(false);

        if (weaponNameText != null)
        {
            weaponNameText.gameObject.SetActive(true);
            weaponNameText.text = weaponName.ToUpper();
        }
    }

    public void ClearWeapon()
    {
        if (ammoText != null)       ammoText.gameObject.SetActive(false);
        if (weaponNameText != null) weaponNameText.gameObject.SetActive(false);
    }

    public void RegisterKill()
    {
        killCombo++;
        comboTimer = comboWindow;
        GameOverScreen.AddKill();

        if (comboText != null)
        {
            if (killCombo >= 2)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = "x" + killCombo + " COMBO!";
                comboText.color = killCombo >= 5 ? Color.red : Color.yellow;
            }
        }

        ScreenShake.Instance?.Shake(0.15f, 0.5f);
        SlowMotion.Instance?.TriggerKillEffect();
    }

    void ResetCombo()
    {
        killCombo = 0;
        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
            comboText.text = "";
        }
    }
}