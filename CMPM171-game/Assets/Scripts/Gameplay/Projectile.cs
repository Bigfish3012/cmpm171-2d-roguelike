using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;

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
        Destroy(gameObject);
    }
}
