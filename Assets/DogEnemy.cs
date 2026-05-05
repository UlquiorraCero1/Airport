using UnityEngine;

public class DogEnemy : MonoBehaviour
{
    [Header("Dog Settings")]
    public float chargeSpeed = 9f;
    public float detectionRange = 14f;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.6f;

    private Transform player;
    private float attackTimer = 0f;
    private bool isDead = false;
    private Renderer rend;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rend = GetComponent<Renderer>();

        if (rend != null)
            rend.material.color = new Color(0.55f, 0.35f, 0.1f);
    }

    void Update()
    {
        if (isDead || player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        if (dist > detectionRange) return;

        if (dist > attackRange)
        {
            transform.position += dir.normalized * chargeSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
        else
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                player.GetComponent<PlayerCombat>()?.TakeDamage();
            }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Destroy(gameObject, 0.05f);
    }
}