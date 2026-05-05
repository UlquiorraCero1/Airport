using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, Charge, Staggered }

    [Header("Stats")]
public int maxHits = 10;
public float chargeSpeed = 5f;      
public float chargeRange = 25f;
public float attackCooldown = 2f;  

[Header("Charge Settings")]
public float chargeWindupTime = 0.8f;  
private bool isWindingUp = false;

    [Header("Visual")]
    public GameObject bloodPrefab;

    private int currentHits = 0;
    private bool isDead = false;
    private BossState state = BossState.Idle;
    private Transform player;
    private float actionTimer = 3f;
    private Renderer rend;
    private Color originalColor;
    private bool isReady = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = new Color(0.5f, 0f, 0f);
            rend.material.color = originalColor;
        }

        transform.localScale = new Vector3(2f, 2f, 2f);
        state = BossState.Idle;
        actionTimer = 3f;

        StartCoroutine(WaitForPlayerToEnter());
    }

    IEnumerator WaitForPlayerToEnter()
    {
        while (true)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                float dist = Vector3.Distance(
                    p.transform.position, transform.position);

                if (dist < 35f)
                {
                    player = p.transform;
                    isReady = true;
                    BossHealthUI.Instance?.ShowBossHealth(maxHits);
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    void Update()
    {
        if (!isReady || isDead || player == null) return;

        actionTimer -= Time.deltaTime;

        switch (state)
        {
            case BossState.Idle:     HandleIdle();   break;
            case BossState.Charge:   HandleCharge(); break;
            case BossState.Staggered: break;
        }
    }

    void HandleIdle()
{
    if (actionTimer > 0f) return;

    FacePlayer();

    if (!isWindingUp)
        StartCoroutine(WindupThenCharge());
}

IEnumerator WindupThenCharge()
{
    isWindingUp = true;

    // Flash to warn player
    if (rend != null) rend.material.color = Color.yellow;
    yield return new WaitForSeconds(chargeWindupTime);

    float healthPercent = 1f - ((float)currentHits / maxHits);
    if (rend != null)
        rend.material.color = Color.Lerp(
            new Color(0.8f, 0.8f, 0f),
            originalColor,
            healthPercent);

    state = BossState.Charge;
    isWindingUp = false;
}

void HandleCharge()
{
    if (player == null) return;

    Vector3 dir = player.position - transform.position;
    dir.y = 0;
    float dist = dir.magnitude;

    FacePlayer();

    if (dist > 1.5f)
    {
        transform.position += dir.normalized * chargeSpeed * Time.deltaTime;
    }
    else
    {
        player.GetComponent<PlayerCombat>()?.TakeDamage();
        state = BossState.Idle;
        actionTimer = attackCooldown;
    }
}

    public void TakeHit()
    {
        if (isDead) return;

        currentHits++;
        BossHealthUI.Instance?.UpdateBossHealth(maxHits - currentHits);
        StartCoroutine(FlashHit());

        float healthPercent = 1f - ((float)currentHits / maxHits);
        if (rend != null)
            rend.material.color = Color.Lerp(
                new Color(0.8f, 0.8f, 0f),
                originalColor,
                healthPercent);

        if (currentHits >= maxHits)
            Die();
        else
            StartCoroutine(Stagger());
    }

    IEnumerator FlashHit()
    {
        if (rend != null) rend.material.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        float healthPercent = 1f - ((float)currentHits / maxHits);
        if (rend != null)
            rend.material.color = Color.Lerp(
                new Color(0.8f, 0.8f, 0f),
                originalColor,
                healthPercent);
    }

    IEnumerator Stagger()
    {
        state = BossState.Staggered;
        yield return new WaitForSeconds(0.3f);
        state = BossState.Idle;
        actionTimer = 0.3f;
    }

    void Die()
    {
        isDead = true;
        BossHealthUI.Instance?.HideBossHealth();
        ScreenShake.Instance?.Shake(0.4f, 1f);
        GameUI.Instance?.RegisterKill();

        if (bloodPrefab != null)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 pos = transform.position + new Vector3(
                    Random.Range(-1.5f, 1.5f), 0,
                    Random.Range(-1.5f, 1.5f));
                Instantiate(bloodPrefab,
                    new Vector3(pos.x, 0.02f, pos.z),
                    Quaternion.Euler(90f, Random.Range(0f, 360f), 0f));
            }
        }

        StartCoroutine(ShowWinAfterDelay());
        Destroy(gameObject, 0.1f);
    }

    IEnumerator ShowWinAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void FacePlayer()
    {
        if (player == null) return;
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
    }
}