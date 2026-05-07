using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    public float speed = 80f;
    public float length = 6f;
    public float lifetime = 0.3f;

    private Vector3 direction;
    private LineRenderer lr;
    private float timer = 0f;
    private Vector3 currentPos;

    public void Setup(Vector3 from, Vector3 to)
    {
        lr = GetComponent<LineRenderer>();
        direction = (to - from).normalized;
        currentPos = from;
        transform.position = from;

        if (lr != null)
        {
            lr.startWidth = 0.12f;
            lr.endWidth = 0.06f;
            lr.SetPosition(0, from);
            lr.SetPosition(1, from);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        float fadeStart = lifetime * 0.7f;
        float alpha = timer < fadeStart
            ? 1f
            : 1f - ((timer - fadeStart) / (lifetime - fadeStart));

        currentPos += direction * speed * Time.deltaTime;
        Vector3 tail = currentPos - direction * length;

        if (lr != null)
        {
            lr.SetPosition(0, tail);
            lr.SetPosition(1, currentPos);
            lr.startWidth = 0.12f * alpha;
            lr.endWidth = 0.06f * alpha;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
