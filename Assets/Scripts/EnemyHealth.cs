using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [HideInInspector]
    public bool isKnockedDown = false;

    public GameObject bloodPrefab;

    [Header("Ammo Drop Settings")]
    public bool canDropAmmo = true;
    public float ammoDropChance = 0.6f;
    public AmmoType[] possibleAmmoDrops = { AmmoType.Pistol, AmmoType.Shotgun, AmmoType.Uzi };
    public int minAmmoDrop = 3;
    public int maxAmmoDrop = 8;

    [Header("Death Effects")]
    public bool enableDeathKnockback = true;
    public float deathKnockbackForce = 5f;
    public int bloodPoolCount = 2;
    public float bloodSpreadRadius = 0.6f;

    public Action onDeath;

    private bool isDead = false;
    private EnemyAI ai;
    private Renderer rend;
    private Color originalColor;
    private Vector3 lastHitDirection;
    private Rigidbody rb;

    void Start()
    {
        ai = GetComponent<EnemyAI>();
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    public void TakeHit()
    {
        if (isDead || isKnockedDown) return;

        DogEnemy dog = GetComponent<DogEnemy>();
        if (dog != null)
        {
            isDead = true;
            SpawnBloodPool(transform.position);
            TryDropAmmo();
            GameUI.Instance?.RegisterKill();
            onDeath?.Invoke();
            dog.Die();
            return;
        }

        isKnockedDown = true;
        if (ai != null) ai.SetKnockedDown(true);
        transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        if (rend != null) rend.material.color = new Color(0.25f, 0.1f, 0.1f);
        SpawnBloodPool(transform.position);
    }

    public void Execute()
    {
        if (!isKnockedDown || isDead) return;
        isDead = true;

        SpawnDeathBlood();
        TryDropAmmo();

        GameUI.Instance?.RegisterKill();
        onDeath?.Invoke();
        Destroy(gameObject, 0.05f);
    }

    public void TakeShot()
    {
        TakeShotFromDirection(Vector3.zero);
    }

    public void TakeShotFromDirection(Vector3 hitDirection)
    {
        if (isDead) return;
        isDead = true;
        lastHitDirection = hitDirection.normalized;

        ApplyDeathKnockback();
        SpawnDeathBlood();
        TryDropAmmo();

        GameUI.Instance?.RegisterKill();
        onDeath?.Invoke();

        DogEnemy dog = GetComponent<DogEnemy>();
        if (dog != null) dog.Die();

        StartCoroutine(DeathSequence());
    }

    System.Collections.IEnumerator DeathSequence()
    {
        if (rend != null)
        {
            Color deathColor = new Color(0.3f, 0.1f, 0.1f);
            rend.material.color = deathColor;
        }

        float rotateTime = 0.1f;
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(90f, transform.eulerAngles.y + UnityEngine.Random.Range(-30f, 30f), 0f);

        while (elapsed < rotateTime)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / rotateTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);
        Destroy(gameObject);
    }

    void ApplyDeathKnockback()
    {
        if (!enableDeathKnockback) return;
        if (rb == null) return;

        Vector3 knockbackDir = lastHitDirection;
        if (knockbackDir == Vector3.zero)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                knockbackDir = (transform.position - player.transform.position).normalized;
            else
                knockbackDir = UnityEngine.Random.insideUnitSphere;
        }

        knockbackDir.y = 0;
        rb.isKinematic = false;
        rb.AddForce(knockbackDir * deathKnockbackForce, ForceMode.Impulse);
    }

    void SpawnDeathBlood()
    {
        SpawnBloodPool(transform.position);

        for (int i = 0; i < bloodPoolCount; i++)
        {
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-bloodSpreadRadius, bloodSpreadRadius),
                0,
                UnityEngine.Random.Range(-bloodSpreadRadius, bloodSpreadRadius)
            );
            SpawnBloodPool(transform.position + offset);
        }
    }

    void TryDropAmmo()
    {
        if (!canDropAmmo) return;
        if (possibleAmmoDrops == null || possibleAmmoDrops.Length == 0) return;

        if (UnityEngine.Random.value > ammoDropChance) return;

        AmmoType dropType = possibleAmmoDrops[UnityEngine.Random.Range(0, possibleAmmoDrops.Length)];
        int dropAmount = GetAmmoDropAmount(dropType);

        SpawnAmmoDrop(dropType, dropAmount);
    }

    int GetAmmoDropAmount(AmmoType type)
    {
        switch (type)
        {
            case AmmoType.Pistol:
                return UnityEngine.Random.Range(3, 8);
            case AmmoType.Shotgun:
                return UnityEngine.Random.Range(2, 5);
            case AmmoType.Uzi:
                return UnityEngine.Random.Range(10, 25);
            default:
                return UnityEngine.Random.Range(minAmmoDrop, maxAmmoDrop + 1);
        }
    }

    void SpawnAmmoDrop(AmmoType type, int amount)
    {
        GameObject ammoObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ammoObj.name = "AmmoPickup_" + type.ToString();

        Vector3 dropPos = transform.position + Vector3.up * 0.3f;
        dropPos += new Vector3(
            UnityEngine.Random.Range(-0.3f, 0.3f),
            0,
            UnityEngine.Random.Range(-0.3f, 0.3f)
        );
        ammoObj.transform.position = dropPos;
        ammoObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        Collider col = ammoObj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        SphereCollider trigger = ammoObj.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 2f;

        AmmoPickup pickup = ammoObj.AddComponent<AmmoPickup>();
        pickup.SetAmmoType(type, amount);
        pickup.autoPickup = true;
        pickup.pickupRadius = 1.5f;
    }

    void SpawnBloodPool(Vector3 position)
    {
        if (bloodPrefab == null) return;
        Vector3 pos = new Vector3(position.x, 0.02f, position.z);
        GameObject blood = Instantiate(bloodPrefab, pos,
            Quaternion.Euler(90f, UnityEngine.Random.Range(0f, 360f), 0f));

        float scale = UnityEngine.Random.Range(0.8f, 1.3f);
        blood.transform.localScale *= scale;
    }

    public void SetLastHitDirection(Vector3 direction)
    {
        lastHitDirection = direction;
    }
}
