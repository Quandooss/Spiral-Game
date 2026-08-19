//New Movement Ipnut 0.4 // jump delay and Character Turn added
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 10f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;


    [Header("Health")]
    public int maxHealth = 5;

    private int currentHealth;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public int attackDamage = 1;
    public LayerMask enemyLayer;
    public LayerMask DestroyableLayer;

    public float attackCooldown = 0.5f;

    private float attackTimer;


    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // NEU
    private bool isGrounded;

    //Jump Variants short and long
    public float jumpCutMultiplier = 0.5f;

    public int CurrentHealth
    {
        get { return currentHealth; }
    }
    public int MaxHealth
    {
        get { return maxHealth; }
    }

    private Vector3 spawnPosition;

    private Animator animator;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float stunDuration = 0.2f;

    private float stunTimer;
    void Start()
    {
        animator = GetComponent<Animator>();

        spawnPosition = transform.position;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // NEU

        currentHealth = maxHealth;
    }
    

    void Update()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        attackTimer -= Time.deltaTime;
        // Bewegung
        float moveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            moveInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            moveInput = 1f;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        //Jump Varianten

        if (Keyboard.current.spaceKey.wasReleasedThisFrame &&
            rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }


        // Spieler drehen
        if (moveInput > 0)
{
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x),transform.localScale.y,transform.localScale.z);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x),transform.localScale.y,transform.localScale.z);
        }
    

        // Ground Check
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Coyote Time
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump Buffer
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Springen
        if (coyoteTimeCounter > 0 && jumpBufferCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            coyoteTimeCounter = 0;
            jumpBufferCounter = 0;
        }

        //Attack
        if (Keyboard.current.jKey.wasPressedThisFrame && attackTimer <= 0)
        {

            animator.SetTrigger("Attack");

            Attack();

            attackTimer = attackCooldown;
        }
    }
    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer | DestroyableLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                Vector2 knockbackDirection =
                    (enemy.transform.position - transform.position)
                    .normalized;

                enemyHealth.TakeDamage(
                    attackDamage,
                    knockbackDirection
                );
            }

            WallHealth wallHealth = enemy.GetComponent<WallHealth>();

            if (wallHealth != null)
            {
                wallHealth.TakeDamage(
                attackDamage
                );
            }
        }
    }
    private void Die()
    {
        Debug.Log("Spieler gestorben");

        currentHealth = maxHealth;
        transform.position = spawnPosition;

        //Destroy(gameObject);
    }
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            knockbackDirection * knockbackForce,
            ForceMode2D.Impulse
        );

        stunTimer = stunDuration;

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRange
        );

    }
}