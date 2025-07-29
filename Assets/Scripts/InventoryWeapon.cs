using UnityEngine;

public class InventoryWeapon
{
    public Sprite icon;
    public Color color;
    public int damage;
    public float attackRate;

    public InventoryWeapon(Sprite icon, Color color, int damage, float attackRate)
    {
        this.icon = icon;
        this.color = color;
        this.damage = damage;
        this.attackRate = attackRate;
    }
}
