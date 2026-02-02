using UnityEngine;

// Put on the same GameObject as the trigger Collider2D (e.g. player body). When enemy touches, deal 1 damage.
public class PlayerDamageReceiver : MonoBehaviour
{
    [SerializeField] private float damageCooldown = 1f;

    private float lastDamageTime;
    private PlayerHealth health;

    void Awake()
    {
        health = GetComponentInParent<PlayerHealth>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    void TryDamage(Collider2D other)
    {
        if (health == null || other.GetComponent<EnemyHealth>() == null) return;
        if (damageCooldown > 0 && Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        health.TakeDamage(1);
    }
}
