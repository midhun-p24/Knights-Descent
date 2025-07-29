using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    private Slider healthSlider;

    void Awake()
    {
        currentHealth = maxHealth;

        // Auto-find the UI slider in the scene by name
        var sliderGO = GameObject.Find("PlayerHealthBar");
        if (sliderGO != null)
            healthSlider = sliderGO.GetComponent<Slider>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Player took {amount} dmg, {currentHealth} HP left.");
        if (healthSlider != null)
            healthSlider.value = currentHealth;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Player died!");

        var anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Death");

        // Disable movement
        GetComponent<PlayerController>().enabled = false;
    }
}
