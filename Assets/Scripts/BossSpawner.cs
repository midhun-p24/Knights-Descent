using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    private bool bossSpawned = false;

    void Update()
    {
        if (bossSpawned) return;

        // If there are zero active minions tagged "Enemy" in the scene…
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            // …spawn the boss here and only once
            Instantiate(bossPrefab, transform.position, Quaternion.identity);
            bossSpawned = true;
        }
    }

    // Optional: visualize the spawn point in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
