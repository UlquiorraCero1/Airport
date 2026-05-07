using UnityEngine;
using System.Collections;

public class BossDoor : MonoBehaviour
{
    [Header("Door Parts")]
    public GameObject leftDoor;
    public GameObject rightDoor;

    [Header("Settings")]
    public float slideDistance = 6f;
    public float openSpeed = 5f;
    public float autoOpenRange = 8f;  // how close player needs to be

    private Vector3 leftClosed;
    private Vector3 rightClosed;
    private Vector3 leftOpen;
    private Vector3 rightOpen;

    private Collider leftCol;
    private Collider rightCol;

    private Transform player;
    private bool hasOpened = false;
    private bool hasClosed = false;
    private bool playerInside = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        leftClosed  = leftDoor.transform.position;
        rightClosed = rightDoor.transform.position;
        leftOpen    = leftClosed  + Vector3.left  * slideDistance;
        rightOpen   = rightClosed + Vector3.right * slideDistance;

        leftCol  = leftDoor.GetComponent<Collider>();
        rightCol = rightDoor.GetComponent<Collider>();

        SetColor(new Color(0.6f, 0.1f, 0.1f));
    }

    void Update()
    {
        if (player == null || hasClosed) return;

        float dist = Vector3.Distance(player.position, transform.position);

        // Auto open when player gets close
        if (!hasOpened && dist < autoOpenRange)
        {
            hasOpened = true;
            StartCoroutine(OpenThenClose());
        }
    }

    IEnumerator OpenThenClose()
{
    // Slide open
    SetColor(new Color(0.1f, 0.6f, 0.1f));
    StartCoroutine(MoveDoor(leftDoor.transform,  leftOpen));
    StartCoroutine(MoveDoor(rightDoor.transform, rightOpen));

    if (leftCol  != null) leftCol.enabled  = false;
    if (rightCol != null) rightCol.enabled = false;

    // Wait a fixed time for player to walk through
    yield return new WaitForSeconds(2.5f);

    // Slam shut
    hasClosed = true;
    SetColor(new Color(0.6f, 0.1f, 0.1f));

    StartCoroutine(MoveDoor(leftDoor.transform,  leftClosed));
    StartCoroutine(MoveDoor(rightDoor.transform, rightClosed));

    yield return new WaitForSeconds(0.8f);

    if (leftCol  != null) leftCol.enabled  = true;
    if (rightCol != null) rightCol.enabled = true;
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