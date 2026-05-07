using UnityEngine;

public class BloodPool : MonoBehaviour
{
    [Header("Growth")]
    public float growDuration = 0.3f;
    public float startScale = 0.5f;
    public float endScale = 1f;

    [Header("Lifetime")]
    public float lifetime = 30f;
    public float fadeStartTime = 25f;

    [Header("Visual")]
    public Color bloodColor = new Color(0.5f, 0.05f, 0.05f);

    private float timer = 0f;
    private bool isGrowing = true;
    private MeshRenderer meshRenderer;
    private float finalScale;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        finalScale = endScale * Random.Range(0.8f, 1.2f);
        transform.localScale = Vector3.one * startScale;

        if (meshRenderer != null)
        {
            meshRenderer.material.color = bloodColor;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isGrowing && timer < growDuration)
        {
            float t = timer / growDuration;
            float scale = Mathf.Lerp(startScale, finalScale, EaseOutQuad(t));
            transform.localScale = Vector3.one * scale;
        }
        else if (isGrowing)
        {
            isGrowing = false;
            transform.localScale = Vector3.one * finalScale;
        }

        if (timer >= fadeStartTime && meshRenderer != null)
        {
            float fadeProgress = (timer - fadeStartTime) / (lifetime - fadeStartTime);
            Color c = bloodColor;
            c.a = 1f - fadeProgress;
            meshRenderer.material.color = c;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    public static BloodPool Create(Vector3 position, float scale = 1f)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "BloodPool";

        position.y = 0.02f;
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);

        Collider col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material = new Material(Shader.Find("Sprites/Default"));
            mr.material.color = new Color(0.5f, 0.05f, 0.05f);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        BloodPool pool = go.AddComponent<BloodPool>();
        pool.endScale = scale;

        return pool;
    }
}
