using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy1 : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float damageCooldown = 1f; // Cooldown between damage to player
    
    private int currentHealth;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private float lastDamageTime;

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

    void Update()
    {
        // Move towards player
        if (playerTransform != null)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Damage player when touching
        if (other.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                Player_settings playerSettings = other.GetComponent<Player_settings>();
                if (playerSettings != null)
                {
                    playerSettings.TakeDamage(1);
                    lastDamageTime = Time.time;
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
}
