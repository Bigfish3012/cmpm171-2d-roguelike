using UnityEngine;
using System.Collections.Generic;

public class SpinningHazard : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 180f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float moveDistanceX = 3f;   // 左右移动范围
    public float moveDistanceY = 1.5f; // 上下移动范围

    [Header("Damage")]
    public int damage = 1;
    public float damageCooldown = 0.35f;

    private Vector3 startPos;

    // 记录每个物体上次受伤时间
    private readonly Dictionary<int, float> lastHitTime = new Dictionary<int, float>();

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 1️⃣ 旋转
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // 2️⃣ 左右 + 上下 同时移动
        float offsetX = Mathf.PingPong(Time.time * moveSpeed, moveDistanceX) - moveDistanceX * 0.5f;
        float offsetY = Mathf.PingPong(Time.time * moveSpeed, moveDistanceY) - moveDistanceY * 0.5f;

        transform.position = new Vector3(
            startPos.x + offsetX,
            startPos.y + offsetY,
            startPos.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    void TryDealDamage(Collider2D other)
    {
        // 只伤害 Player / Enemy
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
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