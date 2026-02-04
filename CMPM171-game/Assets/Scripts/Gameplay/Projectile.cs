using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return; // Don't destroy the projectile if it hits the player
        
        // Damage enemy if it's an enemy
        if (other.CompareTag("Enemies"))
        {
            Enemy1 enemy = other.GetComponent<Enemy1>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        Destroy(gameObject);
    }
}
