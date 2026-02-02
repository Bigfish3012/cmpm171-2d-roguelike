using UnityEngine;

// Moves enemy toward the player. Uses the player's Rigidbody2D transform so we follow the moving body, not a static root.
public class EnemyMoveTowardPlayer : MonoBehaviour
{
    [SerializeField] private float speed = 2f;

    private Transform target;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        RefreshTarget();
    }

    void RefreshTarget()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go == null) return;
        var playerRb = go.GetComponentInChildren<Rigidbody2D>();
        target = playerRb != null ? playerRb.transform : go.transform;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            RefreshTarget();
            if (target == null) return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        if (rb != null)
            rb.linearVelocity = dir * speed;
        else
            transform.position += (Vector3)(dir * speed * Time.fixedDeltaTime);
    }
}
