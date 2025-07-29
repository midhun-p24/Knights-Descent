using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    public RectTransform highlight;
    public Image[] slotImages; // size = 5

    private List<InventoryWeapon> weapons = new List<InventoryWeapon>();
    private int selectedIndex = 0;

    void Awake()
    {
        Instance = this;

        // Hide all slot images at start
        foreach (var img in slotImages)
            img.enabled = false;

        UpdateHighlight();
    }

    public void AddWeapon(InventoryWeapon weapon)
    {
        if (weapons.Count >= slotImages.Length)
        {
            Debug.Log("Inventory full.");
            return;
        }

        weapons.Add(weapon);
        int idx = weapons.Count - 1;
        slotImages[idx].sprite = weapon.icon;
        slotImages[idx].color = weapon.color;
        slotImages[idx].enabled = true;
    }

    public void SelectSlot(int idx)
    {
        if (idx < 0 || idx >= weapons.Count) return;

        selectedIndex = idx;
        UpdateHighlight();

        // Update player damage and attack rate
        var selected = weapons[idx];
        PlayerController.Instance.currentWeaponDamage = selected.damage;
        PlayerController.Instance.attackRate = selected.attackRate;
    }

    private void UpdateHighlight()
    {
        var slot = slotImages[selectedIndex].rectTransform;
        highlight.position = slot.position;
    }

    void Update()
    {
        // Switch using keys 1–5
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }
}
