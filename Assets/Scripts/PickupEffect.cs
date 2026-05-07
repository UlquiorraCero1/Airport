using UnityEngine;

public class PickupEffect : MonoBehaviour
{
    [Header("Ring Effect")]
    public float ringDuration = 0.3f;
    public float ringStartSize = 0.5f;
    public float ringEndSize = 2f;
    public Color ringColor = new Color(1f, 1f, 1f, 0.6f);

    private float timer = 0f;
    private LineRenderer lineRenderer;
    private int segments = 32;

    void Start()
    {
        CreateRing();
    }

    void CreateRing()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = ringColor;
        lineRenderer.material = mat;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        UpdateRing(ringStartSize);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / ringDuration;

        float size = Mathf.Lerp(ringStartSize, ringEndSize, EaseOutQuad(t));
        float alpha = 1f - t;

        UpdateRing(size);

        Color c = ringColor;
        c.a = alpha * ringColor.a;
        if (lineRenderer != null && lineRenderer.material != null)
            lineRenderer.material.color = c;

        float width = Mathf.Lerp(0.08f, 0.02f, t);
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        if (timer >= ringDuration)
            Destroy(gameObject);
    }

    void UpdateRing(float radius)
    {
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, 0f, z));
        }
    }

    float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    public static PickupEffect Create(Vector3 position, Color color)
    {
        GameObject go = new GameObject("PickupEffect");
        go.transform.position = new Vector3(position.x, 0.1f, position.z);

        PickupEffect effect = go.AddComponent<PickupEffect>();
        effect.ringColor = color;

        return effect;
    }

    public static void CreateWeaponPickup(Vector3 position)
    {
        Create(position, new Color(1f, 0.9f, 0.4f, 0.7f));
    }

    public static void CreateAmmoPickup(Vector3 position)
    {
        Create(position, new Color(0.4f, 1f, 0.6f, 0.5f));
    }
}
