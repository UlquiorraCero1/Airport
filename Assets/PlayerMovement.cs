using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    private Rigidbody rb;
    private Vector3 moveInput;

    private InputAction moveAction;
    private InputAction mouseAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        moveAction = new InputAction("Move");
        moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");

        mouseAction = new InputAction("Look", binding: "<Mouse>/position");
    }

    void OnEnable()
    {
        moveAction.Enable();
        mouseAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        mouseAction.Disable();
    }

    void Update()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        moveInput = new Vector3(moveValue.x, 0f, moveValue.y).normalized;

        RotateTowardsMouse();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void RotateTowardsMouse()
    {
        Vector2 mousePos = mouseAction.ReadValue<Vector2>();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 lookPoint = ray.GetPoint(distance);
            lookPoint.y = transform.position.y;
            transform.LookAt(lookPoint);
        }
    }
}