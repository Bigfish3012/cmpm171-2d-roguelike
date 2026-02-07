using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy1 : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHealth = 3;                                          // Maximum health of the enemy
    [SerializeField] private float moveSpeed = 3f;                                       // Movement speed of the enemy
    [SerializeField] private float damageCooldown = 1f;                                  // Cooldown between damage to player
    
    private int currentHealth;                                                           // Current health of the enemy
    private Transform playerTransform;                                                   // Transform of the player (the player's position)
    private Rigidbody2D rb;                                                              // Rigidbody2D component of the enemy
    private float lastDamageTime;                                                        // Time when the enemy last damaged the player
    private float stopUntilTime = 0f;                                                   // Time until enemy can move again after hitting player

    // Start method to initialize the enemy
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // FixedUpdate method to move the enemy towards the player
    void FixedUpdate()
    {
        if (playerTransform == null) return;
        
        // Stop moving if still in cooldown after hitting player
        if (Time.time < stopUntilTime) return;

        Vector2 currentPos = rb.position;
        Vector2 targetPos = (Vector2)playerTransform.position;

        Vector2 direction = (targetPos - currentPos).normalized;
        Vector2 nextPos = currentPos + direction * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPos);
    }

    // OnCollisionEnter2D method to damage player when colliding
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Damage player when colliding
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                Player_settings playerSettings = collision.gameObject.GetComponent<Player_settings>();
                if (playerSettings != null)
                {
                    playerSettings.TakeDamage(1);
                    lastDamageTime = Time.time;
                    stopUntilTime = Time.time + 0.5f; // Stop moving for 0.5 seconds
                }
            }
        }
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

    // Get the current health of the enemy
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
