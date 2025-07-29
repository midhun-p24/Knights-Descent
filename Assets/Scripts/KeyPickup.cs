using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    // Optional: name of the key, if you have multiple keys
    public string keyName = "ExitKey";

    void Reset()
    {
        // Ensure the collider is set up correctly
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the player
        if (!other.CompareTag("Player"))
            return;

        // Inform the door (or GameManager) that the key was picked up
        ExitDoorController door = Object.FindAnyObjectByType<ExitDoorController>();
        if (door != null)
        {
            door.Unlock();
            Debug.Log($"Picked up key '{keyName}'. Door unlocked!");
        }
        else
        {
            Debug.LogWarning("KeyPickup: No ExitDoorController found in scene.");
        }

        // Destroy the key object so it can’t be picked up again
        Destroy(gameObject);
    }
}
