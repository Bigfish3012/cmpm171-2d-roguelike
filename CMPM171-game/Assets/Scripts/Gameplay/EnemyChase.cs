using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : MonoBehaviour
{
    public float speed = 2f;
    public float chaseRange = 6f;

    private Rigidbody2D rb;
    private Transform player;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float dist = toPlayer.magnitude;

        if (dist > chaseRange) return;

        Vector2 dir = toPlayer.normalized;
        Vector2 newPos = rb.position + dir * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
}
