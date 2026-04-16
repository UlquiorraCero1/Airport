using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon")]
    public WeaponData weaponData;

    // Remembers remaining ammo when dropped
    [HideInInspector]
    public int remainingAmmo = -1; 

    private bool isEquipped = false;
    private bool isThrown = false;
    private bool hasHit = false;
    private float pickupDelay = 0f;
    private Rigidbody rb;
    private Collider col;
    private float groundY = 0f;
    private GameObject player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        groundY = transform.position.y;
        player = GameObject.FindGameObjectWithTag("Player");

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void Update()
    {
        if (pickupDelay > 0f)
            pickupDelay -= Time.deltaTime;

        if (isThrown && !hasHit)
            CheckThrowHit();
    }

    void CheckThrowHit()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, 1f,
            LayerMask.GetMask("Enemy"));

        foreach (Collider hit in hits)
        {
            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                StopThrown();
                eh.TakeShot();
                return;
            }
        }
    }

    void StopThrown()
    {
        hasHit = true;
        isThrown = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;
        }

        if (player != null)
        {
            Collider playerCol = player.GetComponent<Collider>();
            if (playerCol != null && col != null)
                Physics.IgnoreCollision(col, playerCol, false);
        }

        if (col != null)
            col.isTrigger = true;

        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
    }

    public void SetThrown(Vector3 direction, float force)
    {
        isThrown = true;
        isEquipped = false;
        hasHit = false;
        pickupDelay = 1f;

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;

            if (player != null)
            {
                Collider playerCol = player.GetComponent<Collider>();
                if (playerCol != null)
                    Physics.IgnoreCollision(col, playerCol, true);
            }
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;
            rb.linearVelocity = direction * force;
        }
    }

    public void OnEquipped()
    {
        isEquipped = true;
        isThrown = false;
    }

    public void OnDropped(int currentAmmo)
    {
        isEquipped = false;
        pickupDelay = 0.5f;

        // Save the ammo so it's not reset on pickup
        remainingAmmo = currentAmmo;

        if (col != null)
            col.isTrigger = true;

        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isEquipped || pickupDelay > 0f) return;
        if (isThrown) return;

        if (other.CompareTag("Player"))
        {
            PlayerWeapon pw = other.GetComponent<PlayerWeapon>();
            if (pw != null)
                pw.EquipWeapon(this);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isThrown) return;
        if (collision.gameObject.CompareTag("Player")) return;
        StopThrown();
    }
}