using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("UI References")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI reserveAmmoText;
    public Image reloadBar;
    public Image reloadBarBackground;

    [Header("Ammo Display Colors")]
    public Color normalAmmoColor = Color.white;
    public Color lowAmmoColor = new Color(1f, 0.5f, 0.3f);
    public Color emptyAmmoColor = new Color(1f, 0.2f, 0.2f);
    public Color reloadingColor = new Color(0.5f, 0.8f, 1f);

    private int killCombo = 0;
    private float comboTimer = 0f;
    private float comboWindow = 3f;
    private PlayerWeapon playerWeapon;
    private bool isShowingReload = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        HideAll();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerWeapon = player.GetComponent<PlayerWeapon>();

        if (AmmoManager.Instance != null)
            AmmoManager.Instance.OnAmmoChanged += OnAmmoChanged;
    }

    void OnDestroy()
    {
        if (AmmoManager.Instance != null)
            AmmoManager.Instance.OnAmmoChanged -= OnAmmoChanged;
    }

    void HideAll()
    {
        if (ammoText != null) ammoText.gameObject.SetActive(false);
        if (weaponNameText != null) weaponNameText.gameObject.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false);
        if (reserveAmmoText != null) reserveAmmoText.gameObject.SetActive(false);
        if (reloadBar != null) reloadBar.gameObject.SetActive(false);
        if (reloadBarBackground != null) reloadBarBackground.gameObject.SetActive(false);
    }

    void Update()
    {
        if (killCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }

        UpdateReloadBar();
    }

    void UpdateReloadBar()
    {
        if (playerWeapon == null) return;

        if (playerWeapon.isReloading)
        {
            if (!isShowingReload)
            {
                isShowingReload = true;
                if (reloadBar != null) reloadBar.gameObject.SetActive(true);
                if (reloadBarBackground != null) reloadBarBackground.gameObject.SetActive(true);
            }

            if (reloadBar != null)
                reloadBar.fillAmount = playerWeapon.reloadProgress;
        }
        else if (isShowingReload)
        {
            isShowingReload = false;
            if (reloadBar != null) reloadBar.gameObject.SetActive(false);
            if (reloadBarBackground != null) reloadBarBackground.gameObject.SetActive(false);
        }
    }

    void OnAmmoChanged()
    {
        if (playerWeapon != null && playerWeapon.equippedWeapon != null)
        {
            int reserve = AmmoManager.Instance.GetReserveAmmo(playerWeapon.equippedWeapon.ammoType);
            UpdateReserveAmmo(reserve);
        }
    }

    public void UpdateAmmo(int current, int max, string weaponName, int reserve = -1)
    {
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(true);
            ammoText.text = current + " / " + max;

            float ratio = (float)current / max;
            if (current == 0)
                ammoText.color = emptyAmmoColor;
            else if (ratio <= 0.25f)
                ammoText.color = lowAmmoColor;
            else
                ammoText.color = normalAmmoColor;
        }

        if (weaponNameText != null)
        {
            weaponNameText.gameObject.SetActive(true);
            weaponNameText.text = weaponName.ToUpper();
        }

        if (reserve >= 0)
            UpdateReserveAmmo(reserve);
    }

    void UpdateReserveAmmo(int reserve)
    {
        if (reserveAmmoText != null)
        {
            reserveAmmoText.gameObject.SetActive(true);
            reserveAmmoText.text = "[" + reserve + "]";

            if (reserve == 0)
                reserveAmmoText.color = emptyAmmoColor;
            else
                reserveAmmoText.color = new Color(0.7f, 0.7f, 0.7f);
        }
    }

    public void ShowReloading(string weaponName)
    {
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(true);
            ammoText.text = "RELOADING";
            ammoText.color = reloadingColor;
        }

        if (weaponNameText != null)
        {
            weaponNameText.gameObject.SetActive(true);
            weaponNameText.text = weaponName.ToUpper();
        }
    }

    public void ShowMeleeWeapon(string weaponName)
    {
        if (ammoText != null)
            ammoText.gameObject.SetActive(false);

        if (reserveAmmoText != null)
            reserveAmmoText.gameObject.SetActive(false);

        if (weaponNameText != null)
        {
            weaponNameText.gameObject.SetActive(true);
            weaponNameText.text = weaponName.ToUpper();
        }
    }

    public void ClearWeapon()
    {
        if (ammoText != null) ammoText.gameObject.SetActive(false);
        if (weaponNameText != null) weaponNameText.gameObject.SetActive(false);
        if (reserveAmmoText != null) reserveAmmoText.gameObject.SetActive(false);
        if (reloadBar != null) reloadBar.gameObject.SetActive(false);
        if (reloadBarBackground != null) reloadBarBackground.gameObject.SetActive(false);
        isShowingReload = false;
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

        float shakeIntensity = Mathf.Min(0.15f + (killCombo * 0.05f), 0.5f);
        ScreenShake.Instance?.Shake(0.15f, shakeIntensity);
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

    public int GetCurrentCombo()
    {
        return killCombo;
    }
}
