using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;

    [HideInInspector]
    public Vector3 currentFollowPos;

    private Vector3 offset;
    private CameraLean cameraLean;

    void Start()
    {
        offset = transform.position - target.position;
        currentFollowPos = transform.position;
        cameraLean = GetComponent<CameraLean>();
    }

    void LateUpdate()
    {
        Vector3 desired = target.position + offset;
        currentFollowPos = Vector3.Lerp(
            currentFollowPos, desired,
            smoothSpeed * Time.deltaTime);

        Vector3 shakeOffset = ScreenShake.Instance != null
            ? ScreenShake.Instance.GetShakeOffset()
            : Vector3.zero;

        Vector3 leanOffset = cameraLean != null
            ? cameraLean.GetLeanOffset()
            : Vector3.zero;

        transform.position = currentFollowPos + shakeOffset + leanOffset;
    }
}