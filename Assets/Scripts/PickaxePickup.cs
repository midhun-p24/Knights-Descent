using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickaxePickup : MonoBehaviour
{
    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
            pc.hasPickaxe = true;
        Destroy(gameObject);
    }
}
