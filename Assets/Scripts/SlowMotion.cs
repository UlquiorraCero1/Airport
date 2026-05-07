using UnityEngine;
using System.Collections;

public class SlowMotion : MonoBehaviour
{
    public static SlowMotion Instance;

    [Header("Kill Effect")]
    public float killSlowScale = 0.15f;
    public float killSlowDuration = 0.08f;

    [Header("Multi-Kill Effect")]
    public float multiKillSlowScale = 0.1f;
    public float multiKillDuration = 0.12f;
    public int multiKillThreshold = 3;

    [Header("Execution Effect")]
    public float executionSlowScale = 0.05f;
    public float executionDuration = 0.15f;

    [Header("Settings")]
    public float normalTimeScale = 1f;
    public bool enableSlowMotion = true;

    private int recentKills = 0;
    private float killWindow = 0.5f;
    private float lastKillTime = 0f;
    private Coroutine currentEffect;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Time.unscaledTime - lastKillTime > killWindow)
        {
            recentKills = 0;
        }
    }

    public void TriggerKillEffect()
    {
        if (!enableSlowMotion) return;

        recentKills++;
        lastKillTime = Time.unscaledTime;

        if (recentKills >= multiKillThreshold)
        {
            TriggerMultiKillEffect();
        }
        else
        {
            if (currentEffect != null) StopCoroutine(currentEffect);
            currentEffect = StartCoroutine(DoSlowMotion(killSlowScale, killSlowDuration));
        }
    }

    public void TriggerMultiKillEffect()
    {
        if (!enableSlowMotion) return;

        if (currentEffect != null) StopCoroutine(currentEffect);
        currentEffect = StartCoroutine(DoSlowMotion(multiKillSlowScale, multiKillDuration));
    }

    public void TriggerExecutionEffect()
    {
        if (!enableSlowMotion) return;

        if (currentEffect != null) StopCoroutine(currentEffect);
        currentEffect = StartCoroutine(DoSlowMotion(executionSlowScale, executionDuration));
    }

    public void TriggerCustomSlowMotion(float timeScale, float duration)
    {
        if (!enableSlowMotion) return;

        if (currentEffect != null) StopCoroutine(currentEffect);
        currentEffect = StartCoroutine(DoSlowMotion(timeScale, duration));
    }

    IEnumerator DoSlowMotion(float targetScale, float duration)
    {
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        float recoveryTime = 0.05f;
        float elapsed = 0f;
        float startScale = Time.timeScale;

        while (elapsed < recoveryTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / recoveryTime;
            Time.timeScale = Mathf.Lerp(startScale, normalTimeScale, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
        currentEffect = null;
    }

    public void PauseTime()
    {
        Time.timeScale = 0f;
    }

    public void ResumeTime()
    {
        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
