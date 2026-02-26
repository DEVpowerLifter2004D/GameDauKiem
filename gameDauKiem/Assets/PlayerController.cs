using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;  // Nếu dùng timer, nhưng Event không cần

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 400f;
    [Header("Check Ground")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    [Header("Air Control")]
    public float airControlMultiplier = 0.5f;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private float moveInput;
    private bool isGrounded;
    private bool isAttacking = false;  // ← Thêm cái này

    // 🔥 THÊM VÀO ĐẦU CLASS
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    // 🔥 THÊM VÀO Start() hoặc Awake()
    void Start() // 🔥 THÊM HÀM NÀY
    {
        currentHealth = maxHealth;
    }

    // 🔥 THÊM HÀM NÀY
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player HP: " + currentHealth);
        if (currentHealth <= 0)
        {
            // Die hoặc Respawn
            Debug.Log("Player Died!");
            // UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Restart scene
        }
    }

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask enemyLayer;
    public int attackDamage = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
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
            Debug.Log("Jump triggered! Force: " + (Vector2.up * jumpForce) + " | Velocity Y after: " + rb.linearVelocity.y);
        }

        // Attack với khóa
        if (Mouse.current.leftButton.wasPressedThisFrame && !isAttacking)
        {
            isAttacking = true;
            anim.SetTrigger("Attack");
            StartCoroutine(ResetAttackAfterDelay(0.6f));  // 0.6f = thời gian animation Attack của bạn (kiểm tra Length trong clip Attack.anim)
        }

        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetBool("Grounded", isGrounded);
    }




    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    // Thêm hàm coroutine này ở dưới cùng class (dưới OnDrawGizmos)
    private IEnumerator ResetAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.color = Color.yellow;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

    }

    // Hàm gọi từ Animation Event (cuối clip Attack)
    public void OnAttackFinished()
    {
        isAttacking = false;
    }
}