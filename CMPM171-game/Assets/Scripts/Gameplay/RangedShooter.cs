using UnityEngine;

public class RangedShooter : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.2f;

    private float nextFireTime;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        // Bullet direction
        Vector2 dir = firePoint.right;

        Projectile p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        p.Init(dir);
    }
}
