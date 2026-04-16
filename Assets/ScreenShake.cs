using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance;

    private Vector3 shakeOffset = Vector3.zero;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Vector3 GetShakeOffset()
    {
        return shakeOffset;
    }

    public void Shake(float duration = 0.1f, float magnitude = 0.15f)
    {
        StopAllCoroutines();
        StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            // Only X and Z — no Y dipping ever
            shakeOffset = new Vector3(x, 0f, z);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }
}