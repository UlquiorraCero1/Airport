using UnityEngine;

public class AmmoCrate : MonoBehaviour
{
    [Header("Crate Settings")]
    public bool containsAllAmmoTypes = true;
    public AmmoType specificAmmoType = AmmoType.Pistol;

    [Header("Ammo Amounts")]
    public int pistolAmmo = 14;
    public int shotgunAmmo = 8;
    public int uziAmmo = 60;

    [Header("Visual")]
    public Color crateColor = new Color(0.4f, 0.6f, 0.3f);
    public bool isOpen = false;

    [Header("Interaction")]
    public float interactRadius = 2f;
    public bool requiresKeyPress = true;

    private bool isEmpty = false;
    private MeshRenderer meshRenderer;
    private bool playerInRange = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.material.color = crateColor;

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
        if (isEmpty) return;

        CheckPlayerRange();

        if (playerInRange && requiresKeyPress)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                OpenCrate();
            }
        }
    }

    void CheckPlayerRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius);
        playerInRange = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerInRange = true;
                if (!requiresKeyPress && !isEmpty)
                    OpenCrate();
                return;
            }
        }
    }

    void OpenCrate()
    {
        if (isEmpty) return;

        EnsureAmmoManager();

        isEmpty = true;

        if (AmmoManager.Instance != null)
        {
            if (containsAllAmmoTypes)
            {
                AmmoManager.Instance.AddAmmo(AmmoType.Pistol, pistolAmmo);
                AmmoManager.Instance.AddAmmo(AmmoType.Shotgun, shotgunAmmo);
                AmmoManager.Instance.AddAmmo(AmmoType.Uzi, uziAmmo);
            }
            else
            {
                int amount = 0;
                switch (specificAmmoType)
                {
                    case AmmoType.Pistol: amount = pistolAmmo; break;
                    case AmmoType.Shotgun: amount = shotgunAmmo; break;
                    case AmmoType.Uzi: amount = uziAmmo; break;
                }
                AmmoManager.Instance.AddAmmo(specificAmmoType, amount);
            }
        }

        OnCrateOpened();
    }

    void OnCrateOpened()
    {
        isOpen = true;

        if (meshRenderer != null)
            meshRenderer.material.color = crateColor * 0.4f;

        ScreenShake.Instance?.Shake(0.1f, 0.15f);

        transform.localScale = transform.localScale * 0.9f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isEmpty) return;
        if (!requiresKeyPress && other.CompareTag("Player"))
        {
            OpenCrate();
        }
    }

    public void ResetCrate()
    {
        isEmpty = false;
        isOpen = false;
        if (meshRenderer != null)
            meshRenderer.material.color = crateColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isEmpty ? Color.gray : Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
