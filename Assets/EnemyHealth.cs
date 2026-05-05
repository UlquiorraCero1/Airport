using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [HideInInspector]
    public bool isKnockedDown = false;

    public GameObject bloodPrefab;

    public Action onDeath;

    private bool isDead = false;
    private EnemyAI ai;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        ai = GetComponent<EnemyAI>();
        rend = GetComponent<Renderer>();
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
            SpawnBlood(transform.position);
            GameUI.Instance?.RegisterKill();
            onDeath?.Invoke();
            dog.Die();
            return;
        }

        isKnockedDown = true;
        if (ai != null) ai.SetKnockedDown(true);
        transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        if (rend != null) rend.material.color = new Color(0.25f, 0.1f, 0.1f);
        SpawnBlood(transform.position);
    }

    public void Execute()
    {
        if (!isKnockedDown || isDead) return;
        isDead = true;

        SpawnBlood(transform.position);
        SpawnBlood(transform.position + new Vector3(
            UnityEngine.Random.Range(-0.6f, 0.6f), 0,
            UnityEngine.Random.Range(-0.6f, 0.6f)));

        GameUI.Instance?.RegisterKill();
        onDeath?.Invoke();
        Destroy(gameObject, 0.05f);
    }

    public void TakeShot()
    {
        if (isDead) return;
        isDead = true;

        SpawnBlood(transform.position);
        SpawnBlood(transform.position + new Vector3(
            UnityEngine.Random.Range(-0.4f, 0.4f), 0,
            UnityEngine.Random.Range(-0.4f, 0.4f)));

        GameUI.Instance?.RegisterKill();
        onDeath?.Invoke();

        DogEnemy dog = GetComponent<DogEnemy>();
        if (dog != null) dog.Die();

        Destroy(gameObject, 0.05f);
    }

    void SpawnBlood(Vector3 position)
    {
        if (bloodPrefab == null) return;
        Vector3 pos = new Vector3(position.x, 0.02f, position.z);
        Instantiate(bloodPrefab, pos,
            Quaternion.Euler(90f, UnityEngine.Random.Range(0f, 360f), 0f));
    }
}