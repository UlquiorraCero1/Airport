using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLean : MonoBehaviour
{
    [Header("Lean Settings")]
    public float leanDistance = 8f;
    public float leanSpeed = 5f;
    public float returnSpeed = 8f;

    private Vector3 leanOffset = Vector3.zero;

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        if (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed)
        {
            // Get mouse offset from screen center
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 mousePos = mouse.position.ReadValue();
            Vector2 delta = (mousePos - screenCenter) / screenCenter;

            // Clamp so it doesn't go too far
            delta = Vector2.ClampMagnitude(delta, 1f);

            Vector3 targetLean = new Vector3(delta.x, 0f, delta.y) * leanDistance;
            leanOffset = Vector3.Lerp(leanOffset, targetLean, leanSpeed * Time.deltaTime);
        }
        else
        {
            // Return smoothly to center
            leanOffset = Vector3.Lerp(leanOffset, Vector3.zero, returnSpeed * Time.deltaTime);
        }
    }

    public Vector3 GetLeanOffset()
    {
        return leanOffset;
    }
}