using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Needed for coroutines

[RequireComponent(typeof(Animator))] // Ensure an Animator exists
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    private Slider healthSlider;

    // --- NEW: Animator reference ---
    private Animator m_animator;
    private bool isDead = false; // Prevent multiple deaths

    void Awake()
    {
        currentHealth = maxHealth;
        healthSlider = GetComponentInChildren<Slider>();

        // --- NEW: Get the animator component ---
        m_animator = GetComponent<Animator>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // Don't take damage if already dead

        currentHealth -= amount;
        Debug.Log($"{name} took {amount} dmg, {currentHealth} HP left.");

        // --- NEW: Trigger Hurt Animation ---
        m_animator.SetTrigger("Hurt");

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{name} died!");

        // --- NEW: Trigger Death Animation and wait ---
        StartCoroutine(DeathSequence());
    }

    // --- NEW: Coroutine to handle death sequence ---
    private IEnumerator DeathSequence()
    {
        // 1. Trigger the death animation
        m_animator.SetTrigger("Death");

        // 2. Disable the AI script so it stops moving
        var ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.enabled = false;
        }

        // 3. Disable the collider so it can't be hit anymore
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 4. Wait for a short time for the animation to play
        // (Adjust this time to match your animation's length)
        yield return new WaitForSeconds(1.5f);

        // 5. Finally, destroy the game object
        Destroy(gameObject);
    }
}
