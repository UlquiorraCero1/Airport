using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Two Part Door")]
    public GameObject leftDoor;
    public GameObject rightDoor;
    public float slideDistance = 2.5f;
    public float openSpeed = 4f;

    private Vector3 leftClosed;
    private Vector3 rightClosed;
    private Vector3 leftOpen;
    private Vector3 rightOpen;

    private Collider leftCol;
    private Collider rightCol;
    private bool isOpen = false;

    void Start()
    {
        if (leftDoor == null || rightDoor == null) return;

        // Remember closed positions
        leftClosed  = leftDoor.transform.position;
        rightClosed = rightDoor.transform.position;

        leftOpen  = leftClosed  + Vector3.left  * slideDistance;
        rightOpen = rightClosed + Vector3.right * slideDistance;

        leftCol  = leftDoor.GetComponent<Collider>();
        rightCol = rightDoor.GetComponent<Collider>();

        OpenDoor();
    }

    public void LockDoor()
    {
        if (isOpen == false) return;
        isOpen = false;

        StopAllCoroutines();
        StartCoroutine(MoveDoor(leftDoor.transform,  leftClosed));
        StartCoroutine(MoveDoor(rightDoor.transform, rightClosed));

        // Enable colliders to block player
        if (leftCol  != null) leftCol.enabled  = true;
        if (rightCol != null) rightCol.enabled = true;

        // Turn red
        SetColor(new Color(0.6f, 0.1f, 0.1f));
    }

    public void UnlockDoor()
    {
        StopAllCoroutines();
        StartCoroutine(OpenAfterDelay());
    }

    IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        OpenDoor();
    }

    void OpenDoor()
    {
        isOpen = true;

        StopAllCoroutines();
        StartCoroutine(MoveDoor(leftDoor.transform,  leftOpen));
        StartCoroutine(MoveDoor(rightDoor.transform, rightOpen));

        // Turn green
        SetColor(new Color(0.1f, 0.6f, 0.1f));

        // Disable colliders after opening
        StartCoroutine(DisableCollidersAfterOpen());
    }

    IEnumerator MoveDoor(Transform door, Vector3 target)
    {
        while (Vector3.Distance(door.position, target) > 0.01f)
        {
            door.position = Vector3.Lerp(
                door.position, target,
                openSpeed * Time.deltaTime);
            yield return null;
        }
        door.position = target;
    }

    IEnumerator DisableCollidersAfterOpen()
    {
        yield return new WaitForSeconds(0.6f);
        if (leftCol  != null) leftCol.enabled  = false;
        if (rightCol != null) rightCol.enabled = false;
    }

    void SetColor(Color color)
    {
        if (leftDoor != null)
        {
            Renderer r = leftDoor.GetComponent<Renderer>();
            if (r != null) r.material.color = color;
        }
        if (rightDoor != null)
        {
            Renderer r = rightDoor.GetComponent<Renderer>();
            if (r != null) r.material.color = color;
        }
    }
}