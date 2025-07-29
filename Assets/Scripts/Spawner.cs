using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int spawnCount = 5;
    public Vector2 areaSize = new Vector2(8f, 8f);

    void Start()
    {
        // Add a null check for safety
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned to the Spawner in the Inspector!");
            return;
        }
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        int spawned = 0;
        int attempts = 0;
        while (spawned < spawnCount && attempts < spawnCount * 5) // Increased attempts for safety
        {
            attempts++;
            // Pick a random offset within the room's area
            float offsetX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
            float offsetY = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
            Vector3 candidatePosition = transform.position + new Vector3(offsetX, offsetY, 0f);

            int x = Mathf.RoundToInt(candidatePosition.x);
            int y = Mathf.RoundToInt(candidatePosition.y);

            // --- THIS IS THE NEW, SMARTER CHECK ---
            // Check if both the target tile (feet) AND the tile above it (head) are walkable.
            if (DungeonGenerator.Instance.IsWalkable(x, y) && DungeonGenerator.Instance.IsWalkable(x, y + 1))
            {
                // If there's enough clearance, spawn the enemy
                Instantiate(enemyPrefab, new Vector3(x + 0.5f, y + 0.5f, -0.1f), Quaternion.identity);
                spawned++;
            }
        }

        if (spawned < spawnCount)
        {
            Debug.LogWarning($"Spawner could only spawn {spawned}/{spawnCount} enemies due to tight space.");
        }
    }
}
