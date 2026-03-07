using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            IDamageable dmg = collision.gameObject.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}