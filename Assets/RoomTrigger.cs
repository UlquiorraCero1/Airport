using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public Room room;
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Disable this trigger so it never fires again
        GetComponent<Collider>().enabled = false;

        if (room != null)
            room.ActivateRoom();
    }
}