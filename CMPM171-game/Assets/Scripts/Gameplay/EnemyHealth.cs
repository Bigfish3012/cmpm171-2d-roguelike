using System;
using UnityEngine;

// Enemy health and death. Add a Collider2D (e.g. Circle/Box) for hit detection.
public class EnemyHealth : MonoBehaviour
{
    public enum EnemyType
    {
        Enemy1,
        Enemy2
    }

    [Header("Identity")]
    public EnemyType enemyType = EnemyType.Enemy1;

    [Header("Health")]
    public int maxHealth = 3;

    private int currentHealth;

    // Fired when any enemy dies; arg is type (used for XP: Enemy1 +1, Enemy2 +2).
    public static event Action<EnemyType> OnEnemyDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // Apply damage (e.g. from projectile). Dies when health <= 0.
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        OnEnemyDied?.Invoke(enemyType);
        Destroy(gameObject);
    }
}
