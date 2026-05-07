using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Ammo Settings")]
    public AmmoType ammoType = AmmoType.Pistol;
    public int ammoAmount = 7;
    public bool isRandomAmmo = false;

    [Header("Visual")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;
    public float rotateSpeed = 90f;

    [Header("Pickup Effect")]
    public float pickupRadius = 1.2f;
    public bool autoPickup = true;

    private Vector3 startPosition;
    private float bobTimer = 0f;
    private bool isPickedUp = false;
    private MeshRenderer meshRenderer;

    void Start()
    {
        startPosition = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        bobTimer = Random.Range(0f, Mathf.PI * 2f);

        if (isRandomAmmo)
            RandomizeAmmoType();

        UpdateVisual();

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
        if (isPickedUp) return;

        bobTimer += Time.deltaTime * bobSpeed;
        float yOffset = Mathf.Sin(bobTimer) * bobHeight;
        transform.position = startPosition + Vector3.up * yOffset;

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        if (autoPickup)
            CheckForPlayer();
    }

    void CheckForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                TryPickup(hit.gameObject);
                return;
            }
        }
    }

    void TryPickup(GameObject player)
    {
        if (isPickedUp) return;

        EnsureAmmoManager();

        if (AmmoManager.Instance == null)
        {
            isPickedUp = true;
            SpawnPickupEffect();
            Destroy(gameObject);
            return;
        }

        int maxAmmo = AmmoManager.Instance.GetMaxAmmo(ammoType);
        int currentAmmo = AmmoManager.Instance.GetReserveAmmo(ammoType);

        if (currentAmmo >= maxAmmo)
            return;

        isPickedUp = true;
        AmmoManager.Instance.AddAmmo(ammoType, ammoAmount);

        SpawnPickupEffect();
        Destroy(gameObject);
    }

    void SpawnPickupEffect()
    {
        ScreenShake.Instance?.Shake(0.05f, 0.1f);
        PickupEffect.CreateAmmoPickup(transform.position);
    }

    void RandomizeAmmoType()
    {
        int rand = Random.Range(0, 3);
        switch (rand)
        {
            case 0:
                ammoType = AmmoType.Pistol;
                ammoAmount = Random.Range(5, 10);
                break;
            case 1:
                ammoType = AmmoType.Shotgun;
                ammoAmount = Random.Range(2, 5);
                break;
            case 2:
                ammoType = AmmoType.Uzi;
                ammoAmount = Random.Range(15, 30);
                break;
        }
    }

    void UpdateVisual()
    {
        if (meshRenderer == null) return;

        Color ammoColor = Color.white;
        switch (ammoType)
        {
            case AmmoType.Pistol:
                ammoColor = new Color(1f, 0.85f, 0.4f);
                break;
            case AmmoType.Shotgun:
                ammoColor = new Color(1f, 0.4f, 0.3f);
                break;
            case AmmoType.Uzi:
                ammoColor = new Color(0.4f, 0.8f, 1f);
                break;
        }

        meshRenderer.material.color = ammoColor;
    }

    public void SetAmmoType(AmmoType type, int amount)
    {
        ammoType = type;
        ammoAmount = amount;
        meshRenderer = GetComponent<MeshRenderer>();
        startPosition = transform.position;
        UpdateVisual();
        EnsureAmmoManager();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;
        if (other.CompareTag("Player"))
        {
            TryPickup(other.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
