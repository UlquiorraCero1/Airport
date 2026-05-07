using UnityEngine;
using System;
using System.Collections.Generic;

public class AmmoManager : MonoBehaviour
{
    public static AmmoManager Instance;

    [Header("Starting Ammo")]
    public int startingPistolAmmo = 0;
    public int startingShotgunAmmo = 0;
    public int startingUziAmmo = 0;

    [Header("Max Ammo")]
    public int maxPistolAmmo = 98;
    public int maxShotgunAmmo = 32;
    public int maxUziAmmo = 300;

    private Dictionary<AmmoType, int> reserveAmmo = new Dictionary<AmmoType, int>();
    private Dictionary<AmmoType, int> maxAmmo = new Dictionary<AmmoType, int>();

    public event Action OnAmmoChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAmmo();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAmmo()
    {
        reserveAmmo.Clear();
        maxAmmo.Clear();

        reserveAmmo[AmmoType.None] = 0;
        reserveAmmo[AmmoType.Pistol] = startingPistolAmmo;
        reserveAmmo[AmmoType.Shotgun] = startingShotgunAmmo;
        reserveAmmo[AmmoType.Uzi] = startingUziAmmo;

        maxAmmo[AmmoType.None] = 0;
        maxAmmo[AmmoType.Pistol] = maxPistolAmmo;
        maxAmmo[AmmoType.Shotgun] = maxShotgunAmmo;
        maxAmmo[AmmoType.Uzi] = maxUziAmmo;
    }

    public int GetReserveAmmo(AmmoType type)
    {
        if (reserveAmmo.ContainsKey(type))
            return reserveAmmo[type];
        return 0;
    }

    public int GetMaxAmmo(AmmoType type)
    {
        if (maxAmmo.ContainsKey(type))
            return maxAmmo[type];
        return 999;
    }

    public void AddAmmo(AmmoType type, int amount)
    {
        if (type == AmmoType.None) return;
        if (!reserveAmmo.ContainsKey(type)) reserveAmmo[type] = 0;
        if (!maxAmmo.ContainsKey(type)) maxAmmo[type] = 999;

        int before = reserveAmmo[type];
        reserveAmmo[type] = Mathf.Min(reserveAmmo[type] + amount, maxAmmo[type]);
        OnAmmoChanged?.Invoke();
    }

    public int TakeAmmo(AmmoType type, int amountNeeded)
    {
        if (type == AmmoType.None) return 0;
        if (!reserveAmmo.ContainsKey(type)) return 0;

        int taken = Mathf.Min(reserveAmmo[type], amountNeeded);
        reserveAmmo[type] -= taken;
        OnAmmoChanged?.Invoke();
        return taken;
    }

    public bool HasAmmo(AmmoType type)
    {
        if (type == AmmoType.None) return false;
        return reserveAmmo.ContainsKey(type) && reserveAmmo[type] > 0;
    }

    public void ResetAmmo()
    {
        InitializeAmmo();
        OnAmmoChanged?.Invoke();
    }

    public static AmmoManager GetOrCreate()
    {
        if (Instance != null) return Instance;

        GameObject go = new GameObject("AmmoManager");
        AmmoManager am = go.AddComponent<AmmoManager>();
        return am;
    }
}
