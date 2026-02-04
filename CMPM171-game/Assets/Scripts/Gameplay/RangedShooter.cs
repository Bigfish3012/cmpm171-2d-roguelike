using UnityEngine;

public class RangedShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Transform firePoint;   // The point from which the projectile will be spawned
    public float fireCooldown = 0.2f;

    private float nextFireTime;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            ShootToMouse();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void ShootToMouse()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouse - transform.position);
        dir.Normalize();

        Projectile p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        p.Init(dir);
    }
}
