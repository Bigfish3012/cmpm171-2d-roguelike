using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 2f;

    [Header("Hit Filter")]
    [SerializeField] private string enemyTag = "Enemies";
    //[SerializeField] private string obstacleTag = "Obstacle";

    private Rigidbody2D rb;
    private bool dead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Init(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        rb.linearVelocity = dir.normalized * speed;

        CancelInvoke();
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dead) return;

        if (other.CompareTag("Player")) return;

        // Hit enemy: deal damage
        if (other.CompareTag(enemyTag))
        {
            dead = true;

            // Deal damage to enemy
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1);
            }

            Destroy(gameObject);
            return;
        }

        // Hit obstacle: destroy
        // if (!string.IsNullOrEmpty(obstacleTag) && other.CompareTag(obstacleTag))
        // {
        //     dead = true;
        //     Destroy(gameObject);
        // }
    }
}
