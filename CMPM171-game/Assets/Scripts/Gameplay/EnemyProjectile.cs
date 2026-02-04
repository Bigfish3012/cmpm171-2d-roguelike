using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 2f;

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

        // Ignore enemies (don't damage other enemies)
        if (other.CompareTag("Enemies")) return;

        // Hit player: deal damage
        if (other.CompareTag("Player"))
        {
            dead = true;

            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(1);
            }

            Destroy(gameObject);
            return;
        }
    }
}
