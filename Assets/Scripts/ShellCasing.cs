using UnityEngine;

public class ShellCasing : MonoBehaviour
{
    [Header("Physics")]
    public float ejectForce = 3f;
    public float upwardForce = 2f;
    public float torque = 500f;
    public float lifetime = 5f;

    [Header("Visual")]
    public Color shellColor = new Color(0.85f, 0.7f, 0.2f);

    private Rigidbody rb;
    private float timer = 0f;
    private bool hasLanded = false;
    private MeshRenderer meshRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
            meshRenderer.material.color = shellColor;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > lifetime - 1f && meshRenderer != null)
        {
            float alpha = 1f - ((timer - (lifetime - 1f)) / 1f);
            Color c = meshRenderer.material.color;
            c.a = alpha;
            meshRenderer.material.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasLanded)
        {
            hasLanded = true;
            if (rb != null)
            {
                rb.linearVelocity *= 0.3f;
                rb.angularVelocity *= 0.5f;
            }
        }
    }

    public void Eject(Vector3 direction, float force)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 ejectDir = direction;
        ejectDir += new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(0.1f, 0.3f),
            Random.Range(-0.2f, 0.2f)
        );

        rb.AddForce(ejectDir.normalized * force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * torque);
    }

    public static ShellCasing Create(Vector3 position, Vector3 ejectDirection, float force, AmmoType ammoType)
    {
        GameObject go;
        Color color = new Color(0.85f, 0.7f, 0.2f);

        switch (ammoType)
        {
            case AmmoType.Shotgun:
                go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
                color = new Color(0.9f, 0.2f, 0.2f);
                break;
            case AmmoType.Uzi:
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.localScale = new Vector3(0.03f, 0.05f, 0.03f);
                color = new Color(0.85f, 0.75f, 0.3f);
                break;
            default:
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.localScale = new Vector3(0.04f, 0.07f, 0.04f);
                color = new Color(0.85f, 0.7f, 0.2f);
                break;
        }

        go.name = "ShellCasing";
        go.transform.position = position;
        go.transform.rotation = Random.rotation;

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material = new Material(Shader.Find("Standard"));
            mr.material.color = color;
            mr.material.SetFloat("_Metallic", 0.8f);
            mr.material.SetFloat("_Smoothness", 0.6f);
        }

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.01f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.3f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        ShellCasing shell = go.AddComponent<ShellCasing>();
        shell.shellColor = color;
        shell.Eject(ejectDirection, force);

        return shell;
    }
}
