//new enemy script
using UnityEngine;
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Detection")]
    public float frontDetectionRange = 6f;
    public float backDetectionRange = 2f;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public int damage = 1;

    private Transform target;
    private Rigidbody2D rb;

    private bool isChasing;
    private bool facingRight = true;

    public float attackCooldown = 1f;
    private float attackTimer;

    public float attackStopTime = 0.4f;
    private float attackStopTimer;

    private Animator animator;

    [Header("Knockback")]
    public float stunDuration = 0.2f;

    private float stunTimer;
    void Start()
    {
        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
        target = pointB;
    }

    void Update()
    {
        if (attackStopTimer > 0)
        {
            attackStopTimer -= Time.deltaTime;

            rb.linearVelocity = new Vector2(
                0,
                rb.linearVelocity.y
            );

            return;
        }

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        float distanceToPlayer = Vector2.Distance(
            transform.position,
            player.position
        );

        bool playerInsidePatrolArea =
            player.position.x >= Mathf.Min(pointA.position.x, pointB.position.x) &&
            player.position.x <= Mathf.Max(pointA.position.x, pointB.position.x);

        bool playerInFront =
            (facingRight && player.position.x > transform.position.x) ||
            (!facingRight && player.position.x < transform.position.x);

        float currentDetectionRange =
            playerInFront
            ? frontDetectionRange
            : backDetectionRange;

        if (distanceToPlayer <= currentDetectionRange &&
            playerInsidePatrolArea)
        {
            isChasing = true;
        }
        else if (distanceToPlayer > currentDetectionRange ||
                 !playerInsidePatrolArea)
        {
            isChasing = false;
        }

        // CHASE
        if (isChasing)
        {
            float direction = Mathf.Sign(
                player.position.x - transform.position.x
            );

            rb.linearVelocity = new Vector2(
                direction * moveSpeed,
                rb.linearVelocity.y
            );

            UpdateFacing(direction);
        }
        // PATROL
        else
        {
            float direction = Mathf.Sign(
                target.position.x - transform.position.x
            );

            rb.linearVelocity = new Vector2(
                direction * moveSpeed,
                rb.linearVelocity.y
            );

            UpdateFacing(direction);

            if (Mathf.Abs(
                transform.position.x - target.position.x) < 0.1f)
            {
                if (target == pointA)
                    target = pointB;
                else
                    target = pointA;
            }
        }

        attackTimer -= Time.deltaTime;

        if (distanceToPlayer <= attackRange)
        {

            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (attackTimer <= 0)
            {
                animator.SetTrigger("Attack");

                PlayerController playerController =
                    player.GetComponent<PlayerController>();


                Vector2 knockbackDirection =(player.position -transform.position).normalized;

                knockbackDirection.y = 0.5f;

                playerController.TakeDamage(damage,knockbackDirection);

                attackTimer = attackCooldown;

                attackStopTimer = attackStopTime;
            }


            //return;
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        rb.linearVelocity = Vector2.zero;

        rb.AddForce(force, ForceMode2D.Impulse);

        stunTimer = stunDuration;
    }
    private void UpdateFacing(float direction)
    {
        if (direction > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingRight = true;
        }
        else if (direction < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingRight = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null)
            return;

        // Patrol-Bereich
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);

        // Sichtbereich
        Gizmos.color = Color.red;

        float range = facingRight
            ? frontDetectionRange
            : backDetectionRange;

        Gizmos.DrawWireSphere(transform.position, range);
    }
}