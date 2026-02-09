using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy_shooter : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHealth = 2;                                         // Maximum health of the enemy
    [SerializeField] private int attackDamage = 1;                                      // Damage of the enemy bullet
    [SerializeField] private Projectile enemyBulletPrefab;                              // Prefab of the enemy bullet
    [SerializeField] private Transform firePoint;                                       // Point to spawn the enemy bullet
    [SerializeField] private float fireCooldown = 1.5f;                                 // Cooldown between each shot
    [SerializeField] private float shootRange = 10f;                                    // Range to spawn the enemy bullet
    [SerializeField] private float aimTime = 0.5f;                                     // Time to aim before firing
    [SerializeField] private float wanderSpeed = 1.5f;                                  // Speed when randomly wandering
    [SerializeField] private float wanderDirectionInterval = 2f;                        // Seconds before picking a new random direction

    private int currentHealth;                                                           // Current health of the enemy
    private Transform playerTransform;                                                   // Transform of the player (the player's position)
    private Rigidbody2D rb;                                                              // Rigidbody2D for movement
    private float nextFireTime;                                                          // Time to spawn the next enemy bullet
    private float aimStartTime = -1f;                                                    // Time when aiming started (-1 = not aiming)
    private Vector2 wanderDirection;                                                     // Current random wander direction
    private float nextWanderDirectionTime;                                               // Time to pick next wander direction

    // Start method to initialize the enemy
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        // Get player transform from singleton
        if (Player_settings.Instance != null)
        {
            playerTransform = Player_settings.Instance.PlayerTransform;
        }
        // If no firePoint is assigned, use enemy's position
        if (firePoint == null)
        {
            firePoint = transform;
        }

        PickNewWanderDirection();
        nextWanderDirectionTime = Time.time + wanderDirectionInterval;
    }

    // Pick a new random direction for wandering
    private void PickNewWanderDirection()
    {
        wanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        if (wanderDirection.sqrMagnitude < 0.01f) wanderDirection = Vector2.right;
    }

    // FixedUpdate method to aim, shoot, or wander based on player distance
    void FixedUpdate()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool playerInRange = distanceToPlayer <= shootRange;

        // Aiming: player in range and we started aiming
        if (aimStartTime >= 0f)
        {
            if (!playerInRange)
            {
                aimStartTime = -1f;
            }
            else if (Time.time >= aimStartTime + aimTime)
            {
                Shoot();
                nextFireTime = Time.time + fireCooldown;
                aimStartTime = -1f;
            }
            // Stay still while aiming
            return;
        }

        // Start aiming when player in range and cooldown ready
        if (playerInRange && Time.time >= nextFireTime)
        {
            aimStartTime = Time.time;
            return;
        }

        // Random wander when not aiming
        if (Time.time >= nextWanderDirectionTime)
        {
            PickNewWanderDirection();
            nextWanderDirectionTime = Time.time + wanderDirectionInterval;
        }
        Vector2 newPos = rb.position + wanderDirection * wanderSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
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
