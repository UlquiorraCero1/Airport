using UnityEngine;

public class HeavyEnemy : MonoBehaviour
{
    private int hitsToKnockdown = 2;
    private int hitCount = 0;
    private EnemyHealth enemyHealth;
    private Renderer rend;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        rend = GetComponent<Renderer>();

        // Make heavy enemies bigger and darker
        transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        if (rend != null)
            rend.material.color = new Color(0.4f, 0.1f, 0.1f);
    }
    public void TakeHeavyHit()
    {
        hitCount++;

        // Flash white to show damage
        if (rend != null)
            StartCoroutine(FlashWhite());

        if (hitCount >= hitsToKnockdown)
        {
            // Enough hits — knock down normally
            enemyHealth.TakeHit();
        }
    }

    System.Collections.IEnumerator FlashWhite()
    {
        Color original = rend.material.color;
        rend.material.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        rend.material.color = original;
    }
}