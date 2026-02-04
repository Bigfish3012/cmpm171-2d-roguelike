using UnityEngine;

// Counts kills via EnemyHealth.OnEnemyDied. Exposes totals for UI.
public class KillCounter : MonoBehaviour
{
    private int totalKills;
    private int killsEnemy1;
    private int killsEnemy2;

    public int TotalKills => totalKills;
    public int KillsEnemy1 => killsEnemy1;
    public int KillsEnemy2 => killsEnemy2;

    void OnEnable()
    {
        EnemyHealth.OnEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnEnemyDied -= OnEnemyDied;
    }

    void OnEnemyDied(EnemyHealth.EnemyType type)
    {
        totalKills++;
        if (type == EnemyHealth.EnemyType.Enemy1)
            killsEnemy1++;
        else
            killsEnemy2++;
    }
}
