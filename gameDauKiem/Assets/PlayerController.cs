using Mono.Cecil.Cil;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 400f;

    [Header("Check Ground")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Air Control")]
    public float airControlMultiplier = 0.5f;

    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public int attackDamage = 1;

    private LayerMask groundLayer;  // ← BỎ PUBLIC
    private LayerMask enemyLayer;   // ← BỎ PUBLIC
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private float moveInput;
    private bool isGrounded;
    private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;

        // ✅ FORCE SET LAYERS
        groundLayer = LayerMask.GetMask("Ground");
        enemyLayer = LayerMask.GetMask("Enemy");
        Debug.Log($"✅ Player Ground Layer: {groundLayer.value}, Enemy Layer: {enemyLayer.value}");
    }

    void Update()
    {
        moveInput = 0f;
        if (Keyboard.current.aKey.isPressed) moveInput = -1f;
        if (Keyboard.current.dKey.isPressed) moveInput = 1f;
        Flip(moveInput);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            anim.SetTrigger("Jump");
            Debug.Log($"🔥 JUMP! Velocity Y: {rb.linearVelocity.y}");
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && !isAttacking)
        {
            isAttacking = true;
            anim.SetTrigger("Attack");
            StartCoroutine(ResetAttackAfterDelay(0.6f));
        }

        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetBool("Grounded", isGrounded);
    }

    void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
        else
        {
            float currentX = rb.linearVelocity.x;
            float newX = Mathf.Lerp(currentX, targetSpeed, airControlMultiplier);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }

    void Flip(float moveX)
    {
        if (moveX == 0) return;
        transform.localScale = new Vector3(moveX > 0 ? 1 : -1, 1, 1);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"💔 Player HP: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            Debug.Log("💀 Player Died!");
        }
    }

    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    private IEnumerator ResetAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }

    public void OnAttackFinished()
    {
        isAttacking = false;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
