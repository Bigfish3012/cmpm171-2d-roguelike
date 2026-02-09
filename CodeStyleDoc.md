# Code Style Documentation

## Naming Rules

### Code
- **Classes, methods, enumerations, public fields, public properties, namespaces**: PascalCase
- **Local variables, parameters**: camelCase
- **Interfaces**: Start with `I`, such as `IHealth`, `IDamageable`

### Files
- In general, filenames and directory names are: `PascalCase.cs`
- For game objects (enemy/player), name them as: `GameObject_function.cs`
  - Examples:
    - `Enemy_shooter.cs`
    - `Player_settings.cs`
    - `Player_health.cs`
    - `Enemy_healthbar.cs`
- Name of interface classes: `I<values>`
  - Example: `IHealth`, `IDamageable`
- Generally, one file will only have one class.

### Scenes
- `MainMenu`
- `GameOver`
- `Pause`
- `Setting`
- `AboutTheTeam`

## Whitespace Rules
- Use tabs for indentation, or 4 spaces
- Column limit: 150 characters (no longer than the screen)
- Brace Styling: Allman style
- Leave a blank line before and after each function

## Comment Rules
- Leave a comment explaining what it does before each function
- If there are more than 3 variables on one line of code, leave a comment explaining
- Add a comment to the right of each variable, and align them vertically

---
### Complete Example

```csharp
using UnityEngine;

public class Enemy_shooter : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int maxHealth = 2;                                         // Maximum health of the enemy
    [SerializeField] private int attackDamage = 1;                                      // Damage of the enemy bullet
    [SerializeField] private Projectile enemyBulletPrefab;                              // Prefab of the enemy bullet
    [SerializeField] private Transform firePoint;                                       // Point to spawn the enemy bullet

    private int currentHealth;                                                           // Current health of the enemy
    private Transform playerTransform;                                                   // Transform of the player

    // Start method to initialize the enemy
    void Start()
    {
        currentHealth = maxHealth;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // Update method to check if the player is within shooting range and shoot
    void Update()
    {
        if (playerTransform == null) return;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= shootRange)
        {
            Shoot();
        }
    }

    // Shoot method to shoot the enemy bullet
    void Shoot()
    {
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;
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
}
```

