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

        // Hide everything at start
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

    // Called when player picks up a weapon
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

    // Called when player drops or runs out of ammo
    public void ClearWeapon()
    {
        if (ammoText != null)       ammoText.gameObject.SetActive(false);
        if (weaponNameText != null) weaponNameText.gameObject.SetActive(false);
    }

    // Called on every kill
    public void RegisterKill()
    {
        killCombo++;
        comboTimer = comboWindow;

        if (comboText != null)
        {
            if (killCombo >= 2)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = "x" + killCombo + " COMBO!";
                comboText.color = killCombo >= 5 ? Color.red : Color.yellow;
            }
        }

        ScreenShake.Instance?.Shake(0.12f, 0.25f);
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