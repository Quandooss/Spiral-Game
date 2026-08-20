using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.8f;

    private float dashTimer;
    private float dashTime;
    private bool isDashing;

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

    // Größe der rechteckigen Attack-Hitbox
    public Vector2 attackSize = new Vector2(1.2f, 0.8f);

    public int attackDamage = 1;

    public LayerMask enemyLayer;
    public LayerMask DestroyableLayer;

    public float attackCooldown = 0.5f;

    private float attackTimer;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float stunDuration = 0.2f;

    private float stunTimer;

    [Header("Jump Cut")]
    public float jumpCutMultiplier = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isGrounded;

    private Vector3 spawnPosition;


    // =========================
    // Health Properties
    // =========================

    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
    }


    // =========================
    // Start
    // =========================

    void Start()
    {
        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        spawnPosition = transform.position;

        currentHealth = maxHealth;
    }


    // =========================
    // Update
    // =========================

    void Update()
    {
        // =========================
        // Stun
        // =========================

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }


        // =========================
        // Attack Timer
        // =========================

        attackTimer -= Time.deltaTime;


        // =========================
        // Movement Input
        // =========================

        float moveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            moveInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            moveInput = 1f;
        }


        // =========================
        // Movement
        // =========================

        if (Keyboard.current.shiftKey.wasPressedThisFrame)
        {
            moveSpeed = 10f;
        }

        if (Keyboard.current.shiftKey.wasReleasedThisFrame)
        {
            moveSpeed = 5f;
        }

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        // =========================
        // Dash
        // =========================

        dashTimer -= Time.deltaTime;

        if (Keyboard.current.kKey.wasPressedThisFrame && dashTimer <= 0 && !isDashing)
        {
            isDashing = true;
            dashTime = dashDuration;
            dashTimer = dashCooldown;
        }

        if (isDashing)
        {
            dashTime -= Time.deltaTime;

            float dashDirection = transform.localScale.x > 0 ? 1f : -1f;

            rb.linearVelocity = new Vector2(
                dashDirection * dashSpeed,
                0f
            );

            if (dashTime <= 0)
            {
                isDashing = false;
            }

            return;
        }

        // =========================
        // Jump Cut
        // =========================

        if (Keyboard.current.spaceKey.wasReleasedThisFrame &&
            rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }


        // =========================
        // Player Direction
        // =========================

        if (moveInput > 0)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }


        // =========================
        // Ground Check
        // =========================

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );


        // =========================
        // Coyote Time
        // =========================

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }


        // =========================
        // Jump Buffer
        // =========================

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }


        // =========================
        // Jump
        // =========================

        if (coyoteTimeCounter > 0 &&
            jumpBufferCounter > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );

            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );

            coyoteTimeCounter = 0;
            jumpBufferCounter = 0;
        }


        // =========================
        // Attack Input
        // =========================

        if (Keyboard.current.jKey.wasPressedThisFrame &&
            attackTimer <= 0)
        {
            animator.SetTrigger("Attack");

            attackTimer = attackCooldown;
        }
    }


    // =========================
    // Animation Event
    // =========================

    public void AttackHit()
    {
        Debug.Log("Attack wurde ausgelöst");
        Attack();
    }


    // =========================
    // Attack
    // =========================

    private void Attack()
    {
        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(
            attackPoint.position,
            attackSize,
            0f,
            enemyLayer | DestroyableLayer
        );


        foreach (Collider2D hitObject in hitObjects)
        {
            // =========================
            // Enemy
            // =========================

            EnemyHealth enemyHealth =
                hitObject.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                Vector2 knockbackDirection =
                    (
                        hitObject.transform.position -
                        transform.position
                    ).normalized;

                enemyHealth.TakeDamage(
                    attackDamage,
                    knockbackDirection
                );
            }


            // =========================
            // Destroyable Object
            // =========================

            WallHealth wallHealth =
                hitObject.GetComponent<WallHealth>();

            if (wallHealth != null)
            {
                wallHealth.TakeDamage(
                    attackDamage
                );
            }
        }
    }


    // =========================
    // Take Damage
    // =========================

    public void TakeDamage(
        int damage,
        Vector2 knockbackDirection)
    {
        currentHealth -= damage;


        // Stop current movement
        rb.linearVelocity = Vector2.zero;


        // Apply Knockback
        rb.AddForce(
            knockbackDirection * knockbackForce,
            ForceMode2D.Impulse
        );


        // Start Stun
        stunTimer = stunDuration;


        // Check Death
        if (currentHealth <= 0)
        {
            Die();
        }
    }


    // =========================
    // Death
    // =========================

    private void Die()
    {
        Debug.Log("Spieler gestorben");

        currentHealth = maxHealth;

        transform.position = spawnPosition;
    }


    // =========================
    // Debug Gizmos
    // =========================

    private void OnDrawGizmosSelected()
    {
        // Ground Check

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }


        // Attack Hitbox

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireCube(
                attackPoint.position,
                attackSize
            );
        }
    }
}