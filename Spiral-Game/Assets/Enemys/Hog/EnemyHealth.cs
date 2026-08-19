using UnityEngine;
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;

    private int currentHealth;

    private Rigidbody2D rb;

    private Animator animator;
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;

        EnemyPatrol enemyPatrol =
    GetComponent<EnemyPatrol>();

        if (enemyPatrol != null)
        {
            enemyPatrol.ApplyKnockback(
                knockbackDirection * 5f
            );
        }

        rb.AddForce(
            knockbackDirection * 5f,
            ForceMode2D.Impulse
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("Die");

        GetComponent<EnemyPatrol>().enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 1f);
    }
}