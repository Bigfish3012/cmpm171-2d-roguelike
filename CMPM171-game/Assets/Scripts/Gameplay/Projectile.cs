using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;                                                            // Speed of the projectile
    public float lifeTime = 2f;                                                          // Lifetime of the projectile before it gets destroyed
    private int damage;                                                                  // Damage dealt by the projectile
    private bool isCrit;                                                                 // Whether this hit is a critical hit

    private Rigidbody2D rb;                                                              // Rigidbody2D component of the projectile
    private bool _hasCustomRotation;

    // Awake method to initialize the Rigidbody2D component
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Init method to initialize the projectile with direction, damage, and crit flag
    public void Init(Vector2 dir, int projectileDamage = 1, bool projectileIsCrit = false)
    {
        damage = projectileDamage;
        isCrit = projectileIsCrit;
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
            damageable.TakeDamage(damage, isCrit);
            Destroy(gameObject);
        }
    }
}
