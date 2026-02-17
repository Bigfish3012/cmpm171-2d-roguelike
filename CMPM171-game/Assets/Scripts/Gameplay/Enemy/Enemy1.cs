using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy1 : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHealth = 3;                                          // Maximum health of the enemy
    [SerializeField] private float moveSpeed = 3f;                                       // Movement speed of the enemy
    [SerializeField] private float damageCooldown = 1f;                                  // Cooldown between damage to player
    [SerializeField] private float avoidanceRadius = 2f;                                 // Radius to detect other enemies for avoidance
    [SerializeField] private float avoidanceStrength = 2f;                               // Strength of avoidance force
    [SerializeField] private int experience = 1;                                         // Experience points given to player when killed
    [SerializeField] private GameObject damagePopUpPrefab;                               // Prefab for damage pop-up text (optional)

    private int currentHealth;                                                           // Current health of the enemy
    private Transform playerTransform;                                                   // Transform of the player (the player's position)
    private Rigidbody2D rb;                                                              // Rigidbody2D component of the enemy
    private float lastDamageTime;                                                        // Time when the enemy last damaged the player
    private float stopUntilTime = 0f;                                                    // Time until enemy can move again after hitting player

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
    }

    // FixedUpdate method to move the enemy towards the player with avoidance
    void FixedUpdate()
    {
        if (playerTransform == null) return;
        
        // Stop moving if still in cooldown after hitting player
        if (Time.time < stopUntilTime) return;

        Vector2 currentPos = rb.position;
        Vector2 targetPos = (Vector2)playerTransform.position;

        // Calculate direction towards player
        Vector2 direction = (targetPos - currentPos).normalized;
        
        // Apply avoidance behavior
        Vector2 avoidanceVector = CalculateAvoidance(currentPos);
        
        // Combine direction towards player with avoidance
        Vector2 finalDirection = (direction + avoidanceVector).normalized;
        Vector2 nextPos = currentPos + finalDirection * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPos);
    }
    
    // Calculate avoidance vector to avoid other enemies
    private Vector2 CalculateAvoidance(Vector2 currentPos)
    {
        Vector2 avoidance = Vector2.zero;
        
        // Find all enemies within avoidance radius
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(currentPos, avoidanceRadius);
        
        foreach (Collider2D col in nearbyEnemies)
        {
            // Skip self and non-enemy objects
            if (col.gameObject == gameObject) continue;
            if (!col.CompareTag("Enemies")) continue;
            
            // Calculate separation vector
            Vector2 toEnemy = (Vector2)col.transform.position - currentPos;
            float distance = toEnemy.magnitude;
            
            if (distance > 0f && distance < avoidanceRadius)
            {
                // Calculate avoidance force (stronger when closer)
                float avoidanceForce = avoidanceStrength * (1f - (distance / avoidanceRadius));
                avoidance -= toEnemy.normalized * avoidanceForce;
            }
        }
        
        return avoidance;
    }

    // OnTriggerEnter2D method to damage player when triggering
    void OnTriggerEnter2D(Collider2D other)
    {
        // Damage player when triggering
        if (other.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                Player_settings playerSettings = other.GetComponent<Player_settings>();
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

        if (damagePopUpPrefab != null)
        {
            GameObject popup = Instantiate(damagePopUpPrefab, transform.position, Quaternion.identity);
            DamagePopUp popupScript = popup.GetComponent<DamagePopUp>();
            if (popupScript != null)
            {
                popupScript.Init(damage, transform.position);
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Die method to destroy the enemy and give experience to player
    private void Die()
    {
        if (Player_settings.Instance != null)
        {
            Player_settings.Instance.AddExperience(experience);
        }
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
