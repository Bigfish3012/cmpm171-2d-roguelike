using UnityEngine;
using System.Collections.Generic;

public class SpinningHazard : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 180f;                                                   // Rotation speed in degrees per second

    [Header("Movement")]
    public float moveSpeed = 2f;                                                         // Movement speed
    public float moveDistanceX = 3f;                                                     // Horizontal movement range
    public float moveDistanceY = 1.5f;                                                   // Vertical movement range

    [Header("Damage")]
    public int damage = 1;                                                               // Damage dealt per hit
    public float damageCooldown = 0.35f;                                                 // Cooldown between hits on the same target

    private Vector3 startPos;                                                            // Initial position of the hazard
    private readonly Dictionary<int, float> lastHitTime = new Dictionary<int, float>();  // Tracks last hit time per target

    // Initialize the starting position
    void Start()
    {
        startPos = transform.position;
    }

    // Rotate and move the hazard in a ping-pong pattern each frame
    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        float offsetX = Mathf.PingPong(Time.time * moveSpeed, moveDistanceX) - moveDistanceX * 0.5f;
        float offsetY = Mathf.PingPong(Time.time * moveSpeed, moveDistanceY) - moveDistanceY * 0.5f;

        transform.position = new Vector3(
            startPos.x + offsetX,
            startPos.y + offsetY,
            startPos.z
        );
    }

    // Deal damage when a target first enters the trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    // Deal damage while a target stays inside the trigger
    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    // Try to deal damage to Player or Enemy if cooldown has elapsed
    void TryDealDamage(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Enemies"))
            return;

        int id = other.gameObject.GetInstanceID();
        float now = Time.time;

        if (lastHitTime.TryGetValue(id, out float lastTime))
        {
            if (now - lastTime < damageCooldown)
                return;
        }

        IDamageable dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            lastHitTime[id] = now;
        }
    }
}
