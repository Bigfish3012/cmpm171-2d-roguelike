using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy_shooter : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHealth = 20;                                         // Maximum health of the enemy
    [SerializeField] private int attackDamage = 1;                                      // Damage of the enemy bullet
    [SerializeField] private Projectile enemyBulletPrefab;                              // Prefab of the enemy bullet
    [SerializeField] private Transform firePoint;                                       // Point to spawn the enemy bullet
    [SerializeField] private float fireCooldown = 1.5f;                                 // Cooldown between each shot
    [SerializeField] private float shootRange = 5f;                                     // Range to spawn the enemy bullet (closer = must get nearer to player)
    [SerializeField] private float aimCancelRangeBuffer = 0.75f;                        // Extra distance before cancelling aim to prevent range-edge jitter
    [SerializeField] private float aimTime = 0.5f;                                     // Time to aim before firing
    [SerializeField] private float wanderSpeed = 1.5f;                                  // Speed when randomly wandering
    [SerializeField] private float wanderDirectionInterval = 2f;                        // Seconds before picking a new random direction
    [SerializeField] private float maxDistanceFromPlayer = 12f;                         // If too far, force move back to player
    [SerializeField] private float returnSpeed = 2.5f;                                  // Move speed when returning to player
    [SerializeField] private int experience = 2;                                        // Experience points given to player when killed
    [SerializeField] private GameObject damagePopUpPrefab;                             // Prefab for damage pop-up text (optional)
    [SerializeField] private Animator enemyAnimator;                                    // Animator used to play death animation
    [SerializeField] private AnimationClip deathAnimationClip;                          // Death animation clip (choose in Inspector)
    [SerializeField] private Transform visualToRotate;                                  // Visual transform to rotate toward player
    [SerializeField] private float aimAngleOffset = -90f;                               // Angle offset for sprite forward direction
    [SerializeField] private float deathRotationOffset = -135f;                         // Additional rightward rotation applied before death animation
    [SerializeField] private AudioClip gotHitClip;                                      // SFX when enemy takes damage
    [Range(0f, 1f)] [SerializeField] private float gotHitVolume = 1f;

    private int currentHealth;                                                           // Current health of the enemy
    private Transform playerTransform;                                                   // Transform of the player (the player's position)
    private Rigidbody2D rb;                                                              // Rigidbody2D for movement
    private float nextFireTime;                                                          // Time to spawn the next enemy bullet
    private float aimStartTime = -1f;                                                    // Time when aiming started (-1 = not aiming)
    private Vector2 wanderDirection;                                                     // Current random wander direction
    private float nextWanderDirectionTime;                                               // Time to pick next wander direction
    private bool isDead;                                                                 // Prevent repeated death logic
    private AudioSource _sfxSource;

    // Start method to initialize the enemy
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        EnsureSFXSource();
        if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
        if (enemyAnimator == null) enemyAnimator = GetComponentInChildren<Animator>();
        if (visualToRotate == null) visualToRotate = transform;

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
        if (isDead) return;
        RotateTowardPlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool playerInRange = distanceToPlayer <= shootRange;
        bool playerStillInAimWindow = distanceToPlayer <= shootRange + aimCancelRangeBuffer;

        // Aiming: player in range and we started aiming
        if (aimStartTime >= 0f)
        {
            if (!playerStillInAimWindow)
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

        // Prevent getting stuck far away from player.
        if (distanceToPlayer > maxDistanceFromPlayer)
        {
            MoveTowardPlayer(returnSpeed);
            return;
        }

        // Move toward player when not in shooting range to avoid wandering away.
        if (!playerInRange)
        {
            MoveTowardPlayer(wanderSpeed);
            return;
        }

        // Random wander only when already in shooting range but not currently aiming/firing.
        if (Time.time >= nextWanderDirectionTime)
        {
            PickNewWanderDirection();
            nextWanderDirectionTime = Time.time + wanderDirectionInterval;
        }
        Vector2 newPos = rb.position + wanderDirection * wanderSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    // Keep bottle mouth facing the player target while alive.
    private void RotateTowardPlayer()
    {
        if (visualToRotate == null) return;

        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)visualToRotate.position;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg + aimAngleOffset;
        visualToRotate.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // Shoot method to shoot the enemy bullet
    void Shoot()
    {
        // Calculate direction towards player
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;

        // Instantiate bullet and initialize with direction and damage
        Projectile bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        if (visualToRotate != null)
        {
            bullet.SetVisualRotation(visualToRotate.rotation);
        }
        bullet.Init(direction, attackDamage);
    }

    // Move toward player to keep shooter in combat area
    private void MoveTowardPlayer(float speed)
    {
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 newPos = rb.position + directionToPlayer * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    // Take damage from projectiles
    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (gotHitClip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(gotHitClip, gotHitVolume);

        if (damagePopUpPrefab != null)
        {
            GameObject popup = Instantiate(damagePopUpPrefab, transform.position, Quaternion.identity);
            DamagePopUp popupScript = popup.GetComponent<DamagePopUp>();
            if (popupScript != null)
            {
                popupScript.Init(damage, transform.position, isCrit);
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
        if (isDead) return;
        isDead = true;

        if (Player_settings.Instance != null)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddEnemyKill();
            Player_settings.Instance.AddExperience(experience);
        }

        if (rb != null) rb.simulated = false;
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D col in colliders) col.enabled = false;
        ApplyDeathRotation();

        float destroyDelay = 0f;
        if (enemyAnimator != null && deathAnimationClip != null)
        {
            enemyAnimator.Play(deathAnimationClip.name, 0, 0f);
            destroyDelay = deathAnimationClip.length;
        }

        Destroy(gameObject, Mathf.Max(0f, destroyDelay));
    }

    private void ApplyDeathRotation()
    {
        if (visualToRotate == null) return;
        visualToRotate.rotation *= Quaternion.Euler(0f, 0f, deathRotationOffset);
    }

    // IHealth interface implementation
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ApplyWaveScaling(float healthMultiplier, int damageBonus)
    {
        maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * Mathf.Max(0.1f, healthMultiplier)));
        attackDamage = Mathf.Max(1, attackDamage + damageBonus);
    }

    // Get the maximum health of the enemy
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private void EnsureSFXSource()
    {
        if (_sfxSource != null) return;
        _sfxSource = GetComponent<AudioSource>();
        if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.spatialBlend = 0f;
        _sfxSource.playOnAwake = false;
    }
}
