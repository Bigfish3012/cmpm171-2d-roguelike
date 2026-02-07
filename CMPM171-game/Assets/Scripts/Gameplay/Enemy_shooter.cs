using UnityEngine;

public class Enemy_shooter : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHealth = 2;                                         // Maximum health of the enemy
    [SerializeField] private int attackDamage = 1;                                      // Damage of the enemy bullet
    [SerializeField] private Projectile enemyBulletPrefab;                              // Prefab of the enemy bullet
    [SerializeField] private Transform firePoint;                                       // Point to spawn the enemy bullet
    [SerializeField] private float fireCooldown = 1.5f;                                 // Cooldown between each shot
    [SerializeField] private float shootRange = 10f;                                    // Range to spawn the enemy bullet

    private int currentHealth;                                                           // Current health of the enemy
    private Transform playerTransform;                                                   // Transform of the player (the player's position)
    private float nextFireTime;                                                          // Time to spawn the next enemy bullet

    // Start method to initialize the enemy
    void Start()
    {
        currentHealth = maxHealth;
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        // If no firePoint is assigned, use enemy's position
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    // Update method to check if the player is within shooting range and shoot the enemy bullet
    void Update()
    {
        if (playerTransform == null) return;

        // Check if player is within shooting range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > shootRange) return;

        // Shoot at player when cooldown is ready
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    // Shoot method to shoot the enemy bullet
    void Shoot()
    {
        // Calculate direction towards player
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;

        // Instantiate bullet and initialize with direction and damage
        Projectile bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Init(direction, attackDamage);
    }

    // Take damage from projectiles
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    // Die method to destroy the enemy
    private void Die()
    {
        Destroy(gameObject);
    }

    // IHealth interface implementation
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Get the maximum health of the enemy
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
