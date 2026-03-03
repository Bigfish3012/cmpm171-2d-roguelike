using UnityEngine;

public class RangedShooter : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10;                                     // Damage of the projectile
    [SerializeField] private Projectile projectilePrefab;                               // Prefab of the projectile
    [SerializeField] private Transform firePoint;                                       // Point to spawn the projectile
    [SerializeField] private float fireCooldown = 3f;                                   // Cooldown between each shot
    [SerializeField] private GunAim gunAim;                                             // Gun aiming state for target checks

    private float nextFireTime;                                                         // Time to spawn the next projectile

    // NEW: damage multiplier (1 = normal, 0.9 = -10% damage)
    private float damageMultiplier = 1f;

    void Awake()
    {
        if (gunAim == null)
            gunAim = GetComponentInChildren<GunAim>();
    }

    void Update()
    {
        if (gunAim == null || !gunAim.HasTarget) return;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        Vector2 dir = firePoint.right;

        // NEW: apply damage multiplier BEFORE crit calculation
        int scaledBaseDamage = Mathf.Max(0, Mathf.RoundToInt(attackDamage * damageMultiplier));

        int finalDamage = scaledBaseDamage;
        bool isCrit = false;

        if (Player_settings.Instance != null)
        {
            var result = Player_settings.Instance.CalculateDamageWithCrit(scaledBaseDamage);
            finalDamage = result.damage;
            isCrit = result.isCrit;
        }

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

    // NEW: used by penalty system (e.g. -0.10f => -10% damage)
    public void AddDamageMultiplier(float amount)
    {
        damageMultiplier = Mathf.Clamp(damageMultiplier + amount, 0.1f, 5f);
        // NOTE: not saved/restored yet (kept simple)
    }

    // For GameManager persistence
    public int GetAttackDamage() => attackDamage;
    public void SetAttackDamage(int value) => attackDamage = value;
}