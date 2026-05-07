using UnityEngine;
using UnityEngine.UI;

public class Scanlines : MonoBehaviour
{
    [Header("Settings")]
    public int lineThickness = 2;
    public float alpha = 0.12f;
    public float scrollSpeed = 15f;

    private RawImage rawImage;
    private Texture2D scanlineTex;
    private float scrollOffset = 0f;

    void Start()
    {
        rawImage = GetComponent<RawImage>();

        // Create a small repeating scanline texture
        scanlineTex = new Texture2D(1, lineThickness * 2, TextureFormat.RGBA32, false);
        scanlineTex.filterMode = FilterMode.Point;
        scanlineTex.wrapMode   = TextureWrapMode.Repeat;

        // Top half dark, bottom half transparent
        for (int y = 0; y < lineThickness; y++)
            scanlineTex.SetPixel(0, y, new Color(0, 0, 0, alpha));
        for (int y = lineThickness; y < lineThickness * 2; y++)
            scanlineTex.SetPixel(0, y, new Color(0, 0, 0, 0));

        scanlineTex.Apply();

        if (rawImage != null)
        {
            rawImage.texture = scanlineTex;
            rawImage.color   = Color.white;

            rawImage.uvRect = new Rect(0, 0,
                Screen.width  / (float)(lineThickness * 2),
                Screen.height / (float)(lineThickness * 2));
        }
    }

    void Update()
    {
        // Slowly scroll downward
        scrollOffset += scrollSpeed * Time.deltaTime / Screen.height;
        if (scrollOffset > 1f) scrollOffset -= 1f;

        if (rawImage != null)
        {
            Rect r = rawImage.uvRect;
            r.y = scrollOffset;
            rawImage.uvRect = r;
        }
    }
}