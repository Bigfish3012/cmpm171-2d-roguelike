using UnityEngine;

public class Enemy_shooter : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Projectile enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 1.5f;
    [SerializeField] private float shootRange = 10f; // Only shoot if player is within range

    private int currentHealth;
    private Transform playerTransform;
    private float nextFireTime;

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

    private void Die()
    {
        Destroy(gameObject);
    }

    // IHealth interface implementation
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
