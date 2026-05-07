using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    public string weaponName = "Pistol";
    public WeaponType weaponType = WeaponType.Pistol;
    public AmmoType ammoType = AmmoType.Pistol;

    [Header("Magazine")]
    public int magazineSize = 7;
    public float reloadTime = 1.2f;
    public bool canReload = true;

    [Header("Shooting")]
    public int ammo = 7;
    public float fireRate = 0.3f;
    public float range = 30f;
    public int pelletsPerShot = 1;
    public float spreadAngle = 0f;

    [Header("Recoil & Feel")]
    public float recoilStrength = 0.3f;
    public float muzzleFlashSize = 1f;
    public float shellEjectForce = 3f;
    public float screenShakeOnFire = 0.08f;

    [Header("Audio")]
    public float soundRadius = 20f;

    [Header("Melee Weapon")]
    public float meleeRange = 2.5f;
    public float meleeAngle = 120f;

    [Header("Throw")]
    public float throwForce = 15f;
    public float throwDamageRange = 1f;
}

public enum WeaponType
{
    Pistol,
    Shotgun,
    Uzi,
    MeleeWeapon
}

public enum AmmoType
{
    None,
    Pistol,
    Shotgun,
    Uzi
}