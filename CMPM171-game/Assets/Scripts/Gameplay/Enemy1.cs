using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy1 : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float damageCooldown = 1f; // Cooldown between damage to player
    
    private int currentHealth;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private float lastDamageTime;
    private float stopUntilTime = 0f; // Time until enemy can move again after hitting player

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
                    stopUntilTime = Time.time + 0.5f; // Stop moving for 1 second
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

    private void Die()
    {
        Destroy(gameObject);
    }

    // Public getters for health bar
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
