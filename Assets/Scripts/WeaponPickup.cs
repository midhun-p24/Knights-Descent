using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Settings")]
    public int damage = 10;
    [Tooltip("Attacks per second")]
    public float attackRate = 2f;

    private Sprite icon;
    private Color iconColor = Color.white;

    void Awake()
    {
        // Get sprite for inventory icon
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            icon = sr.sprite;
            iconColor = sr.color;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var w = new InventoryWeapon(icon, iconColor, damage, attackRate);
        InventoryManager.Instance.AddWeapon(w);

        Destroy(gameObject);
    }
}
