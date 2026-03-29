using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [Header("References")]
    public Transform weaponHoldPoint;
    public LayerMask enemyLayer;
    public LayerMask wallLayer;
    public GameObject bloodPrefab;

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

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (equippedWeapon == null) return;

            if (equippedWeapon.weaponType == WeaponType.MeleeWeapon)
                MeleeAttack();
            else
                Shoot();
        }

        if (keyboard.fKey.wasPressedThisFrame && equippedWeapon != null)
            ThrowWeapon();
    }

    // ─── EQUIP ────────────────────────────────────────────
    public void EquipWeapon(WeaponPickup pickup)
{
    if (heldPickup != null)
        DropCurrentWeapon();

    equippedWeapon = pickup.weaponData;
    currentAmmo = equippedWeapon.ammo;
    heldPickup = pickup;

    pickup.transform.SetParent(weaponHoldPoint);
    pickup.transform.localPosition = Vector3.zero;
    pickup.transform.localRotation = Quaternion.identity;

    // Disable collider and physics while held
    Collider col = pickup.GetComponent<Collider>();
    if (col != null) col.enabled = false;

    Rigidbody rb = pickup.GetComponent<Rigidbody>();
    if (rb != null) rb.isKinematic = true;

    // Mark as equipped so it won't re-trigger
    pickup.OnEquipped();

    if (playerCombat != null)
        playerCombat.hasWeapon = true;

    GameUI.Instance?.UpdateAmmo(currentAmmo, equippedWeapon.ammo, equippedWeapon.weaponName);
}

void DropCurrentWeapon()
{
    if (heldPickup == null) return;

    heldPickup.transform.SetParent(null);

    Collider col = heldPickup.GetComponent<Collider>();
    if (col != null) col.enabled = true;

    Rigidbody rb = heldPickup.GetComponent<Rigidbody>();
    if (rb != null) rb.isKinematic = false;

    // Mark as dropped with delay so player doesn't instantly re-pickup
    heldPickup.OnDropped();

    heldPickup = null;
    equippedWeapon = null;
    currentAmmo = 0;

    if (playerCombat != null)
        playerCombat.hasWeapon = false;

    GameUI.Instance?.ClearWeapon();
}

    // ─── SHOOT ────────────────────────────────────────────
    void Shoot()
    {
        if (fireTimer > 0f) return;

        if (currentAmmo <= 0)
        {
            DropCurrentWeapon();
            Debug.Log("Out of ammo!");
            return;
        }

        fireTimer = equippedWeapon.fireRate;
        currentAmmo--;

        // Update ammo UI
        GameUI.Instance?.UpdateAmmo(currentAmmo, equippedWeapon.ammo, equippedWeapon.weaponName);

        for (int i = 0; i < equippedWeapon.pelletsPerShot; i++)
            FireRaycast();
    }

    void FireRaycast()
{
    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
    Vector3 shootDirection = transform.forward;

    if (groundPlane.Raycast(ray, out float dist))
    {
        Vector3 mouseWorld = ray.GetPoint(dist);
        shootDirection = (mouseWorld - transform.position).normalized;
        shootDirection.y = 0;
    }

    if (equippedWeapon.spreadAngle > 0f)
    {
        float spread = equippedWeapon.spreadAngle;
        shootDirection = Quaternion.Euler(0,
            Random.Range(-spread, spread), 0) * shootDirection;
    }

    Vector3 origin = transform.position + Vector3.up * 0.5f;

    // Cast with NO layer mask — hits absolutely everything
    RaycastHit[] allHits = Physics.RaycastAll(origin, shootDirection, equippedWeapon.range, ~0, QueryTriggerInteraction.Collide);

    if (allHits.Length == 0)
    {
        Debug.Log("Ray hit NOTHING at all");
    }
    else
    {
        foreach (RaycastHit h in allHits)
        {
            Debug.Log("Ray hit: " + h.collider.gameObject.name 
                + " | Layer: " + h.collider.gameObject.layer
                + " | IsTrigger: " + h.collider.isTrigger
                + " | HasEnemyHealth: " + (h.collider.GetComponent<EnemyHealth>() != null));

            EnemyHealth eh = h.collider.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeShot();
                SpawnBlood(h.point);
                break;
            }
        }
    }

    AlertSystem.Instance?.ReportSound(transform.position, 20f);
    Debug.DrawRay(origin, shootDirection * equippedWeapon.range, Color.red, 1f);
}

    // ─── MELEE WEAPON ─────────────────────────────────────
    void MeleeAttack()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            equippedWeapon.meleeRange,
            enemyLayer);

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
    }

    // ─── THROW ────────────────────────────────────────────
    void ThrowWeapon()
{
    if (heldPickup == null) return;

    heldPickup.transform.SetParent(null);

    Rigidbody rb = heldPickup.GetComponent<Rigidbody>();
    if (rb != null) rb.isKinematic = false;

    Collider col = heldPickup.GetComponent<Collider>();
    if (col != null) col.enabled = true;

    Vector3 throwDir = transform.forward;

    // Throw with high force
    heldPickup.SetThrown(throwDir, 30f);

    heldPickup = null;
    equippedWeapon = null;
    currentAmmo = 0;

    if (playerCombat != null)
        playerCombat.hasWeapon = false;

    GameUI.Instance?.ClearWeapon();
}

    void SpawnBlood(Vector3 position)
    {
        if (bloodPrefab == null) return;
        Vector3 pos = new Vector3(position.x, 0.02f, position.z);
        Instantiate(bloodPrefab, pos,
            Quaternion.Euler(90f, Random.Range(0f, 360f), 0f));
    }
}