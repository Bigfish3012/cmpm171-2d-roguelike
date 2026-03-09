using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BossAI : MonoBehaviour, IHealth, IDamageable
{
    [Header("References")]
    public Animator animator;
    public GameObject fireballPrefab;
    public Transform firePoint;

    [Header("Attack Timing")]
    public float attackInterval = 5f;     // 每隔几秒攻击一次
    public float attackDelay = 0.8f;      // 播放攻击动画后，延迟多久发射火球
    public float attackEndDelay = 2f;     // 攻击状态持续多久后结束

    [Header("Fireball Settings")]
    public int fireballCount = 5;         // 一次发射多少颗火球
    public float spreadAngle = 60f;       // 扇形角度
    public float fireballSpeed = 6f;      // 火球速度

    [Header("Target")]
    public Transform player;              // 玩家，用于决定朝向

    [Header("Health")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private GameObject damagePopUpPrefab;
    [SerializeField] private AnimationClip deathAnimationClip;

    private int currentHealth;
    private float attackTimer;
    private bool isAttacking = false;
    private bool facingRight = true;
    private bool isDead = false;

    private Rigidbody2D rb;

    private void Start()
    {
        currentHealth = maxHealth;
        attackTimer = attackInterval;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (player != null)
        {
            UpdateFacing();
        }

        if (isAttacking)
        {
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            StartAttack();
        }
    }

    private void UpdateFacing()
    {
        if (player.position.x > transform.position.x && !facingRight)
        {
            facingRight = true;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (player.position.x < transform.position.x && facingRight)
        {
            facingRight = false;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void StartAttack()
    {
        if (isDead)
        {
            return;
        }

        isAttacking = true;
        attackTimer = attackInterval;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        CancelInvoke(nameof(FireBurst));
        CancelInvoke(nameof(EndAttack));

        Invoke(nameof(FireBurst), attackDelay);
        Invoke(nameof(EndAttack), attackEndDelay);
    }

    private void FireBurst()
    {
        if (isDead)
        {
            return;
        }

        if (fireballPrefab == null || firePoint == null)
        {
            Debug.LogWarning("BossAI: fireballPrefab or firePoint is not assigned.");
            return;
        }

        float startAngle = -spreadAngle * 0.5f;
        float angleStep = (fireballCount > 1) ? spreadAngle / (fireballCount - 1) : 0f;

        for (int i = 0; i < fireballCount; i++)
        {
            float angle = startAngle + angleStep * i;
            float finalAngle = facingRight ? angle : 180f - angle;

            Quaternion rotation = Quaternion.Euler(0f, 0f, finalAngle);
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, rotation);

            Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
            if (fireballRb != null)
            {
                Vector2 direction = rotation * Vector2.right;
                fireballRb.linearVelocity = direction.normalized * fireballSpeed;
            }
        }
    }

    private void EndAttack()
    {
        if (isDead)
        {
            return;
        }

        isAttacking = false;
    }

    // =========================
    // Damage / Health System
    // =========================
    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (damagePopUpPrefab != null)
        {
            GameObject popup = Instantiate(damagePopUpPrefab, transform.position, Quaternion.identity);
            DamagePopUp popupScript = popup.GetComponent<DamagePopUp>();
            if (popupScript != null)
            {
                popupScript.Init(damage, transform.position, isCrit);
            }
        }

        Debug.Log("Boss HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isAttacking = false;

        CancelInvoke(nameof(FireBurst));
        CancelInvoke(nameof(EndAttack));

        if (rb != null)
        {
            rb.simulated = false;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        float destroyDelay = 0f;

        if (animator != null && deathAnimationClip != null)
        {
            animator.Play(deathAnimationClip.name, 0, 0f);
            destroyDelay = deathAnimationClip.length;
        }

        Destroy(gameObject, Mathf.Max(0f, destroyDelay));
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}