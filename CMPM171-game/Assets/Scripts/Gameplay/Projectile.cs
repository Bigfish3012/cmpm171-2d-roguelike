using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;                                                            // Speed of the projectile
    public float lifeTime = 2f;                                                          // Lifetime of the projectile before it gets destroyed
    public float ricochetSearchRadius = 8f;                                              // Search radius for next ricochet target
    private int damage;                                                                  // Damage dealt by the projectile
    private bool isCrit;                                                                 // Whether this hit is a critical hit
    private int remainingRicochets;                                                      // Remaining chained ricochets
    private readonly HashSet<int> hitTargets = new HashSet<int>();                       // Prevent repeat hits on same target

    private Rigidbody2D rb;                                                              // Rigidbody2D component of the projectile
    private bool _hasCustomRotation;

    // Awake method to initialize the Rigidbody2D component
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Init method to initialize the projectile with direction, damage, and crit flag
    public void Init(Vector2 dir, int projectileDamage = 1, bool projectileIsCrit = false, int projectileRicochets = 0)
    {
        damage = projectileDamage;
        isCrit = projectileIsCrit;
        remainingRicochets = Mathf.Max(0, projectileRicochets);
        hitTargets.Clear();
        Vector2 normalizedDirection = dir.normalized;
        rb.linearVelocity = normalizedDirection * speed;

        if (!_hasCustomRotation && normalizedDirection.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Destroy(gameObject, lifeTime);
    }

    public void SetVisualRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
        _hasCustomRotation = true;
    }

    // OnTriggerEnter2D method to damage objects that implement IDamageable
    void OnTriggerEnter2D(Collider2D other)
    {
        // Try to damage the object if it implements IDamageable
        // Layer collision matrix handles faction separation (PlayerProjectile vs Player, EnemyProjectile vs Enemies)
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            int targetId = other.GetInstanceID();
            if (hitTargets.Contains(targetId))
                return;

            hitTargets.Add(targetId);
            damageable.TakeDamage(damage, isCrit);
            if (!TryRicochet(other))
                Destroy(gameObject);
        }
    }

    private bool TryRicochet(Collider2D currentTarget)
    {
        if (remainingRicochets <= 0)
            return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ricochetSearchRadius);
        Collider2D nextTarget = null;
        float closestDistanceSq = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D candidate = hits[i];
            if (candidate == null || candidate == currentTarget)
                continue;

            if (hitTargets.Contains(candidate.GetInstanceID()))
                continue;

            if (candidate.GetComponent<Player_settings>() != null)
                continue;

            if (candidate.GetComponent<IDamageable>() == null)
                continue;

            float distanceSq = ((Vector2)candidate.bounds.center - (Vector2)transform.position).sqrMagnitude;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                nextTarget = candidate;
            }
        }

        if (nextTarget == null)
            return false;

        remainingRicochets--;

        Vector2 nextDirection = ((Vector2)nextTarget.bounds.center - (Vector2)transform.position).normalized;
        rb.linearVelocity = nextDirection * speed;

        if (!_hasCustomRotation && nextDirection.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(nextDirection.y, nextDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        transform.position = (Vector2)transform.position + nextDirection * 0.2f;
        return true;
    }
}
