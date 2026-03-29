using UnityEngine;
using System.Collections;

public class SlowMotion : MonoBehaviour
{
    public static SlowMotion Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Call this when player kills an enemy
    public void TriggerKillEffect()
    {
        StopAllCoroutines();
        StartCoroutine(KillSlowMo());
    }

    IEnumerator KillSlowMo()
    {
        // Slam into slow motion
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(0.1f);

        // Snap back to normal
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}