using UnityEngine;

public class BossHealth : EnemyHealth
{
    [Header("Boss Rewards")]
    public GameObject keyPrefab;

    protected override void Die()
    {
        // 1) Spawn the key at the boss’s position
        if (keyPrefab != null)
            Instantiate(keyPrefab, transform.position, Quaternion.identity);

        // 2) Then destroy the boss
        base.Die();
    }
}
