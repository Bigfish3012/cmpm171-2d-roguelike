using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 1.5f;
    [SerializeField] private float shootRange = 8f;

    [Header("Aiming")]
    [SerializeField] private bool rotateToFacePlayer = true;

    private Transform player;
    private float nextFireTime;

    void Start()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        // If no firePoint is assigned, use this transform
        if (firePoint == null) firePoint = transform;
    }

    void Update()
    {
        if (player == null) return;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float distance = toPlayer.magnitude;

        // Only shoot if player is within range
        if (distance > shootRange) return;

        // Rotate to face player if enabled
        if (rotateToFacePlayer)
        {
            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Shoot at intervals
        if (Time.time >= nextFireTime)
        {
            Shoot(toPlayer.normalized);
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot(Vector2 direction)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("EnemyShooter: Projectile prefab is not assigned!");
            return;
        }

        Vector2 spawnPosition = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        EnemyProjectile p = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        p.Init(direction);
    }
}
