using UnityEngine;

public class AlertSystem : MonoBehaviour
{
    public static AlertSystem Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ReportSound(Vector3 position, float radius)
    {
        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies)
        {
            float dist = Vector3.Distance(position, enemy.transform.position);
            if (dist <= radius)
                enemy.HearSound(position);
        }
    }
}