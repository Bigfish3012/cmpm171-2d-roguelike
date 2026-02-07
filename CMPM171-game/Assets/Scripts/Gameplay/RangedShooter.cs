using UnityEngine;

public class RangedShooter : MonoBehaviour
{
    [SerializeField] private int attackDamage = 1;                                      // Damage of the projectile
    [SerializeField] private Projectile projectilePrefab;                               // Prefab of the projectile
    [SerializeField] private Transform firePoint;                                       // Point to spawn the projectile
    [SerializeField] private float fireCooldown = 0.2f;                                 // Cooldown between each shot

    private float nextFireTime;                                                          // Time to spawn the next projectile

    // Update method to check for input and shoot when cooldown is ready
    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
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

        // Instantiate bullet and initialize with direction and damage
        Projectile p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        p.Init(dir, attackDamage);
    }
}