using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;

    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  h = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h =  1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)  v = -1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)    v =  1f;

        moveInput = new Vector3(h, 0f, v).normalized;

        RotateTowardsMouse();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void RotateTowardsMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 lookPoint = ray.GetPoint(distance);
            lookPoint.y = transform.position.y;
            transform.LookAt(lookPoint);
        }
    }
}