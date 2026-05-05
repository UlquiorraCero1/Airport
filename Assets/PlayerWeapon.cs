using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [Header("References")]
    public Transform weaponHoldPoint;
    public LayerMask enemyLayer;
    public LayerMask wallLayer;
    public GameObject bloodPrefab;
    public GameObject bulletTracerPrefab;

    [Header("Current Weapon (read only)")]
    public WeaponData equippedWeapon;
    public int currentAmmo = 0;

    private float fireTimer = 0f;
    private WeaponPickup heldPickup;
    private PlayerCombat playerCombat;

    void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;

        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null || keyboard == null) return;

        if (equippedWeapon != null)
        {
            bool isMelee = equippedWeapon.weaponType == WeaponType.MeleeWeapon;
            bool isUzi   = equippedWeapon.weaponType == WeaponType.Uzi;

            if (!isMelee)
            {
                bool shouldShoot = isUzi
                    ? mouse.leftButton.isPressed
                    : mouse.leftButton.wasPressedThisFrame;

                if (shouldShoot)
                    Shoot();
            }
            else
            {
                if (mouse.leftButton.wasPressedThisFrame)
                    MeleeAttack();
            }

            if (keyboard.fKey.wasPressedThisFrame)
                ThrowWeapon();
        }
    }

    // EQUIP
    public void EquipWeapon(WeaponPickup pickup)
    {
        if (heldPickup != null)
            DropCurrentWeapon();

        equippedWeapon = pickup.weaponData;

        // Use saved ammo if available, otherwise full ammo
        currentAmmo = pickup.remainingAmmo >= 0
            ? pickup.remainingAmmo
            : equippedWeapon.ammo;

        heldPickup = pickup;

        pickup.transform.SetParent(weaponHoldPoint);
        pickup.transform.localPosition = Vector3.zero;
        pickup.transform.localRotation = Quaternion.identity;

        Collider col = pickup.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = pickup.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        pickup.OnEquipped();

        if (playerCombat != null)
            playerCombat.hasWeapon = true;

        if (equippedWeapon.weaponType == WeaponType.MeleeWeapon)
            GameUI.Instance?.ShowMeleeWeapon(equippedWeapon.weaponName);
        else
            GameUI.Instance?.UpdateAmmo(currentAmmo, equippedWeapon.ammo,
                equippedWeapon.weaponName);
    }

    // DROP
    void DropCurrentWeapon()
    {
        if (heldPickup == null) return;

        heldPickup.transform.SetParent(null);

        Collider col = heldPickup.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = heldPickup.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // Save current ammo so player can pick it back up
        heldPickup.OnDropped(currentAmmo);

        heldPickup = null;
        equippedWeapon = null;
        currentAmmo = 0;

        if (playerCombat != null)
            playerCombat.hasWeapon = false;

        GameUI.Instance?.ClearWeapon();
    }

    // SHOOT
    void Shoot()
    {
        if (fireTimer > 0f) return;

        if (currentAmmo <= 0)
        {
            DropCurrentWeapon();
            return;
        }

        fireTimer = equippedWeapon.fireRate;
        currentAmmo--;

        GameUI.Instance?.UpdateAmmo(currentAmmo, equippedWeapon.ammo,
            equippedWeapon.weaponName);

        for (int i = 0; i < equippedWeapon.pelletsPerShot; i++)
            FireRaycast();
    }

    void FireRaycast()
    {
        Vector3 shootDirection = transform.forward;

        if (equippedWeapon.spreadAngle > 0f)
        {
            shootDirection = Quaternion.Euler(0,
                Random.Range(-equippedWeapon.spreadAngle,
                              equippedWeapon.spreadAngle), 0) * shootDirection;
        }

        Vector3 origin   = transform.position + Vector3.up * 0.5f;
        Vector3 hitPoint = origin + shootDirection * equippedWeapon.range;

        RaycastHit[] allHits = Physics.RaycastAll(
            origin, shootDirection, equippedWeapon.range,
            ~0, QueryTriggerInteraction.Collide);

        foreach (RaycastHit h in allHits)
{
    // Check for boss first
    BossAI boss = h.collider.GetComponent<BossAI>();
    if (boss != null)
    {
        boss.TakeHit();
        SpawnBlood(h.point);
        hitPoint = h.point;
        break;
    }

    // Then check normal enemy
    EnemyHealth eh = h.collider.GetComponent<EnemyHealth>();
    if (eh != null)
    {
        eh.TakeShot();
        SpawnBlood(h.point);
        hitPoint = h.point;
        break;
    }
    }

        SpawnTracer(origin, hitPoint);
        AlertSystem.Instance?.ReportSound(transform.position, 20f);
    }

    void SpawnTracer(Vector3 from, Vector3 to)
    {
        if (bulletTracerPrefab == null) return;
        GameObject tracer = Instantiate(bulletTracerPrefab, from, Quaternion.identity);
        BulletTracer bt = tracer.GetComponent<BulletTracer>();
        if (bt != null)
            bt.Setup(from, to);
    }

    // MELEE WEAPON
    void MeleeAttack()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, equippedWeapon.meleeRange, enemyLayer);

        foreach (Collider hit in hits)
        {
            Vector3 dir   = (hit.transform.position - transform.position).normalized;
            float   angle = Vector3.Angle(transform.forward, dir);

            if (angle < equippedWeapon.meleeAngle / 2f)
            {
                EnemyHealth eh = hit.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    HeavyEnemy heavy = eh.GetComponent<HeavyEnemy>();
                    if (heavy != null)
                        heavy.TakeHeavyHit();
                    else if (eh.isKnockedDown)
                        eh.Execute();
                    else
                        eh.TakeHit();

                    SpawnBlood(hit.transform.position);
                }
            }
        }
    }

    // THROW 
    void ThrowWeapon()
    {
        if (heldPickup == null) return;

        WeaponPickup toThrow   = heldPickup;
        int          savedAmmo = currentAmmo;

        // Clear player hands immediately
        heldPickup      = null;
        equippedWeapon  = null;
        currentAmmo     = 0;

        if (playerCombat != null)
            playerCombat.hasWeapon = false;

        GameUI.Instance?.ClearWeapon();

        // Detach from player
        toThrow.transform.SetParent(null);

        Rigidbody rb = toThrow.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // Save remaining ammo so pickup restores it if picked up again
        toThrow.remainingAmmo = savedAmmo;

        // Launch it
        toThrow.SetThrown(transform.forward, 30f);
    }

    // HELPERS 
    void SpawnBlood(Vector3 position)
    {
        if (bloodPrefab == null) return;
        Vector3 pos = new Vector3(position.x, 0.02f, position.z);
        Instantiate(bloodPrefab, pos,
            Quaternion.Euler(90f, Random.Range(0f, 360f), 0f));
    }
}