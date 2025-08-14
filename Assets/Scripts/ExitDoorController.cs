using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitDoorController : MonoBehaviour
{
    public bool isLocked = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isLocked)
        {
            Debug.Log("Door is locked. Need the key!");
            return;
        }

        // --- MODIFIED: Call the GameManager to go to the next level ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToNextLevel();
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }
    }

    public void Unlock() => isLocked = false;
}
