using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.05f;
    public float minSize = 0.3f;
    public float maxSize = 0.6f;

    [Header("Light")]
    public Light flashLight;
    public float lightIntensity = 3f;
    public float lightRange = 5f;

    private float timer = 0f;
    private MeshRenderer meshRenderer;
    private float startSize;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        startSize = Random.Range(minSize, maxSize);
        transform.localScale = Vector3.one * startSize;
        transform.Rotate(0f, 0f, Random.Range(0f, 360f));

        if (flashLight != null)
        {
            flashLight.intensity = lightIntensity;
            flashLight.range = lightRange;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;
        float scale = Mathf.Lerp(startSize, 0f, t);
        transform.localScale = Vector3.one * scale;

        if (flashLight != null)
            flashLight.intensity = Mathf.Lerp(lightIntensity, 0f, t);

        if (timer >= duration)
            Destroy(gameObject);
    }

    public static MuzzleFlash Create(Vector3 position, float size = 1f)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "MuzzleFlash";
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(90f, 0f, Random.Range(0f, 360f));

        Collider col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material = new Material(Shader.Find("Sprites/Default"));
            mr.material.color = new Color(1f, 0.9f, 0.5f, 1f);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        MuzzleFlash mf = go.AddComponent<MuzzleFlash>();
        mf.minSize = 0.3f * size;
        mf.maxSize = 0.6f * size;

        GameObject lightObj = new GameObject("FlashLight");
        lightObj.transform.SetParent(go.transform);
        lightObj.transform.localPosition = Vector3.zero;
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.85f, 0.5f);
        light.intensity = 3f * size;
        light.range = 5f * size;
        mf.flashLight = light;

        return mf;
    }
}
