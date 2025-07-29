using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitDoorController : MonoBehaviour
{
    public bool isLocked = true;
    public string requiredKey = "ExitKey";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isLocked)
        {
            Debug.Log("Door is locked. Need the key!");
            return;
        }
        GameManager.Instance.WinGame();
    }

    // Call this from your KeyPickup script when player gets the key
    public void Unlock() => isLocked = false;
}
