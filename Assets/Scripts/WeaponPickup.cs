using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon")]
    public WeaponData weaponData;

    [HideInInspector]
    public int remainingAmmo = -1;

    [Header("Visual")]
    public float bobSpeed = 1.5f;
    public float bobHeight = 0.1f;
    public bool enableBobbing = true;

    private bool isEquipped = false;
    private bool isThrown = false;
    private bool hasHit = false;
    private float pickupDelay = 0f;
    private Rigidbody rb;
    private Collider col;
    private float groundY = 0f;
    private GameObject player;
    private Vector3 startPosition;
    private float bobTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        groundY = transform.position.y;
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player");
        bobTimer = Random.Range(0f, Mathf.PI * 2f);

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

        if (!isEquipped && !isThrown && enableBobbing)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float yOffset = Mathf.Sin(bobTimer) * bobHeight;
            Vector3 pos = transform.position;
            pos.y = groundY + yOffset;
            transform.position = pos;
        }
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

                Vector3 hitDir = (hit.transform.position - transform.position).normalized;
                eh.SetLastHitDirection(hitDir);
                eh.TakeShot();

                HitMarker.Create(hit.transform.position, true);
                return;
            }

            BossAI boss = hit.GetComponent<BossAI>();
            if (boss != null)
            {
                StopThrown();
                boss.TakeHit();
                HitMarker.Create(hit.transform.position, false);
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
        startPosition = pos;
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

        PickupEffect.CreateWeaponPickup(transform.position);
        ScreenShake.Instance?.Shake(0.05f, 0.1f);
    }

    public void OnDropped(int currentAmmo)
    {
        isEquipped = false;
        pickupDelay = 0.5f;

        remainingAmmo = currentAmmo;

        if (col != null)
            col.isTrigger = true;

        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
        startPosition = pos;

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
