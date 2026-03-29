using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    public float attackRange = 2f;
    public float attackAngle = 90f;
    public LayerMask enemyLayer;
    public GameObject bloodPrefab;

    [HideInInspector]
    public bool hasWeapon = false;    // ← NEW

    private bool isDead = false;

    void Update()
    {
        if (isDead) return;

        // Only punch with fists if no weapon equipped
        if (hasWeapon) return;        // ← NEW

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        EnemyHealth closestEnemy = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            Vector3 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToEnemy);

            if (angle < attackAngle / 2f)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = hit.GetComponent<EnemyHealth>();
                }
            }
        }

        if (closestEnemy != null)
        {
            if (closestEnemy.isKnockedDown)
            {
                closestEnemy.Execute();
                SpawnBlood(closestEnemy.transform.position);
            }
            else
            {
                closestEnemy.TakeHit();
            }
        }
    }

    public void TakeDamage()
    {
        if (isDead) return;
        isDead = true;
        SpawnBlood(transform.position);
        GameManager.Instance.PlayerDied();
        GetComponent<Renderer>().enabled = false;
    }

    void SpawnBlood(Vector3 position)
    {
        if (bloodPrefab == null) return;
        Vector3 pos = new Vector3(position.x, 0.02f, position.z);
        Instantiate(bloodPrefab, pos, Quaternion.Euler(90f, Random.Range(0f, 360f), 0f));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}