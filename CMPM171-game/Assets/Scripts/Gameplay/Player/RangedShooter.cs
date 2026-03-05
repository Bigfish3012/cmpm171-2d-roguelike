using UnityEngine;

public class RangedShooter : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10;                                     // Damage of the projectile
    [SerializeField] private int projectileCount = 1;                                   // Number of projectiles fired each shot
    [SerializeField] private float projectileSpreadAngle = 12f;                         // Total spread angle when firing multiple projectiles
    [SerializeField] private Projectile projectilePrefab;                               // Prefab of the projectile
    [SerializeField] private Transform firePoint;                                       // Point to spawn the projectile
    [SerializeField] private float fireCooldown = 3f;                                   // Cooldown between each shot
    [SerializeField] private GunAim gunAim;                                             // Gun aiming state for target checks
    [SerializeField] private AudioClip fireSoundClip;                                    // SFX played when firing
    [Range(0f, 1f)] [SerializeField] private float fireSoundVolume = 1f;

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
        if (fireSoundClip != null)
        {
            Vector3 pos = firePoint != null ? firePoint.position : transform.position;
            AudioSource.PlayClipAtPoint(fireSoundClip, pos, fireSoundVolume);
        }

        Vector2 baseDir = firePoint.right;

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

        int count = Mathf.Max(1, projectileCount);
        float step = count > 1 ? projectileSpreadAngle / (count - 1) : 0f;
        float startAngle = -projectileSpreadAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angleOffset = startAngle + (step * i);
            Vector2 dir = (Quaternion.Euler(0f, 0f, angleOffset) * baseDir).normalized;
            Projectile p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            p.Init(dir, finalDamage, isCrit);
        }
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

    public void AddProjectileCount(int amount)
    {
        projectileCount = Mathf.Max(1, projectileCount + amount);
        if (GameManager.Instance != null && Player_settings.Instance != null)
            GameManager.Instance.SaveFrom(Player_settings.Instance, GetComponent<PlayerController>(), this);
    }

    // For GameManager persistence
    public int GetAttackDamage() => attackDamage;
    public void SetAttackDamage(int value) => attackDamage = value;
    public int GetProjectileCount() => projectileCount;
    public void SetProjectileCount(int value) => projectileCount = Mathf.Max(1, value);
}
