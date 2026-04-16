using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [Header("Room Settings")]
    public string roomName = "Room 1";
    public List<GameObject> enemies = new List<GameObject>();
    public List<Door> doors = new List<Door>();

    [Header("State")]
    public bool isActive = false;
    public bool isCleared = false;

    private int remainingEnemies = 0;

    void Start()
    {
        // Deactivate all enemies at start
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
                enemy.SetActive(false);
        }
    }

    // Called when player enters the room
    public void ActivateRoom()
    {
        if (isCleared || isActive) return;

        isActive = true;
        remainingEnemies = 0;

        // Count and activate all enemies
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
                remainingEnemies++;

                // Listen for enemy death
                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh != null)
                    eh.onDeath += OnEnemyDied;
            }
        }

        // Lock all doors
        foreach (Door door in doors)
        {
            if (door != null)
                door.LockDoor();
        }

        Debug.Log(roomName + " activated — enemies: " + remainingEnemies);

        // If room has no enemies just clear it immediately
        if (remainingEnemies == 0)
            ClearRoom();
    }

    void OnEnemyDied()
    {
        remainingEnemies--;
        Debug.Log(roomName + " — enemies left: " + remainingEnemies);

        if (remainingEnemies <= 0)
            ClearRoom();
    }

    void ClearRoom()
    {
        isCleared = true;
        isActive = false;

        Debug.Log(roomName + " CLEARED!");

        // Unlock all doors
        foreach (Door door in doors)
        {
            if (door != null)
                door.UnlockDoor();
        }
    }
}