using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy_Charge : MonoBehaviour, IHealth, IDamageable
{
    // Enemy states
    private enum ChargeState
    {
        Idle,           // Not detecting player
        Charging,       // Detected player, charging up (staying still)
        Dashing         // Charging complete, dashing towards player
    }

    [SerializeField] private int maxHealth = 5;                                          // Maximum health of the enemy
    [SerializeField] private int attackDamage = 2;                                       // Damage dealt to player on collision
    [SerializeField] private float dashSpeed = 20f;                                      // Speed when dashing towards player
    [SerializeField] private float detectionRange = 30f;                                 // Range to detect player
    [SerializeField] private float chargeTime = 1f;                                      // Time to charge before dashing
    [SerializeField] private float dashDuration = 2f;                                    // Duration of the dash
    [SerializeField] private float damageCooldown = 2f;                                  // Cooldown between damage to player
    
    private int currentHealth;                                                           // Current health of the enemy
    private Transform playerTransform;                                                   // Transform of the player (the player's position)
    private Rigidbody2D rb;                                                              // Rigidbody2D component of the enemy
    private float lastDamageTime;                                                        // Time when the enemy last damaged the player
    private ChargeState currentState = ChargeState.Idle;                                 // Current state of the enemy
    private float chargeStartTime;                                                       // Time when charging started
    private float dashStartTime;                                                         // Time when dashing started
    private Vector2 dashDirection;                                                       // Direction to dash towards player

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

    // FixedUpdate method to handle enemy behavior based on state
    void FixedUpdate()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // State machine logic
        switch (currentState)
        {
            case ChargeState.Idle:
                // Check if player is within detection range
                if (distanceToPlayer <= detectionRange)
                {
                    // Start charging
                    currentState = ChargeState.Charging;
                    chargeStartTime = Time.time;
                    dashDirection = ((Vector2)playerTransform.position - rb.position).normalized;
                }
                break;

            case ChargeState.Charging:
                // Update dash direction in case player moves during charge
                dashDirection = ((Vector2)playerTransform.position - rb.position).normalized;
                
                // Check if charge time is complete
                if (Time.time >= chargeStartTime + chargeTime)
                {
                    currentState = ChargeState.Dashing;
                    dashStartTime = Time.time;
                }
                // Stay still during charging
                break;

            case ChargeState.Dashing:
                // Check if dash duration has expired
                if (Time.time >= dashStartTime + dashDuration)
                {
                    currentState = ChargeState.Idle;
                    break;
                }

                // Move quickly towards player with obstacle avoidance
                Vector2 currentPos = rb.position;
                Vector2 nextPos = currentPos + dashDirection * dashSpeed * Time.fixedDeltaTime;
                rb.MovePosition(nextPos);

                // If player moved out of range or too far, reset to idle
                if (distanceToPlayer > detectionRange * 1.5f)
                {
                    currentState = ChargeState.Idle;
                }
                break;
        }
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
                    playerSettings.TakeDamage(attackDamage);
                    lastDamageTime = Time.time;
                    
                    // Reset to idle state after hitting player
                    currentState = ChargeState.Idle;
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
