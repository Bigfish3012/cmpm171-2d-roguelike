using UnityEngine;

public class RangedShooter : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10;                                      // Damage of the projectile
    [SerializeField] private Projectile projectilePrefab;                               // Prefab of the projectile
    [SerializeField] private Transform firePoint;                                       // Point to spawn the projectile
    [SerializeField] private float fireCooldown = 3f;                                 // Cooldown between each shot
    [SerializeField] private GunAim gunAim;                                              // Gun aiming state for target checks

    private float nextFireTime;                                                          // Time to spawn the next projectile

    void Awake()
    {
        if (gunAim == null)
            gunAim = GetComponentInChildren<GunAim>();
    }

    // Update method to auto-fire when cooldown is ready
    void Update()
    {
        if (gunAim == null || !gunAim.HasTarget) return;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    // Shoot method to shoot the projectile
    void Shoot()
    {
        // Calculate bullet direction
        Vector2 dir = firePoint.right;

        // Apply crit calculation if Player_settings exists
        int finalDamage = attackDamage;
        bool isCrit = false;
        if (Player_settings.Instance != null)
        {
            var result = Player_settings.Instance.CalculateDamageWithCrit(attackDamage);
            finalDamage = result.damage;
            isCrit = result.isCrit;
        }

        // Instantiate bullet and initialize with direction, damage, and crit flag
        Projectile p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        p.Init(dir, finalDamage, isCrit);
    }

    // Upgrade method for Level Up Menu
    public void AddAttackDamage(int amount)
    {
        attackDamage += amount;
        if (GameManager.Instance != null && Player_settings.Instance != null)
            GameManager.Instance.SaveFrom(Player_settings.Instance, GetComponent<PlayerController>(), this);
    }

    // For GameManager persistence
    public int GetAttackDamage() => attackDamage;
    public void SetAttackDamage(int value) => attackDamage = value;
}