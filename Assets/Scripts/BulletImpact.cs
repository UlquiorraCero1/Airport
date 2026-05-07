using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 0.3f;
    public float startSize = 0.2f;
    public float endSize = 0.6f;

    [Header("Debris")]
    public int debrisCount = 3;
    public float debrisForce = 2f;

    private float timer = 0f;
    private MeshRenderer meshRenderer;
    private Color startColor;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            startColor = meshRenderer.material.color;

        transform.localScale = Vector3.one * startSize;

        SpawnDebris();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;

        float scale = Mathf.Lerp(startSize, endSize, t);
        transform.localScale = Vector3.one * scale;

        if (meshRenderer != null)
        {
            Color c = startColor;
            c.a = 1f - t;
            meshRenderer.material.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void SpawnDebris()
    {
        for (int i = 0; i < debrisCount; i++)
        {
            GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.name = "Debris";
            debris.transform.position = transform.position;
            debris.transform.localScale = Vector3.one * Random.Range(0.02f, 0.06f);

            Collider col = debris.GetComponent<Collider>();
            if (col != null) Destroy(col);

            MeshRenderer mr = debris.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = new Material(Shader.Find("Standard"));
                mr.material.color = new Color(0.3f, 0.3f, 0.3f);
            }

            Rigidbody rb = debris.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.useGravity = true;

            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1.5f),
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(randomDir * debrisForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10f);

            Destroy(debris, 2f);
        }
    }

    public static BulletImpact Create(Vector3 position, Vector3 normal, bool isWallHit)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "BulletImpact";
        go.transform.position = position + normal * 0.01f;
        go.transform.rotation = Quaternion.LookRotation(-normal);

        Collider col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material = new Material(Shader.Find("Sprites/Default"));
            mr.material.color = isWallHit ? new Color(0.5f, 0.5f, 0.5f, 0.8f) : new Color(1f, 0.3f, 0.2f, 0.8f);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        BulletImpact impact = go.AddComponent<BulletImpact>();
        impact.debrisCount = isWallHit ? 3 : 0;

        return impact;
    }
}
