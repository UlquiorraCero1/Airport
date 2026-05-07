using UnityEngine;

public class HitMarker : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 0.3f;
    public float startSize = 0.3f;
    public float endSize = 0.8f;
    public Color hitColor = new Color(1f, 1f, 1f, 0.8f);
    public Color killColor = new Color(1f, 0.3f, 0.2f, 1f);

    private float timer = 0f;
    private bool isKill = false;
    private LineRenderer[] lines;

    public void Setup(bool kill)
    {
        isKill = kill;

        lines = new LineRenderer[2];

        for (int i = 0; i < 2; i++)
        {
            GameObject lineObj = new GameObject("Line" + i);
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.useWorldSpace = false;

            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = isKill ? killColor : hitColor;
            lr.material = mat;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            lines[i] = lr;
        }

        UpdateLines(startSize);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;

        float size = Mathf.Lerp(startSize, endSize, t);
        float alpha = 1f - t;

        UpdateLines(size);

        Color c = isKill ? killColor : hitColor;
        c.a = alpha;
        foreach (LineRenderer lr in lines)
        {
            if (lr != null && lr.material != null)
                lr.material.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void UpdateLines(float size)
    {
        float half = size / 2f;

        if (lines[0] != null)
        {
            lines[0].SetPosition(0, new Vector3(-half, 0, -half));
            lines[0].SetPosition(1, new Vector3(half, 0, half));
        }

        if (lines[1] != null)
        {
            lines[1].SetPosition(0, new Vector3(-half, 0, half));
            lines[1].SetPosition(1, new Vector3(half, 0, -half));
        }
    }

    public static HitMarker Create(Vector3 position, bool isKill = false)
    {
        GameObject go = new GameObject("HitMarker");
        go.transform.position = position + Vector3.up * 0.5f;

        HitMarker marker = go.AddComponent<HitMarker>();
        marker.Setup(isKill);

        return marker;
    }
}
