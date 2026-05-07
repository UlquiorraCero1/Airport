using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    [Header("Reload Settings")]
    public bool isReloading = false;
    public float reloadProgress = 0f;

    [Header("Effects")]
    public bool enableMuzzleFlash = true;
    public bool enableShellCasings = true;
    public bool enableBulletImpacts = true;

    [Header("Melee Swing")]
    public float swingDuration = 0.15f;
    public float swingAngle = 90f;

    private float fireTimer = 0f;
    private float reloadTimer = 0f;
    private WeaponPickup heldPickup;
    private PlayerCombat playerCombat;
    private float recoilOffset = 0f;
    private float recoilRecoverySpeed = 15f;
    private bool isSwinging = false;

    void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();
        EnsureAmmoManager();
    }

    void EnsureAmmoManager()
    {
        if (AmmoManager.Instance == null)
        {
            AmmoManager.GetOrCreate();
        }
    }

    void Update()
    {
        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;

        if (recoilOffset > 0f)
            recoilOffset = Mathf.Lerp(recoilOffset, 0f, Time.deltaTime * recoilRecoverySpeed);

        UpdateReload();

        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null || keyboard == null) return;

        if (equippedWeapon != null)
        {
            bool isMelee = equippedWeapon.weaponType == WeaponType.MeleeWeapon;
            bool isUzi = equippedWeapon.weaponType == WeaponType.Uzi;

            if (!isMelee && !isReloading)
            {
                bool shouldShoot = isUzi
                    ? mouse.leftButton.isPressed
                    : mouse.leftButton.wasPressedThisFrame;

                if (shouldShoot)
                    Shoot();

                if (keyboard.rKey.wasPressedThisFrame)
                    TryReload();
            }
            else if (isMelee)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                    MeleeAttack();
            }

            if (keyboard.fKey.wasPressedThisFrame && !isReloading)
                ThrowWeapon();
        }
    }

    void UpdateReload()
    {
        if (!isReloading) return;

        float reloadTime = equippedWeapon.reloadTime > 0 ? equippedWeapon.reloadTime : 1f;
        reloadTimer -= Time.deltaTime;
        reloadProgress = 1f - (reloadTimer / reloadTime);

        if (reloadTimer <= 0f)
            FinishReload();
    }

    void TryReload()
    {
        if (equippedWeapon == null) return;
        if (!equippedWeapon.canReload) return;
        if (equippedWeapon.weaponType == WeaponType.MeleeWeapon) return;
        if (isReloading) return;

        int magSize = equippedWeapon.magazineSize > 0 ? equippedWeapon.magazineSize : equippedWeapon.ammo;
        if (currentAmmo >= magSize) return;

        EnsureAmmoManager();

        if (AmmoManager.Instance == null)
        {
            return;
        }

        AmmoType ammoNeeded = GetAmmoTypeForWeapon();
        if (!AmmoManager.Instance.HasAmmo(ammoNeeded))
        {
            return;
        }

        StartReload();
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = equippedWeapon.reloadTime > 0 ? equippedWeapon.reloadTime : 1f;
        reloadProgress = 0f;

        GameUI.Instance?.ShowReloading(equippedWeapon.weaponName);
    }

    void FinishReload()
    {
        isReloading = false;

        int magSize = equippedWeapon.magazineSize > 0 ? equippedWeapon.magazineSize : equippedWeapon.ammo;
        int ammoNeeded = magSize - currentAmmo;
        AmmoType ammoType = GetAmmoTypeForWeapon();

        int ammoGot = AmmoManager.Instance != null
            ? AmmoManager.Instance.TakeAmmo(ammoType, ammoNeeded)
            : 0;

        currentAmmo += ammoGot;

        GameUI.Instance?.UpdateAmmo(currentAmmo, magSize,
            equippedWeapon.weaponName, GetReserveAmmo());
    }

    AmmoType GetAmmoTypeForWeapon()
    {
        if (equippedWeapon == null) return AmmoType.None;

        if (equippedWeapon.ammoType != AmmoType.None)
            return equippedWeapon.ammoType;

        switch (equippedWeapon.weaponType)
        {
            case WeaponType.Pistol: return AmmoType.Pistol;
            case WeaponType.Shotgun: return AmmoType.Shotgun;
            case WeaponType.Uzi: return AmmoType.Uzi;
            case WeaponType.MeleeWeapon: return AmmoType.None;
            default: return AmmoType.Pistol;
        }
    }

    int GetReserveAmmo()
    {
        if (equippedWeapon == null) return 0;
        if (AmmoManager.Instance == null) return 0;
        return AmmoManager.Instance.GetReserveAmmo(GetAmmoTypeForWeapon());
    }

    public void EquipWeapon(WeaponPickup pickup)
    {
        if (heldPickup != null)
            DropCurrentWeapon();

        equippedWeapon = pickup.weaponData;

        int magSize = equippedWeapon.magazineSize > 0 ? equippedWeapon.magazineSize : equippedWeapon.ammo;
        currentAmmo = pickup.remainingAmmo >= 0
            ? pickup.remainingAmmo
            : magSize;

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

        isReloading = false;
        reloadProgress = 0f;

        EnsureAmmoManager();

        if (equippedWeapon.weaponType == WeaponType.MeleeWeapon)
            GameUI.Instance?.ShowMeleeWeapon(equippedWeapon.weaponName);
        else
            GameUI.Instance?.UpdateAmmo(currentAmmo, magSize,
                equippedWeapon.weaponName, GetReserveAmmo());
    }

    void DropCurrentWeapon()
    {
        if (heldPickup == null) return;

        isReloading = false;

        heldPickup.transform.SetParent(null);

        Collider col = heldPickup.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = heldPickup.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        heldPickup.OnDropped(currentAmmo);

        heldPickup = null;
        equippedWeapon = null;
        currentAmmo = 0;

        if (playerCombat != null)
            playerCombat.hasWeapon = false;

        GameUI.Instance?.ClearWeapon();
    }

    void Shoot()
    {
        if (fireTimer > 0f) return;

        AmmoType ammoType = GetAmmoTypeForWeapon();

        if (currentAmmo <= 0)
        {
            if (AmmoManager.Instance != null && AmmoManager.Instance.HasAmmo(ammoType))
            {
                TryReload();
            }
            else
            {
                DropCurrentWeapon();
            }
            return;
        }

        fireTimer = equippedWeapon.fireRate;
        currentAmmo--;

        int magSize = equippedWeapon.magazineSize > 0 ? equippedWeapon.magazineSize : equippedWeapon.ammo;
        GameUI.Instance?.UpdateAmmo(currentAmmo, magSize,
            equippedWeapon.weaponName, GetReserveAmmo());

        ApplyRecoil();
        SpawnMuzzleFlash();
        SpawnShellCasing();

        for (int i = 0; i < equippedWeapon.pelletsPerShot; i++)
            FireRaycast();

        ScreenShake.Instance?.Shake(0.05f, equippedWeapon.screenShakeOnFire);

        if (currentAmmo <= 0 && AmmoManager.Instance != null && AmmoManager.Instance.HasAmmo(ammoType))
        {
            TryReload();
        }
    }

    void ApplyRecoil()
    {
        recoilOffset += equippedWeapon.recoilStrength;
        recoilOffset = Mathf.Clamp(recoilOffset, 0f, 0.3f);

        if (weaponHoldPoint != null)
        {
            Vector3 localPos = weaponHoldPoint.localPosition;
            localPos.z = -recoilOffset;
            weaponHoldPoint.localPosition = localPos;
        }
    }

    void SpawnMuzzleFlash()
    {
        if (!enableMuzzleFlash) return;

        Vector3 muzzlePos = transform.position + transform.forward * 0.8f + Vector3.up * 0.5f;
        MuzzleFlash.Create(muzzlePos, equippedWeapon.muzzleFlashSize);
    }

    void SpawnShellCasing()
    {
        if (!enableShellCasings) return;

        Vector3 ejectPos = transform.position + transform.right * 0.3f + Vector3.up * 0.5f;
        Vector3 ejectDir = transform.right;
        ShellCasing.Create(ejectPos, ejectDir, equippedWeapon.shellEjectForce, GetAmmoTypeForWeapon());
    }

    void FireRaycast()
    {
        Vector3 shootDirection = transform.forward;

        if (equippedWeapon.spreadAngle > 0f)
        {
            float spreadX = Random.Range(-equippedWeapon.spreadAngle, equippedWeapon.spreadAngle);
            float spreadY = Random.Range(-equippedWeapon.spreadAngle * 0.3f, equippedWeapon.spreadAngle * 0.3f);
            shootDirection = Quaternion.Euler(spreadY, spreadX, 0) * shootDirection;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 hitPoint = origin + shootDirection * equippedWeapon.range;
        Vector3 hitNormal = -shootDirection;
        bool hitEnemy = false;
        bool hitWall = false;

        RaycastHit[] allHits = Physics.RaycastAll(
            origin, shootDirection, equippedWeapon.range,
            ~0, QueryTriggerInteraction.Collide);

        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit h in allHits)
        {
            if (h.collider.CompareTag("Player")) continue;

            BossAI boss = h.collider.GetComponent<BossAI>();
            if (boss != null)
            {
                boss.TakeHit();
                SpawnBlood(h.point);
                HitMarker.Create(h.point, false);
                hitPoint = h.point;
                hitNormal = h.normal;
                hitEnemy = true;
                break;
            }

            EnemyHealth eh = h.collider.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.SetLastHitDirection(shootDirection);
                eh.TakeShotFromDirection(shootDirection);
                SpawnBlood(h.point);
                HitMarker.Create(h.point, true);
                hitPoint = h.point;
                hitNormal = h.normal;
                hitEnemy = true;
                break;
            }

            if ((wallLayer.value & (1 << h.collider.gameObject.layer)) != 0)
            {
                hitPoint = h.point;
                hitNormal = h.normal;
                hitWall = true;
                break;
            }

            if (!h.collider.isTrigger)
            {
                hitPoint = h.point;
                hitNormal = h.normal;
                hitWall = true;
                break;
            }
        }

        SpawnTracer(origin, hitPoint);

        if (enableBulletImpacts && (hitWall || hitEnemy))
        {
            BulletImpact.Create(hitPoint, hitNormal, hitWall && !hitEnemy);
        }

        AlertSystem.Instance?.ReportSound(transform.position, equippedWeapon.soundRadius);
    }

    void SpawnTracer(Vector3 from, Vector3 to)
    {
        if (bulletTracerPrefab == null) return;
        GameObject tracer = Instantiate(bulletTracerPrefab, from, Quaternion.identity);
        BulletTracer bt = tracer.GetComponent<BulletTracer>();
        if (bt != null)
            bt.Setup(from, to);
    }

    void MeleeAttack()
    {
        if (isSwinging) return;

        StartCoroutine(SwingWeapon());

        Collider[] hits = Physics.OverlapSphere(
            transform.position, equippedWeapon.meleeRange, enemyLayer);

        foreach (Collider hit in hits)
        {
            Vector3 dir = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);

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

        ScreenShake.Instance?.Shake(0.08f, 0.2f);
    }

    IEnumerator SwingWeapon()
    {
        if (weaponHoldPoint == null) yield break;

        isSwinging = true;

        Quaternion startRot = weaponHoldPoint.localRotation;
        Quaternion windUp = Quaternion.Euler(0f, -30f, 0f);
        Quaternion swingEnd = Quaternion.Euler(0f, swingAngle, 0f);

        float windUpTime = swingDuration * 0.2f;
        float elapsed = 0f;
        while (elapsed < windUpTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windUpTime;
            weaponHoldPoint.localRotation = Quaternion.Slerp(startRot, windUp, t);
            yield return null;
        }

        elapsed = 0f;
        float swingTime = swingDuration * 0.8f;
        while (elapsed < swingTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swingTime;
            t = 1f - (1f - t) * (1f - t);
            weaponHoldPoint.localRotation = Quaternion.Slerp(windUp, swingEnd, t);
            yield return null;
        }

        elapsed = 0f;
        float returnTime = swingDuration * 0.5f;
        while (elapsed < returnTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnTime;
            weaponHoldPoint.localRotation = Quaternion.Slerp(swingEnd, startRot, t);
            yield return null;
        }

        weaponHoldPoint.localRotation = startRot;
        isSwinging = false;
    }

    void ThrowWeapon()
    {
        if (heldPickup == null) return;

        WeaponPickup toThrow = heldPickup;
        int savedAmmo = currentAmmo;

        heldPickup = null;
        equippedWeapon = null;
        currentAmmo = 0;
        isReloading = false;

        if (playerCombat != null)
            playerCombat.hasWeapon = false;

        GameUI.Instance?.ClearWeapon();

        toThrow.transform.SetParent(null);

        Rigidbody rb = toThrow.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        toThrow.remainingAmmo = savedAmmo;

        toThrow.SetThrown(transform.forward, 30f);
    }

    void SpawnBlood(Vector3 position)
    {
        if (bloodPrefab == null) return;
        Vector3 pos = new Vector3(position.x, 0.02f, position.z);
        Instantiate(bloodPrefab, pos,
            Quaternion.Euler(90f, Random.Range(0f, 360f), 0f));
    }

    public bool HasAmmoForCurrentWeapon()
    {
        if (equippedWeapon == null) return false;
        if (currentAmmo > 0) return true;
        if (AmmoManager.Instance == null) return false;
        return AmmoManager.Instance.HasAmmo(equippedWeapon.ammoType);
    }
}
