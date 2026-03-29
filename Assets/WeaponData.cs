using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    public string weaponName = "Pistol";
    public WeaponType weaponType = WeaponType.Pistol;

    [Header("Shooting")]
    public int ammo = 7;              // How many shots before empty
    public float fireRate = 0.3f;     // Seconds between shots
    public float range = 30f;         // How far the bullet travels
    public int pelletsPerShot = 1;    // Shotgun uses more than 1
    public float spreadAngle = 0f;    // Bullet spread (shotgun = ~15)

    [Header("Melee Weapon")]
    public float meleeRange = 2.5f;   // Wider than fist
    public float meleeAngle = 120f;

    [Header("Throw")]
    public float throwForce = 15f;
    public float throwDamageRange = 1f; // How close it needs to land to kill
}

public enum WeaponType
{
    Pistol,
    Shotgun,
    Uzi,
    MeleeWeapon  // bat, pipe etc
}