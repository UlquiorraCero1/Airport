using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;

    [HideInInspector]
    public Vector3 currentFollowPos;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
        currentFollowPos = transform.position;
    }

    void LateUpdate()
    {
        Vector3 desired = target.position + offset;
        currentFollowPos = Vector3.Lerp(currentFollowPos, desired, smoothSpeed * Time.deltaTime);

        // Apply shake on top of follow position
        Vector3 shakeOffset = ScreenShake.Instance != null
            ? ScreenShake.Instance.GetShakeOffset()
            : Vector3.zero;

        transform.position = currentFollowPos + shakeOffset;
    }
}