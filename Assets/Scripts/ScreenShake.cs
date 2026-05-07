using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance;

    [Header("Settings")]
    public float traumaDecay = 1.5f;
    public float maxAngle = 5f;
    public float maxOffset = 0.5f;
    public float frequency = 25f;

    private Vector3 shakeOffset = Vector3.zero;
    private float shakeAngle = 0f;
    private float trauma = 0f;
    private float seed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        seed = Random.value * 1000f;
    }

    void Update()
    {
        if (trauma > 0f)
        {
            trauma = Mathf.Clamp01(trauma - traumaDecay * Time.unscaledDeltaTime);

            float shake = trauma * trauma;

            float time = Time.unscaledTime * frequency;
            float offsetX = maxOffset * shake * (Mathf.PerlinNoise(seed, time) * 2f - 1f);
            float offsetZ = maxOffset * shake * (Mathf.PerlinNoise(seed + 1f, time) * 2f - 1f);
            shakeAngle = maxAngle * shake * (Mathf.PerlinNoise(seed + 2f, time) * 2f - 1f);

            shakeOffset = new Vector3(offsetX, 0f, offsetZ);
        }
        else
        {
            shakeOffset = Vector3.zero;
            shakeAngle = 0f;
        }
    }

    public Vector3 GetShakeOffset()
    {
        return shakeOffset;
    }

    public float GetShakeAngle()
    {
        return shakeAngle;
    }

    public void Shake(float duration = 0.1f, float magnitude = 0.15f)
    {
        AddTrauma(magnitude * 2f);
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    public void ShakeOnce(float intensity)
    {
        StopAllCoroutines();
        StartCoroutine(DoShakeOnce(intensity));
    }

    IEnumerator DoShakeOnce(float intensity)
    {
        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float shake = intensity * (1f - t);

            float x = Random.Range(-1f, 1f) * shake;
            float z = Random.Range(-1f, 1f) * shake;

            shakeOffset = new Vector3(x, 0f, z);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }

    public void BigShake()
    {
        AddTrauma(0.8f);
    }

    public void SmallShake()
    {
        AddTrauma(0.3f);
    }
}
